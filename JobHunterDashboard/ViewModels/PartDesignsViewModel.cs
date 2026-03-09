using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;

namespace JobHunterDashboard.ViewModels;

public partial class PartDesignsViewModel : ObservableObject
{
    private readonly IPartDesignRepository _repository;
    
    [ObservableProperty]
    private ObservableCollection<PartDesign> _parts = new();

    [ObservableProperty]
    private string _searchText = "";

    partial void OnSearchTextChanged(string value)
    {
        _ = LoadPartsAsync();
    }

    public PartDesignsViewModel(IPartDesignRepository repository)
    {
        _repository = repository;
        _ = LoadPartsAsync();
    }

    [RelayCommand]
    private async Task ViewCad(string path)
    {
        if (!string.IsNullOrEmpty(path) && File.Exists(path))
        {
            await Launcher.Default.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
        }
        else if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "CAD file not found on disk.", "OK");
        }
    }

    private async Task LoadPartsAsync()
    {
        try
        {
            IEnumerable<PartDesign> parts;
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                parts = await _repository.GetAllAsync();
            }
            else
            {
                parts = await _repository.SearchAsync(SearchText);
            }
            
            Parts.Clear();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var p in parts)
                {
                    Parts.Add(p);
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading parts: {ex.Message}");
        }
    }
}
