using JobHunter.Domain.Models;
using JobHunterDashboard.Models;
using JobHunterDashboard.ViewModels;

namespace JobHunterDashboard.Views;

public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MainViewModel viewModel)
        {
            await viewModel.InitializeAsync();
        }
    }

    private async void OnStatusSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;

        // 1. MAUI recycling/scrolling sets this to null. Ignore it!
        if (picker.SelectedItem == null) return;

        if (picker.BindingContext is DashboardJobOpportunity job)
        {
            var newStatus = (JobStatus)picker.SelectedItem;

            // 2. The binding engine applying the initial value. Ignore it!
            if (newStatus.Id == job.Status?.Id) return;

            // --> If we reach here, the user genuinely clicked a new item! <--
            var viewModel = (MainViewModel)BindingContext;

            picker.SelectedItem = job.Status; // Immediately revert visually

            await viewModel.ChangeStatusAsync(job, newStatus); // Trigger the save/popups

            picker.SelectedItem = job.Status; // Re-sync picker if successful
        }
    }

    private async void OnBusinessSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;

        // 1. Block scroll recycling
        if (picker.SelectedItem == null) return;

        if (picker.BindingContext is DashboardJobOpportunity job && picker.SelectedItem is Business newBusiness)
        {
            // 2. Block initial binding
            if (newBusiness.Id == job.BusinessId) return;

            // Genuine User Interaction
            var viewModel = (MainViewModel)BindingContext;
            await viewModel.ChangeBusinessAsync(job, newBusiness);
        }
    }

    private async void OnPreferredContactSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;

        // 1. Block scroll recycling
        if (picker.SelectedItem == null) return;

        if (picker.BindingContext is DashboardJobOpportunity job && picker.SelectedItem is JobHunter.Domain.Models.Contact newContact)
        {
            // 2. Block initial binding
            if (newContact.Id == job.PreferredContactId) return;

            // Genuine User Interaction
            var viewModel = (MainViewModel)BindingContext;
            await viewModel.ChangePreferredContactAsync(job, newContact);
        }
    }
}
