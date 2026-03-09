namespace JobHunterDashboard;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(Views.SettingsPage), typeof(Views.SettingsPage));
	}
}
