using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using JobHunter.Domain.Models;
using JobHunter.Domain.Interfaces;
using JobHunter.Application.Interfaces;
using JobHunterDashboard.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobHunter.Infrastructure.Services;

namespace JobHunterDashboard.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IEmailService _emailService;
    private readonly IJobLeadRepository _repository;
    private readonly ILeadWorkflowService _workflowService;
    private readonly IJobStatusRepository _statusRepository;
    private readonly IBusinessService _businessService;

    [ObservableProperty]
    private ObservableCollection<DashboardLead> _jobs = new();

    public ObservableCollection<FilterItem> ProviderFilters { get; } = new();
    public ObservableCollection<FilterItem> StatusFilters { get; } = new();
    public ObservableCollection<LeadStatus> AvailableStatuses { get; } = new();
    public ObservableCollection<Business> AvailableBusinesses { get; } = new();

    [ObservableProperty]
    private int _currentPage = 1;
    private const int PageSize = 20;

    [ObservableProperty]
    private LeadSortOption _selectedSortOption = LeadSortOption.NewestFirst;

    partial void OnSelectedSortOptionChanged(LeadSortOption value)
    {
        _ = LoadLeadsFromDatabaseAsync();
    }

    public Array SortOptions => Enum.GetValues(typeof(LeadSortOption));

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    private bool _isProviderDropdownOpen;

    [ObservableProperty]
    private bool _isStatusDropdownOpen;

    [ObservableProperty]
    private string _searchText = "";

    private bool _isInitialized;

    partial void OnSearchTextChanged(string value)
    {
        Preferences.Default.Set("SearchText", value);
        _ = LoadLeadsFromDatabaseAsync();
    }

    public MainViewModel(IEmailService emailService, IJobLeadRepository repository, ILeadWorkflowService workflowService, IJobStatusRepository statusRepository, IBusinessService businessService)
    {
        _emailService = emailService;
        _repository = repository;
        _workflowService = workflowService;
        _statusRepository = statusRepository;
        _businessService = businessService;

        // Load persisted filter preferences (or default to All/Empty)
        _searchText = Preferences.Default.Get("SearchText", "");
    }

    [RelayCommand]
    private void ToggleProviderDropdown()
    {
        IsProviderDropdownOpen = !IsProviderDropdownOpen;
        if (IsProviderDropdownOpen) IsStatusDropdownOpen = false;
    }

    [RelayCommand]
    private void ToggleStatusDropdown()
    {
        IsStatusDropdownOpen = !IsStatusDropdownOpen;
        if (IsStatusDropdownOpen) IsProviderDropdownOpen = false;
    }

    [RelayCommand]
    private async Task ViewJob(string? url)
    {
        if (!string.IsNullOrEmpty(url))
            await Launcher.Default.OpenAsync(url);
        else if (Application.Current?.MainPage != null)
            await Application.Current.MainPage.DisplayAlert("Error", "No URL is available for this job lead.", "OK");
    }

    [RelayCommand]
    private async Task FetchLeads() => await FetchLeadsAsync();

    [RelayCommand]
    private async Task LoadMore() => await LoadLeadsFromDatabaseAsync(true);

    [RelayCommand]
    private async Task OpenLeadFolder(DashboardLead job) => await OpenLeadFolderAsync(job);

    [RelayCommand]
    private async Task ImportQuote(DashboardLead job) => await ImportQuoteAsync(job);

    [RelayCommand]
    private async Task GenerateDummyQuote(DashboardLead job) => await GenerateDummyQuoteAsync(job);

    [RelayCommand]
    private async Task ViewHistory(DashboardLead job) => await ViewHistoryAsync(job);

    public async Task InitializeAsync()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        // Load businesses from DB for provider filters and business picker
        var businessesFromDb = await _businessService.GetAllBusinessesAsync();
        string savedProviders = Preferences.Default.Get("SelectedProviders", "");
        var selectedProviderList = savedProviders.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        foreach (var b in businessesFromDb)
        {
            AvailableBusinesses.Add(b);
            var item = new FilterItem { Name = b.Name, IsSelected = string.IsNullOrEmpty(savedProviders) || selectedProviderList.Contains(b.Name) };
            item.PropertyChanged += FilterItem_PropertyChanged;
            ProviderFilters.Add(item);
        }

        // Load statuses from DB
        var statusesFromDb = await _statusRepository.GetAllStatusesAsync();
        string savedStatuses = Preferences.Default.Get("SelectedStatuses", "");
        var selectedStatusList = savedStatuses.Split(',', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        foreach (var s in statusesFromDb)
        {
            AvailableStatuses.Add(s);
            var item = new FilterItem { Name = s.Name, IsSelected = string.IsNullOrEmpty(savedStatuses) || selectedStatusList.Contains(s.Name) };
            item.PropertyChanged += FilterItem_PropertyChanged;
            StatusFilters.Add(item);
        }

        await _workflowService.InitializeFoldersForExistingLeadsAsync();
        await LoadLeadsFromDatabaseAsync();
    }

    private void FilterItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterItem.IsSelected))
        {
            var pList = ProviderFilters.Where(x => x.IsSelected).Select(x => x.Name).ToList();
            Preferences.Default.Set("SelectedProviders", string.Join(",", pList));

            var sList = StatusFilters.Where(x => x.IsSelected).Select(x => x.Name).ToList();
            Preferences.Default.Set("SelectedStatuses", string.Join(",", sList));

            _ = LoadLeadsFromDatabaseAsync();
        }
    }

    private async Task LoadLeadsFromDatabaseAsync(bool isLoadMore = false)
    {
        try
        {
            if (!isLoadMore)
            {
                CurrentPage = 1;
                Jobs.Clear();
            }

            var pList = ProviderFilters.Where(x => x.IsSelected).Select(x => x.Name).ToList();
            var sList = StatusFilters.Where(x => x.IsSelected).Select(x => x.Name).ToList();

            var domainLeads = await _repository.GetLeadsAsync(SearchText, pList, sList, CurrentPage, PageSize, SelectedSortOption);

            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Map Domain models to Presentation models (DashboardJobOpportunity)
                foreach (var lead in domainLeads)
                {
                    var dashboardJob = MapToDashboard(lead);

                    // MAUI Picker Reference Binding Bug workaround:
                    // Overriding Equals in Domain models is not always respected by bound DataTemplates.
                    // We must explicitly align the object reference pointers generated from the database fetch
                    // with the exact references held in the `Available` collections that act as the Pickers' ItemsSource.
                    if (dashboardJob.BusinessId.HasValue)
                    {
                        dashboardJob.Business = AvailableBusinesses.FirstOrDefault(b => b.Id == dashboardJob.BusinessId) ?? dashboardJob.Business;
                        if (dashboardJob.PreferredContactId.HasValue && dashboardJob.Business != null)
                        {
                            dashboardJob.PreferredContact = dashboardJob.Business.Contacts.FirstOrDefault(c => c.Id == dashboardJob.PreferredContactId) ?? dashboardJob.PreferredContact;
                        }
                    }

                    var latestHistory = dashboardJob.StatusHistories?.OrderByDescending(x => x.Date).FirstOrDefault();
                    if (latestHistory?.Status != null)
                    {
                        latestHistory.Status = AvailableStatuses.FirstOrDefault(s => s.Id == latestHistory.Status.Id) ?? latestHistory.Status;
                    }

                    Jobs.Add(dashboardJob);
                }
            });

            if (domainLeads.Any())
            {
                CurrentPage++;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading from DB: {ex.Message}");
            ErrorMessage = $"Failed to load leads from database: {ex.Message}";
        }
    }

    /// <summary>
    /// Maps a Domain JobOpportunity to a Presentation DashboardJobOpportunity.
    /// This mapping is the Presentation layer's responsibility.
    /// </summary>
    private static DashboardLead MapToDashboard(Lead source)
    {
        return new DashboardLead
        {
            Id = source.Id,
            Title = source.Title,
            Url = source.Url,
            ClosingDate = source.ClosingDate,
            IsAutomationPossible = source.IsAutomationPossible,
            DateFetched = source.DateFetched,
            Description = source.Description,
            MachiningProcessInferred = source.MachiningProcessInferred,
            EstimatedValue = source.EstimatedValue,
            AskedPrice = source.AskedPrice,
            PublishedDate = source.PublishedDate,
            Metadata = source.Metadata,
            ProviderJobId = source.ProviderJobId,
            BusinessId = source.BusinessId,
            Business = source.Business,
            PreferredContactId = source.PreferredContactId,
            PreferredContact = source.PreferredContact,
            StatusHistories = source.StatusHistories
        };
    }

    public async Task ChangeBusinessAsync(DashboardLead job, Business? newBusiness)
    {
        if (job == null) return;
        if (job.BusinessId == newBusiness?.Id) return;

        job.BusinessId = newBusiness?.Id;
        job.Business = newBusiness;
        // Clear preferred contact when business changes
        job.PreferredContactId = null;
        job.PreferredContact = null;

        await _repository.UpsertLeadAsync(job);

        job.RaisePropertyChanged(nameof(job.Business));
        job.RaisePropertyChanged(nameof(job.PreferredContact));
        job.RaisePropertyChanged(nameof(job.DisplayBusinessName));
    }

    public async Task ChangePreferredContactAsync(DashboardLead job, JobHunter.Domain.Models.Contact? newContact)
    {
        if (job == null) return;
        if (job.PreferredContactId == newContact?.Id) return;

        job.PreferredContactId = newContact?.Id;
        job.PreferredContact = newContact;

        await _repository.UpsertLeadAsync(job);

        job.RaisePropertyChanged(nameof(job.PreferredContact));
        job.RaisePropertyChanged(nameof(job.DisplayPreferredContact));
    }

    private async Task FetchLeadsAsync()
    {
        // Find businesses from AvailableBusinesses for mock data
        var canadaBuys = AvailableBusinesses.FirstOrDefault(b => b.Name == "CanadaBuys");
        var seao = AvailableBusinesses.FirstOrDefault(b => b.Name == "SEAO Quebec");
        var xometry = AvailableBusinesses.FirstOrDefault(b => b.Name == "Xometry Partner Network");

        var newJobs = new List<DashboardLead>
        {
            new DashboardLead { Id = $"CB-{Random.Shared.Next(1000,9999)}", Title = "CNC Aerospace Components", BusinessId = canadaBuys?.Id, Business = canadaBuys, ClosingDate = DateTime.Now.AddDays(14), IsAutomationPossible = true, Url = "https://buyandsell.gc.ca/", MachiningProcessInferred = new[] { MachiningProcess.CncMilling }, EstimatedValue = 4500.00m, AskedPrice = 4000.00m },
            new DashboardLead { Id = $"SE-{Random.Shared.Next(1000,9999)}", Title = "Usinage d'aluminium 6061", BusinessId = seao?.Id, Business = seao, ClosingDate = DateTime.Now.AddDays(5), IsAutomationPossible = true, Url = "https://seao.ca/", MachiningProcessInferred = new[] { MachiningProcess.CncMilling }, EstimatedValue = 1250.00m, AskedPrice = null },
            new DashboardLead { Id = $"XM-{Random.Shared.Next(1000,9999)}", Title = "Rapid Prototyping - Medical Device", BusinessId = xometry?.Id, Business = xometry, ClosingDate = DateTime.Now.AddDays(2), IsAutomationPossible = false, Url = "https://xometry.com/", MachiningProcessInferred = new[] { MachiningProcess.CncTurning }, EstimatedValue = 800.00m, AskedPrice = 750.00m }
        };

        foreach (var job in newJobs)
        {
            await _repository.UpsertLeadAsync(job);
        }

        // Refresh UI from the DB
        await LoadLeadsFromDatabaseAsync();

        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Sync Complete", $"Successfully fetched and saved {newJobs.Count} new machining leads.", "OK");
        }
    }

    public async Task ChangeStatusAsync(DashboardLead job, LeadStatus newStatus)
    {
        if (job == null || job.Status?.Id == newStatus?.Id) return;

        try
        {
            string? reason = null;
            if (newStatus?.Name == LeadStatus.Lost || newStatus?.Name == LeadStatus.Rejected)
            {
                if (Application.Current?.MainPage != null)
                {
                    reason = await Application.Current.MainPage.DisplayPromptAsync(
                        "Reason Required",
                        $"Please provide a reason for marking this lead as {newStatus}:",
                        "Submit", "Cancel");

                    if (string.IsNullOrWhiteSpace(reason))
                    {
                        await Application.Current.MainPage.DisplayAlert("Validation Error", "A reason is required to proceed.", "OK");
                        return; // Abort transition
                    }
                }
            }

            if (newStatus?.Name == LeadStatus.Quoted)
            {
                bool hasQuote = await _workflowService.HasQuotePdfAsync(job.Id);
                if (!hasQuote)
                {
                    if (Application.Current?.MainPage != null)
                    {
                        var action = await Application.Current.MainPage.DisplayActionSheet(
                            "Quote PDF Required", "Cancel", null, "Import Quote", "Generate Dummy Quote");

                        if (action == "Import Quote")
                        {
                            await ImportQuoteAsync(job);
                        }
                        else if (action == "Generate Dummy Quote")
                        {
                            await GenerateDummyQuoteAsync(job);
                        }
                        else return; // Abort

                        hasQuote = await _workflowService.HasQuotePdfAsync(job.Id);
                        if (!hasQuote)
                        {
                            await Application.Current.MainPage.DisplayAlert("Validation Error", "A quote PDF is required to transition to Quoted.", "OK");
                            return;
                        }
                    }
                }
            }

            // The Workflow service handles the actual domain logic validations!
            await _workflowService.ChangeLeadStatusAsync(job, newStatus, reason);

            // Re-align the status reference with AvailableStatuses so the picker selection remains valid
            var latestHistory = job.StatusHistories?.OrderByDescending(x => x.Date).FirstOrDefault();
            if (latestHistory?.Status != null)
            {
                latestHistory.Status = AvailableStatuses.FirstOrDefault(s => s.Id == latestHistory.Status.Id) ?? latestHistory.Status;
            }

            job.RaisePropertyChanged(nameof(job.Status));
            job.RaisePropertyChanged(nameof(job.StatusHistories));

            if (job.Status?.Name == LeadStatus.Quoted)
            {
                var quote = new Quote
                {
                    JobId = job.Id,
                    Price = 1500.00m,
                    LeadTime = TimeSpan.FromDays(14),
                    Message = "Automated AI Quote Estimate"
                };

                string businessName = job.Business?.Name ?? "Unknown Provider";
                bool supportsApi = businessName.Contains("Xometry");

                if (supportsApi)
                {
                    Console.WriteLine($"[Mock API Pipeline] API Quote submitted to {businessName}");
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Pipeline AI: Quote Executed", $"An automated CAD API quote for {quote.Price:C} has been successfully submitted to the {businessName} backend.", "Awesome");
                    }
                }
                else
                {
                    Console.WriteLine($"[Mock Email Pipeline] Triggering Email Quote fallback for {businessName}");
                    await _emailService.SendQuoteEmailAsync(job, quote);
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert("Pipeline AI: Quote Drafted", $"An email quote fallback for {quote.Price:C} has been drafted to {businessName} buyer since this portal does not support API automation.", "Understood");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Transition Failed", ex.Message, "OK");
            }
        }
    }

    private async Task ImportQuoteAsync(DashboardLead job)
    {
        if (job == null) return;
        try
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select a Quote PDF",
                FileTypes = FilePickerFileType.Pdf
            });

            if (result != null)
            {
                var folderPath = await _workflowService.GetLeadFolderPathAsync(job.Id);
                var destPath = Path.Combine(folderPath, $"quote_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                File.Copy(result.FullPath, destPath, true);

                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Success", "Quote PDF imported successfully to the lead folder.", "OK");
            }
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to import quote: {ex.Message}", "OK");
        }
    }

    private async Task GenerateDummyQuoteAsync(DashboardLead job)
    {
        if (job == null) return;
        try
        {
            var folderPath = await _workflowService.GetLeadFolderPathAsync(job.Id);
            var destPath = Path.Combine(folderPath, $"dummy_quote_{DateTime.Now:yyyyMMddHHmmss}.pdf");

            // Create a dummy text file but with pdf extension just to satisfy the check
            await File.WriteAllTextAsync(destPath, "Dummy Quote PDF Content");

            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Success", "Dummy Quote PDF generated in the lead folder.", "OK");
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to generate dummy quote: {ex.Message}", "OK");
        }
    }

    private async Task OpenLeadFolderAsync(DashboardLead job)
    {
        if (job == null) return;
        try
        {
            var folderPath = await _workflowService.GetLeadFolderPathAsync(job.Id);
            await Launcher.Default.OpenAsync(new Uri($"file:///{folderPath.Replace('\\', '/')}"));
        }
        catch (Exception ex)
        {
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Error", $"Could not open folder: {ex.Message}", "OK");
        }
    }

    private async Task ViewHistoryAsync(DashboardLead job)
    {
        if (job == null) return;
        if (Application.Current?.MainPage != null)
        {
            if (job.StatusHistories == null || !job.StatusHistories.Any())
            {
                await Application.Current.MainPage.DisplayAlert("History", "No status history found for this lead.", "OK");
                return;
            }

            var historyStr = string.Join("\n\n", job.StatusHistories.OrderByDescending(h => h.Date).Select(h =>
                $"• {h.Date:MMM dd, yyyy HH:mm} - {h.Status?.Name}{(string.IsNullOrEmpty(h.Reason) ? "" : $"\n  Reason: {h.Reason}")}"));

            await Application.Current.MainPage.DisplayAlert($"Status History", historyStr, "Close");
        }
    }
}
