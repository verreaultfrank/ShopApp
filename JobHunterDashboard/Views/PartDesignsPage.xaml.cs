using JobHunterDashboard.ViewModels;

namespace JobHunterDashboard.Views;

public partial class PartDesignsPage : ContentPage
{
    public PartDesignsPage(PartDesignsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
