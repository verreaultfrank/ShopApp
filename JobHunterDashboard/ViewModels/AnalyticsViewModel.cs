using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JobHunterDashboard.Models;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Models;

namespace JobHunterDashboard.ViewModels;

public class ChartDataPoint
{
    public string XValue { get; set; } = string.Empty;
    public double YValue { get; set; }
}

public class AnalyticsViewModel : INotifyPropertyChanged
{
    private readonly IAnalyticsService _analyticsService;

    // --- Tab selection ---
    private int _selectedTabIndex;
    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set { _selectedTabIndex = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsStatisticsVisible)); OnPropertyChanged(nameof(IsRegressionVisible)); OnPropertyChanged(nameof(IsMLVisible)); }
    }

    public bool IsStatisticsVisible => SelectedTabIndex == 0;
    public bool IsRegressionVisible => SelectedTabIndex == 1;
    public bool IsMLVisible => SelectedTabIndex == 2;

    // --- Configuration panel ---
    private bool _isConfigOpen = true;
    public bool IsConfigOpen
    {
        get => _isConfigOpen;
        set { _isConfigOpen = value; OnPropertyChanged(); }
    }

    // --- Table selection ---
    public ObservableCollection<string> Tables { get; } = new();

    private string? _selectedPrimaryTable;
    public string? SelectedPrimaryTable
    {
        get => _selectedPrimaryTable;
        set
        {
            if (_selectedPrimaryTable != value)
            {
                _selectedPrimaryTable = value;
                OnPropertyChanged();
                _ = LoadColumnsForPrimaryTableAsync();
            }
        }
    }

    public ObservableCollection<string> PrimaryColumns { get; } = new();

    private string? _selectedJoinTable;
    public string? SelectedJoinTable
    {
        get => _selectedJoinTable;
        set
        {
            if (_selectedJoinTable != value)
            {
                _selectedJoinTable = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsJoinConfigVisible));
                _ = LoadColumnsForJoinTableAsync();
            }
        }
    }

    public ObservableCollection<string> JoinColumns { get; } = new();
    public bool IsJoinConfigVisible => !string.IsNullOrEmpty(SelectedJoinTable) && SelectedJoinTable != "(None)";

    // --- Join config ---
    public ObservableCollection<string> JoinTypes { get; } = new() { "Inner", "Left", "Right", "Full" };

    private string _selectedJoinType = "Inner";
    public string SelectedJoinType
    {
        get => _selectedJoinType;
        set { _selectedJoinType = value; OnPropertyChanged(); }
    }

    private string? _joinColumnLeft;
    public string? JoinColumnLeft
    {
        get => _joinColumnLeft;
        set { _joinColumnLeft = value; OnPropertyChanged(); }
    }

    private string? _joinColumnRight;
    public string? JoinColumnRight
    {
        get => _joinColumnRight;
        set { _joinColumnRight = value; OnPropertyChanged(); }
    }

    // --- Axis & Aggregate ---
    public ObservableCollection<string> AllAvailableColumns { get; } = new();

    private string? _selectedXAxis;
    public string? SelectedXAxis
    {
        get => _selectedXAxis;
        set { _selectedXAxis = value; OnPropertyChanged(); }
    }

    private string? _selectedYAxis;
    public string? SelectedYAxis
    {
        get => _selectedYAxis;
        set { _selectedYAxis = value; OnPropertyChanged(); }
    }

    public ObservableCollection<string> AggregateFunctions { get; } = new() { "None", "Count", "Sum", "Avg", "Min", "Max" };

    private string _selectedAggregate = "Count";
    public string SelectedAggregate
    {
        get => _selectedAggregate;
        set { _selectedAggregate = value; OnPropertyChanged(); }
    }

    // --- Chart type ---
    public ObservableCollection<string> ChartTypes { get; } = new() { "Bar", "Column", "Line", "Pie", "Doughnut", "Scatter" };

    private string _selectedChartType = "Column";
    public string SelectedChartType
    {
        get => _selectedChartType;
        set
        {
            _selectedChartType = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCartesianChart));
            OnPropertyChanged(nameof(IsCircularChart));
        }
    }

    public bool IsCartesianChart => SelectedChartType != "Pie" && SelectedChartType != "Doughnut";
    public bool IsCircularChart => SelectedChartType == "Pie" || SelectedChartType == "Doughnut";

    // --- Chart data ---
    public ObservableCollection<ChartDataPoint> ChartData { get; } = new();

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
    }

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    private bool _hasData;
    public bool HasData
    {
        get => _hasData;
        set { _hasData = value; OnPropertyChanged(); }
    }

    // --- Commands ---
    public ICommand SelectTabCommand { get; }
    public ICommand ToggleConfigCommand { get; }
    public ICommand RunQueryCommand { get; }

    public AnalyticsViewModel(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;

        SelectTabCommand = new Command<string>(index =>
        {
            if (int.TryParse(index, out var i))
                SelectedTabIndex = i;
        });

        ToggleConfigCommand = new Command(() => IsConfigOpen = !IsConfigOpen);
        RunQueryCommand = new Command(async () => await ExecuteQueryAsync());

        _ = LoadTablesAsync();
    }

    private async Task LoadTablesAsync()
    {
        try
        {
            var tables = await _analyticsService.GetTablesAsync();
            Tables.Clear();
            Tables.Add("(None)"); // For join table "no selection"
            foreach (var t in tables)
                Tables.Add(t);
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load tables: {ex.Message}";
        }
    }

    private async Task LoadColumnsForPrimaryTableAsync()
    {
        PrimaryColumns.Clear();
        AllAvailableColumns.Clear();
        if (string.IsNullOrEmpty(SelectedPrimaryTable) || SelectedPrimaryTable == "(None)") return;

        try
        {
            var cols = await _analyticsService.GetColumnsAsync(SelectedPrimaryTable);
            foreach (var c in cols)
            {
                PrimaryColumns.Add(c);
                AllAvailableColumns.Add(c);
            }
            // Also add join table columns if selected
            await AppendJoinColumnsToAllAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load columns: {ex.Message}";
        }
    }

    private async Task LoadColumnsForJoinTableAsync()
    {
        JoinColumns.Clear();
        if (string.IsNullOrEmpty(SelectedJoinTable) || SelectedJoinTable == "(None)")
        {
            await RebuildAllAvailableColumnsAsync();
            return;
        }

        try
        {
            var cols = await _analyticsService.GetColumnsAsync(SelectedJoinTable);
            foreach (var c in cols)
                JoinColumns.Add(c);
            await RebuildAllAvailableColumnsAsync();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to load join columns: {ex.Message}";
        }
    }

    private Task RebuildAllAvailableColumnsAsync()
    {
        AllAvailableColumns.Clear();
        foreach (var c in PrimaryColumns)
            AllAvailableColumns.Add(c);
        foreach (var c in JoinColumns)
        {
            if (!AllAvailableColumns.Contains(c))
                AllAvailableColumns.Add(c);
        }
        return Task.CompletedTask;
    }

    private Task AppendJoinColumnsToAllAsync()
    {
        foreach (var c in JoinColumns)
        {
            if (!AllAvailableColumns.Contains(c))
                AllAvailableColumns.Add(c);
        }
        return Task.CompletedTask;
    }

    private async Task ExecuteQueryAsync()
    {
        ErrorMessage = null;
        HasData = false;

        if (string.IsNullOrEmpty(SelectedPrimaryTable) || SelectedPrimaryTable == "(None)")
        {
            ErrorMessage = "Please select a primary table.";
            return;
        }

        if (string.IsNullOrEmpty(SelectedXAxis))
        {
            ErrorMessage = "Please select an X-axis column.";
            return;
        }

        IsLoading = true;
        try
        {
            var joinTable = (SelectedJoinTable == "(None)") ? null : SelectedJoinTable;

            var config = new AnalyticsQueryConfig
            {
                PrimaryTable = SelectedPrimaryTable,
                JoinTable = joinTable,
                JoinType = Enum.TryParse<JoinType>(SelectedJoinType, out var jt) ? jt : JoinType.Inner,
                JoinColumnLeft = JoinColumnLeft,
                JoinColumnRight = JoinColumnRight,
                XAxisColumn = SelectedXAxis,
                YAxisColumn = SelectedYAxis ?? "",
                AggregateFunction = Enum.TryParse<AggregateFunction>(SelectedAggregate, out var af) ? af : AggregateFunction.Count
            };

            var results = await _analyticsService.ExecuteQueryAsync(config);

            ChartData.Clear();
            foreach (var row in results)
            {
                var xVal = row.ContainsKey("XValue") ? row["XValue"]?.ToString() ?? "(null)" : "(null)";
                double yVal = 0;
                if (row.ContainsKey("YValue") && row["YValue"] != DBNull.Value)
                {
                    double.TryParse(row["YValue"]?.ToString(), out yVal);
                }

                ChartData.Add(new ChartDataPoint { XValue = xVal, YValue = yVal });
            }

            HasData = ChartData.Count > 0;
            if (!HasData)
                ErrorMessage = "Query returned no results.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Query error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
