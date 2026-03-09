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

        if (picker.BindingContext is DashboardJobOpportunity job)
        {
            if (!picker.IsFocused) return; // Only react to user interaction!

            var newStatus = (JobStatus)picker.SelectedItem;
            if (newStatus?.Id == job.Status?.Id) return; // Prevent Initialization Spams

            var viewModel = (MainViewModel)BindingContext;

            // Immediately revert picker selection visually so it doesn't bypass validation
            picker.SelectedItem = job.Status;

            await viewModel.ChangeStatusAsync(job, newStatus);

            // Re-sync picker if successful
            picker.SelectedItem = job.Status;
        }
    }

    private async void OnBusinessSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;

        if (picker.BindingContext is DashboardJobOpportunity job && picker.SelectedItem is Business newBusiness)
        {
            if (!picker.IsFocused) return; // Only react to user interaction!

            if (newBusiness.Id == job.BusinessId) return;

            var viewModel = (MainViewModel)BindingContext;
            await viewModel.ChangeBusinessAsync(job, newBusiness);
        }
    }

    private async void OnPreferredContactSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;

        if (picker.BindingContext is DashboardJobOpportunity job && picker.SelectedItem is JobHunter.Domain.Models.Contact newContact)
        {
            if (!picker.IsFocused) return; // Only react to user interaction!

            if (newContact.Id == job.PreferredContactId) return;

            var viewModel = (MainViewModel)BindingContext;
            await viewModel.ChangePreferredContactAsync(job, newContact);
        }
    }
}
