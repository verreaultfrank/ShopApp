using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;

namespace JobHunterDashboard.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    public SettingsViewModel()
    {
    }

    [RelayCommand]
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

    [RelayCommand]
    private void ApplyAccentColor(string colorKey)
    {
        var resources = Application.Current?.Resources;
        if (resources != null && resources.TryGetValue(colorKey, out var colorObj) && colorObj is Color newColor)
        {
            resources["AppPrimaryColor"] = newColor;
        }
    }
}

