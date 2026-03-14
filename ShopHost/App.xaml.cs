namespace ShopHost;

using JobHunterDashboard;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
