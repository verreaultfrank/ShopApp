using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JobHunter.Domain.Models;

namespace JobHunterDashboard.Models;

public class DashboardJobOpportunity : JobOpportunity, INotifyPropertyChanged
{
    public string DisplayMachiningProcesses => MachiningProcessInferred != null && MachiningProcessInferred.Length > 0 ? string.Join(", ", MachiningProcessInferred) : "None specified";
    public string DisplayMaterials => Materials != null && Materials.Count > 0 ? string.Join(", ", Materials.Select(m => m.Name)) : "None specified";
    
    // New computed property for UI
    public JobStatus? Status => StatusHistories?.OrderByDescending(x => x.Date).FirstOrDefault()?.Status;

    // Business display
    public string DisplayBusinessName => Business?.Name ?? "No Business";
    public string DisplayPreferredContact => PreferredContact?.FullName ?? "No Contact";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
