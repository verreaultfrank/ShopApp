using JobHunterDashboard.ViewModels;

namespace JobHunterDashboard.Views;

public partial class InventoryPage : ContentPage
{
	public InventoryPage(InventoryViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
