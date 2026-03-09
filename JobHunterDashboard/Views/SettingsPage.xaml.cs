using Microsoft.Maui.Controls;
using JobHunterDashboard.ViewModels;

namespace JobHunterDashboard.Views;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
