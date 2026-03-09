using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;

namespace JobHunterDashboard.ViewModels;

public class PartDesignsViewModel : INotifyPropertyChanged
{
    private readonly IPartDesignRepository _repository;
    
    private ObservableCollection<PartDesign> _parts = new();
    public ObservableCollection<PartDesign> Parts
    {
        get => _parts;
        set { _parts = value; OnPropertyChanged(); }
    }

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                _ = LoadPartsAsync();
            }
        }
    }

    public ICommand ViewCadCommand { get; }

    public PartDesignsViewModel(IPartDesignRepository repository)
    {
        _repository = repository;
        
        ViewCadCommand = new Command<string>(async (path) => 
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                await Launcher.Default.OpenAsync(new OpenFileRequest { File = new ReadOnlyFile(path) });
            }
            else if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "CAD file not found on disk.", "OK");
            }
        });

        _ = LoadPartsAsync();
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
            foreach (var p in parts)
            {
                Parts.Add(p);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading parts: {ex.Message}");
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
