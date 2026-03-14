using System.Linq;
using JobHunter.Domain.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace JobHunterDashboard.Models;

[INotifyPropertyChanged]
public partial class DashboardLead : Lead
{
    public string DisplayMachiningProcesses => MachiningProcessInferred != null && MachiningProcessInferred.Length > 0 ? string.Join(", ", MachiningProcessInferred) : "None specified";

    public LeadStatus? Status => StatusHistories?.OrderByDescending(x => x.Date).FirstOrDefault()?.Status;

    public string DisplayBusinessName => Business?.Name ?? "No Business";
    public string DisplayPreferredContact => PreferredContact?.FullName ?? "No Contact";

    public void RaisePropertyChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
    }
}