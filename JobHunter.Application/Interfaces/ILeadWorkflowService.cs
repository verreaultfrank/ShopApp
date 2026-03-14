using JobHunter.Domain.Models;

namespace JobHunter.Application.Interfaces;

public interface ILeadWorkflowService
{
    Task<string> GetLeadFolderPathAsync(string leadId);
    Task<bool> HasQuotePdfAsync(string leadId);
    Task InitializeFoldersForExistingLeadsAsync();
    Task<bool> ValidateStateTransitionAsync(Lead lead, LeadStatus newStatus);
    Task ChangeLeadStatusAsync(Lead lead, LeadStatus newStatus, string? reason = null);
}
