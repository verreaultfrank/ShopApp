using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace JobHunterDashboard.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    public ICommand ChangeThemeModeCommand { get; }
    public ICommand ChangeAccentColorCommand { get; }

    public SettingsViewModel()
    {
        ChangeThemeModeCommand = new Command<string>(ApplyThemeMode);
        ChangeAccentColorCommand = new Command<string>(ApplyAccentColor);
    }

    private void ApplyThemeMode(string mode)
    {
        var resources = Application.Current?.Resources;
        if (resources == null) return;

        if (mode == "Dark")
        {
            resources["AppBackgroundColor"] = resources["DarkBackground"];
            resources["AppSurfaceColor"] = resources["DarkSurface"];
            resources["AppTextColor"] = resources["DarkText"];
            resources["AppSubtextColor"] = resources["DarkSubtext"];
            resources["AppBorderColor"] = resources["DarkBorder"];
        }
        else // Light Default
        {
            resources["AppBackgroundColor"] = resources["LightBackground"];
            resources["AppSurfaceColor"] = resources["LightSurface"];
            resources["AppTextColor"] = resources["LightText"];
            resources["AppSubtextColor"] = resources["LightSubtext"];
            resources["AppBorderColor"] = resources["LightBorder"];
        }
    }

    private void ApplyAccentColor(string colorKey)
    {
        var resources = Application.Current?.Resources;
        if (resources != null && resources.TryGetValue(colorKey, out var colorObj) && colorObj is Color newColor)
        {
            resources["AppPrimaryColor"] = newColor;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
