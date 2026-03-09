using System.Linq;
using JobHunter.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JobHunterDashboard.Models;

[INotifyPropertyChanged]
public partial class DashboardJobOpportunity : JobOpportunity
{
    public string DisplayMachiningProcesses => MachiningProcessInferred != null && MachiningProcessInferred.Length > 0 ? string.Join(", ", MachiningProcessInferred) : "None specified";

    public string DisplayMaterials => Materials != null && Materials.Count > 0 ? string.Join(", ", Materials.Select(m => m.Name)) : "None specified";

    public JobStatus? Status => StatusHistories?.OrderByDescending(x => x.Date).FirstOrDefault()?.Status;

    public string DisplayBusinessName => Business?.Name ?? "No Business";
    public string DisplayPreferredContact => PreferredContact?.FullName ?? "No Contact";

    public void RaisePropertyChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
    }
}