using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JobHunterDashboard.Models;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace JobHunterDashboard.ViewModels;

public class ChartDataPoint
{
    public string XValue { get; set; } = string.Empty;
    public double YValue { get; set; }
}

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly IAnalyticsService _analyticsService;

    // --- Tab selection ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsStatisticsVisible))]
    [NotifyPropertyChangedFor(nameof(IsRegressionVisible))]
    [NotifyPropertyChangedFor(nameof(IsMLVisible))]
    private int _selectedTabIndex;

    public bool IsStatisticsVisible => SelectedTabIndex == 0;
    public bool IsRegressionVisible => SelectedTabIndex == 1;
    public bool IsMLVisible => SelectedTabIndex == 2;

    // --- Configuration panel ---
    [ObservableProperty]
    private bool _isConfigOpen = true;

    // --- Table selection ---
    public ObservableCollection<string> Tables { get; } = new();

    [ObservableProperty]
    private string? _selectedPrimaryTable;

    partial void OnSelectedPrimaryTableChanged(string? value)
    {
        _ = LoadColumnsForPrimaryTableAsync();
    }

    public ObservableCollection<string> PrimaryColumns { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsJoinConfigVisible))]
    private string? _selectedJoinTable;

    partial void OnSelectedJoinTableChanged(string? value)
    {
        _ = LoadColumnsForJoinTableAsync();
    }

    public ObservableCollection<string> JoinColumns { get; } = new();
    public bool IsJoinConfigVisible => !string.IsNullOrEmpty(SelectedJoinTable) && SelectedJoinTable != "(None)";

    // --- Join config ---
    public ObservableCollection<string> JoinTypes { get; } = new() { "Inner", "Left", "Right", "Full" };

    [ObservableProperty]
    private string _selectedJoinType = "Inner";

    [ObservableProperty]
    private string? _joinColumnLeft;

    [ObservableProperty]
    private string? _joinColumnRight;

    // --- Axis & Aggregate ---
    public ObservableCollection<string> AllAvailableColumns { get; } = new();

    [ObservableProperty]
    private string? _selectedXAxis;

    [ObservableProperty]
    private string? _selectedYAxis;

    public ObservableCollection<string> AggregateFunctions { get; } = new() { "None", "Count", "Sum", "Avg", "Min", "Max" };

    [ObservableProperty]
    private string _selectedAggregate = "Count";

    // --- Chart type ---
    public ObservableCollection<string> ChartTypes { get; } = new() { "Bar", "Column", "Line", "Pie", "Doughnut", "Scatter" };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCartesianChart))]
    [NotifyPropertyChangedFor(nameof(IsCircularChart))]
    private string _selectedChartType = "Column";

    public bool IsCartesianChart => SelectedChartType != "Pie" && SelectedChartType != "Doughnut";
    public bool IsCircularChart => SelectedChartType == "Pie" || SelectedChartType == "Doughnut";

    // --- Chart data ---
    public ObservableCollection<ChartDataPoint> ChartData { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    private bool _hasData;

    public AnalyticsViewModel(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
        _ = LoadTablesAsync();
    }

    [RelayCommand]
    private void SelectTab(string index)
    {
        if (int.TryParse(index, out var i))
            SelectedTabIndex = i;
    }

    [RelayCommand]
    private void ToggleConfig() => IsConfigOpen = !IsConfigOpen;

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

    [RelayCommand]
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

            MainThread.BeginInvokeOnMainThread(() =>
            {
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
            });
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
}

