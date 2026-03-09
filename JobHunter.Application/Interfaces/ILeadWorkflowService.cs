using JobHunter.Domain.Models;

namespace JobHunter.Application.Interfaces;

public interface ILeadWorkflowService
{
    Task<string> GetLeadFolderPathAsync(string leadId);
    Task<bool> HasQuotePdfAsync(string leadId);
    Task InitializeFoldersForExistingLeadsAsync();
    Task<bool> ValidateStateTransitionAsync(JobOpportunity lead, JobStatus newStatus);
    Task ChangeLeadStatusAsync(JobOpportunity lead, JobStatus newStatus, string? reason = null);
}
