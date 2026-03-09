using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JobHunter.Application.Interfaces;
using JobHunter.Domain.Models;
using JobHunterDashboard.Models;

namespace JobHunterDashboard.ViewModels;

public class InventoryViewModel : INotifyPropertyChanged
{
    private readonly IStockItemService _service;

    private ObservableCollection<StockItem> _stockItems = new();
    public ObservableCollection<StockItem> StockItems
    {
        get => _stockItems;
        set { _stockItems = value; OnPropertyChanged(); }
    }

    private ObservableCollection<StockType> _stockTypesList = new();
    public ObservableCollection<StockType> StockTypesList
    {
        get => _stockTypesList;
        set { _stockTypesList = value; OnPropertyChanged(); }
    }

    private ObservableCollection<Material> _materials = new();
    public ObservableCollection<Material> Materials
    {
        get => _materials;
        set { _materials = value; OnPropertyChanged(); }
    }

    // ── Filter collections ──
    public ObservableCollection<FilterItem> StockTypeFilters { get; } = new();
    public ObservableCollection<FilterItem> MaterialCategoryFilters { get; } = new();

    // ── Search ──
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                _ = LoadStockItemsAsync();
            }
        }
    }

    // ── Sort ──
    private string _selectedSort = "Default";
    public string SelectedSort
    {
        get => _selectedSort;
        set
        {
            if (_selectedSort != value)
            {
                _selectedSort = value;
                OnPropertyChanged();
                _ = LoadStockItemsAsync();
            }
        }
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
    private bool _isTypeDropdownOpen;
    public bool IsTypeDropdownOpen
    {
        get => _isTypeDropdownOpen;
        set { _isTypeDropdownOpen = value; OnPropertyChanged(); }
    }

    private bool _isMaterialDropdownOpen;
    public bool IsMaterialDropdownOpen
    {
        get => _isMaterialDropdownOpen;
        set { _isMaterialDropdownOpen = value; OnPropertyChanged(); }
    }

    // ── Paging ──
    private int _currentPage = 1;
    private const int PageSize = 30;

    // ── Create / Edit form state ──
    private bool _isFormVisible;
    public bool IsFormVisible
    {
        get => _isFormVisible;
        set { _isFormVisible = value; OnPropertyChanged(); }
    }

    private bool _isEditing;
    public bool IsEditing
    {
        get => _isEditing;
        set { _isEditing = value; OnPropertyChanged(); OnPropertyChanged(nameof(FormTitle)); }
    }

    public string FormTitle => IsEditing ? "Edit Stock Item" : "Add New Stock";

    private int _editingId;

    private StockType? _formStockType;
    public StockType? FormStockType
    {
        get => _formStockType;
        set { _formStockType = value; OnPropertyChanged(); }
    }

    private Material? _formMaterial;
    public Material? FormMaterial
    {
        get => _formMaterial;
        set { _formMaterial = value; OnPropertyChanged(); }
    }

    private string _formOD = "";
    public string FormOD { get => _formOD; set { _formOD = value; OnPropertyChanged(); } }

    private string _formID = "";
    public string FormID { get => _formID; set { _formID = value; OnPropertyChanged(); } }

    private string _formWall = "";
    public string FormWall { get => _formWall; set { _formWall = value; OnPropertyChanged(); } }

    private string _formWidth = "";
    public string FormWidth { get => _formWidth; set { _formWidth = value; OnPropertyChanged(); } }

    private string _formHeight = "";
    public string FormHeight { get => _formHeight; set { _formHeight = value; OnPropertyChanged(); } }

    private string _formThickness = "";
    public string FormThickness { get => _formThickness; set { _formThickness = value; OnPropertyChanged(); } }

    private string _formLength = "";
    public string FormLength { get => _formLength; set { _formLength = value; OnPropertyChanged(); } }

    private string _formAcrossFlats = "";
    public string FormAcrossFlats { get => _formAcrossFlats; set { _formAcrossFlats = value; OnPropertyChanged(); } }

    private string _formLegLength = "";
    public string FormLegLength { get => _formLegLength; set { _formLegLength = value; OnPropertyChanged(); } }

    private string _formLegWidth = "";
    public string FormLegWidth { get => _formLegWidth; set { _formLegWidth = value; OnPropertyChanged(); } }

    private string _formFlangeWidth = "";
    public string FormFlangeWidth { get => _formFlangeWidth; set { _formFlangeWidth = value; OnPropertyChanged(); } }

    private string _formWebDepth = "";
    public string FormWebDepth { get => _formWebDepth; set { _formWebDepth = value; OnPropertyChanged(); } }

    private string _formQuantity = "1";
    public string FormQuantity { get => _formQuantity; set { _formQuantity = value; OnPropertyChanged(); } }

    private string _formLocation = "";
    public string FormLocation { get => _formLocation; set { _formLocation = value; OnPropertyChanged(); } }

    // ── Commands ──
    public ICommand ToggleTypeDropdownCommand { get; }
    public ICommand ToggleMaterialDropdownCommand { get; }
    public ICommand LoadMoreCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ShowCreateFormCommand { get; }
    public ICommand CancelFormCommand { get; }
    public ICommand SaveFormCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }

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

        ToggleTypeDropdownCommand = new Command(() =>
        {
            IsTypeDropdownOpen = !IsTypeDropdownOpen;
            if (IsTypeDropdownOpen) IsMaterialDropdownOpen = false;
        });

        ToggleMaterialDropdownCommand = new Command(() =>
        {
            IsMaterialDropdownOpen = !IsMaterialDropdownOpen;
            if (IsMaterialDropdownOpen) IsTypeDropdownOpen = false;
        });

        LoadMoreCommand = new Command(async () => await LoadStockItemsAsync(true));
        RefreshCommand = new Command(async () => await LoadStockItemsAsync());

        ShowCreateFormCommand = new Command(() =>
        {
            IsEditing = false;
            _editingId = 0;
            ClearForm();
            IsFormVisible = true;
        });

        CancelFormCommand = new Command(() =>
        {
            IsFormVisible = false;
        });

        SaveFormCommand = new Command(async () => await SaveStockItemAsync());
        EditCommand = new Command<StockItem>(async (item) => await EditStockItemAsync(item));
        DeleteCommand = new Command<StockItem>(async (item) => await DeleteStockItemAsync(item));

        _ = InitializeAsync();
    }

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
        "Material A→Z"       => "MaterialAsc",
        "Material Z→A"       => "MaterialDesc",
        "Type A→Z"           => "TypeAsc",
        "Type Z→A"           => "TypeDesc",
        "Qty Low→High"       => "QuantityAsc",
        "Qty High→Low"       => "QuantityDesc",
        "Length Short→Long"  => "LengthAsc",
        "Length Long→Short"  => "LengthDesc",
        _                    => null
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

            foreach (var item in items)
            {
                StockItems.Add(item);
            }

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

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
