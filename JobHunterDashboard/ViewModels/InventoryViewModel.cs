using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Models;
using JobHunterDashboard.Models;

namespace JobHunterDashboard.ViewModels;

public partial class InventoryViewModel : ObservableObject
{
    private readonly IStockItemService _service;

    [ObservableProperty]
    private ObservableCollection<StockItem> _stockItems = new();

    [ObservableProperty]
    private ObservableCollection<StockType> _stockTypesList = new();

    [ObservableProperty]
    private ObservableCollection<Material> _materials = new();

    // ── Filter collections ──
    public ObservableCollection<FilterItem> StockTypeFilters { get; } = new();
    public ObservableCollection<FilterItem> MaterialCategoryFilters { get; } = new();

    // ── Search ──
    [ObservableProperty]
    private string _searchText = "";

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadStockItemsAsync();
    }

    // ── Sort ──
    [ObservableProperty]
    private string _selectedSort = "Default";

    partial void OnSelectedSortChanged(string value)
    {
        _ = LoadStockItemsAsync();
    }

    public List<string> SortOptions { get; } = new()
    {
        "Default",
        "Material A→Z",
        "Material Z→A",
        "Type A→Z",
        "Type Z→A",
        "Qty Low→High",
        "Qty High→Low",
        "Length Short→Long",
        "Length Long→Short"
    };

    // ── Dropdown visibility ──
    [ObservableProperty]
    private bool _isTypeDropdownOpen;

    [ObservableProperty]
    private bool _isMaterialDropdownOpen;

    // ── Paging ──
    private int _currentPage = 1;
    private const int PageSize = 30;

    // ── Create / Edit form state ──
    [ObservableProperty]
    private bool _isFormVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    private bool _isEditing;

    public string FormTitle => IsEditing ? "Edit Stock Item" : "Add New Stock";

    private int _editingId;

    [ObservableProperty]
    private StockType? _formStockType;

    [ObservableProperty]
    private Material? _formMaterial;

    [ObservableProperty]
    private string _formOD = "";

    [ObservableProperty]
    private string _formID = "";

    [ObservableProperty]
    private string _formWall = "";

    [ObservableProperty]
    private string _formWidth = "";

    [ObservableProperty]
    private string _formHeight = "";

    [ObservableProperty]
    private string _formThickness = "";

    [ObservableProperty]
    private string _formLength = "";

    [ObservableProperty]
    private string _formAcrossFlats = "";

    [ObservableProperty]
    private string _formLegLength = "";

    [ObservableProperty]
    private string _formLegWidth = "";

    [ObservableProperty]
    private string _formFlangeWidth = "";

    [ObservableProperty]
    private string _formWebDepth = "";

    [ObservableProperty]
    private string _formQuantity = "1";

    [ObservableProperty]
    private string _formLocation = "";

    // ── Materials service (injected separately for the form picker) ──
    private readonly JobHunter.Domain.Interfaces.IMaterialRepository _materialRepo;
    private readonly IStockTypeService _stockTypeService;

    public InventoryViewModel(IStockItemService service, JobHunter.Domain.Interfaces.IMaterialRepository materialRepo, IStockTypeService stockTypeService)
    {
        _service = service;
        _materialRepo = materialRepo;
        _stockTypeService = stockTypeService;

        // Initialize Material Category filters
        var categories = new[] { "Aluminum", "Titanium", "Nickel", "Steel", "Copper", "Magnesium" };
        foreach (var c in categories)
        {
            var item = new FilterItem { Name = c, IsSelected = true };
            item.PropertyChanged += FilterChanged;
            MaterialCategoryFilters.Add(item);
        }

        _ = InitializeAsync();
    }

    [RelayCommand]
    private void ToggleTypeDropdown()
    {
        IsTypeDropdownOpen = !IsTypeDropdownOpen;
        if (IsTypeDropdownOpen) IsMaterialDropdownOpen = false;
    }

    [RelayCommand]
    private void ToggleMaterialDropdown()
    {
        IsMaterialDropdownOpen = !IsMaterialDropdownOpen;
        if (IsMaterialDropdownOpen) IsTypeDropdownOpen = false;
    }

    [RelayCommand]
    private async Task LoadMoreAsync() => await LoadStockItemsAsync(true);

    [RelayCommand]
    private async Task RefreshAsync() => await LoadStockItemsAsync();

    [RelayCommand]
    private void ShowCreateForm()
    {
        IsEditing = false;
        _editingId = 0;
        ClearForm();
        IsFormVisible = true;
    }

    [RelayCommand]
    private void CancelForm()
    {
        IsFormVisible = false;
    }

    [RelayCommand]
    private async Task SaveFormAsync() => await SaveStockItemAsync();

    [RelayCommand]
    private async Task EditAsync(StockItem item) => await EditStockItemAsync(item);

    [RelayCommand]
    private async Task DeleteAsync(StockItem item) => await DeleteStockItemAsync(item);

    private async Task InitializeAsync()
    {
        // Load materials for the form picker
        var mats = await _materialRepo.GetAllAsync();
        Materials = new ObservableCollection<Material>(mats);

        var stypes = await _stockTypeService.GetAllAsync();
        StockTypesList = new ObservableCollection<StockType>(stypes.OrderBy(s => s.Name));

        // Initialize Stock Type filters
        foreach (var t in StockTypesList)
        {
            var item = new FilterItem { Name = t.Name, IsSelected = true };
            item.PropertyChanged += FilterChanged;
            StockTypeFilters.Add(item);
        }

        // Load stock items
        await LoadStockItemsAsync();
    }

    private void FilterChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterItem.IsSelected))
        {
            _ = LoadStockItemsAsync();
        }
    }

    private string? MapSortOption(string display) => display switch
    {
        "Material A→Z" => "MaterialAsc",
        "Material Z→A" => "MaterialDesc",
        "Type A→Z" => "TypeAsc",
        "Type Z→A" => "TypeDesc",
        "Qty Low→High" => "QuantityAsc",
        "Qty High→Low" => "QuantityDesc",
        "Length Short→Long" => "LengthAsc",
        "Length Long→Short" => "LengthDesc",
        _ => null
    };

    private async Task LoadStockItemsAsync(bool isLoadMore = false)
    {
        try
        {
            if (!isLoadMore)
            {
                _currentPage = 1;
                StockItems.Clear();
            }

            var typeFilters = StockTypeFilters.Where(x => x.IsSelected).Select(x => x.Name).ToList();
            var matCategoryFilters = MaterialCategoryFilters.Where(x => x.IsSelected).Select(x => x.Name).ToList();

            var items = await _service.GetFilteredAsync(
                SearchText,
                typeFilters,
                matCategoryFilters,
                _currentPage,
                PageSize,
                MapSortOption(SelectedSort));

            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var item in items)
                {
                    StockItems.Add(item);
                }
            });

            if (items.Any())
            {
                _currentPage++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading stock items: {ex.Message}");
        }
    }

    private void ClearForm()
    {
        FormStockType = StockTypesList.FirstOrDefault();
        FormMaterial = Materials.FirstOrDefault();
        FormOD = ""; FormID = ""; FormWall = "";
        FormWidth = ""; FormHeight = ""; FormThickness = "";
        FormLength = ""; FormAcrossFlats = "";
        FormLegLength = ""; FormLegWidth = "";
        FormFlangeWidth = ""; FormWebDepth = "";
        FormQuantity = "1"; FormLocation = "";
    }

    private async Task SaveStockItemAsync()
    {
        try
        {
            if (FormMaterial == null)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Validation", "Please select a material.", "OK");
                return;
            }

            if (FormStockType == null)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Validation", "Please select a stock type.", "OK");
                return;
            }

            var item = new StockItem
            {
                Id = _editingId,
                StockTypeId = FormStockType.Id,
                MaterialId = FormMaterial.Id,
                OutsideDiameter = ParseDecimal(FormOD),
                InsideDiameter = ParseDecimal(FormID),
                WallThickness = ParseDecimal(FormWall),
                Width = ParseDecimal(FormWidth),
                Height = ParseDecimal(FormHeight),
                Thickness = ParseDecimal(FormThickness),
                Length = ParseDecimal(FormLength),
                AcrossFlats = ParseDecimal(FormAcrossFlats),
                LegLength = ParseDecimal(FormLegLength),
                LegWidth = ParseDecimal(FormLegWidth),
                FlangeWidth = ParseDecimal(FormFlangeWidth),
                WebDepth = ParseDecimal(FormWebDepth),
                Quantity = int.TryParse(FormQuantity, out var q) ? q : 1,
                Location = string.IsNullOrWhiteSpace(FormLocation) ? null : FormLocation.Trim()
            };

            if (IsEditing)
            {
                await _service.UpdateAsync(item);
            }
            else
            {
                await _service.AddAsync(item);
            }

            IsFormVisible = false;
            await LoadStockItemsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving stock item: {ex.Message}");
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save: {ex.Message}", "OK");
        }
    }

    private async Task EditStockItemAsync(StockItem item)
    {
        if (item == null) return;

        IsEditing = true;
        _editingId = item.Id;
        FormStockType = StockTypesList.FirstOrDefault(s => s.Id == item.StockTypeId);
        FormMaterial = Materials.FirstOrDefault(m => m.Id == item.MaterialId);
        FormOD = item.OutsideDiameter?.ToString() ?? "";
        FormID = item.InsideDiameter?.ToString() ?? "";
        FormWall = item.WallThickness?.ToString() ?? "";
        FormWidth = item.Width?.ToString() ?? "";
        FormHeight = item.Height?.ToString() ?? "";
        FormThickness = item.Thickness?.ToString() ?? "";
        FormLength = item.Length?.ToString() ?? "";
        FormAcrossFlats = item.AcrossFlats?.ToString() ?? "";
        FormLegLength = item.LegLength?.ToString() ?? "";
        FormLegWidth = item.LegWidth?.ToString() ?? "";
        FormFlangeWidth = item.FlangeWidth?.ToString() ?? "";
        FormWebDepth = item.WebDepth?.ToString() ?? "";
        FormQuantity = item.Quantity.ToString();
        FormLocation = item.Location ?? "";
        IsFormVisible = true;

        await Task.CompletedTask;
    }

    private async Task DeleteStockItemAsync(StockItem item)
    {
        if (item == null) return;

        if (Application.Current?.MainPage != null)
        {
            bool confirmed = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Delete {item.StockType} — {item.Material?.Name}?\n{item.DimensionSummary}",
                "Delete", "Cancel");

            if (!confirmed) return;
        }

        try
        {
            await _service.DeleteAsync(item.Id);
            StockItems.Remove(item);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting stock item: {ex.Message}");
        }
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return decimal.TryParse(value, out var result) ? result : null;
    }
}
