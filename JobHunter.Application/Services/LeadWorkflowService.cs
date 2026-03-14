using JobHunter.Application.Interfaces;
using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace JobHunter.Application.Services;

public class LeadWorkflowService : ILeadWorkflowService
{
    private readonly IConfiguration _configuration;
    private readonly IJobLeadRepository _jobLeadRepository;
    private readonly string _basePath;

    public LeadWorkflowService(IConfiguration configuration, IJobLeadRepository jobLeadRepository)
    {
        _configuration = configuration;
        _jobLeadRepository = jobLeadRepository;
        _basePath = _configuration["JobHunterFileSystem"] ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JobHunter");
    }

    public async Task<string> GetLeadFolderPathAsync(string leadId)
    {
        // Sanitize the lead ID to ensure it's a valid folder name
        var safeLeadId = string.Join("_", leadId.Split(Path.GetInvalidFileNameChars()));
        var path = Path.Combine(_basePath, "Leads", safeLeadId);
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        return await Task.FromResult(path);
    }

    public async Task<bool> HasQuotePdfAsync(string leadId)
    {
        var folderPath = await GetLeadFolderPathAsync(leadId);
        var anyPdf = Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly);
        return anyPdf.Any();
    }

    public async Task InitializeFoldersForExistingLeadsAsync()
    {
        // Get all leads
        var allLeads = await _jobLeadRepository.GetLeadsAsync(pageSize: 10000);
        foreach (var lead in allLeads)
        {
            await GetLeadFolderPathAsync(lead.Id);
        }
    }

    public async Task<bool> ValidateStateTransitionAsync(Lead lead, LeadStatus newStatus)
    {
        // Compute current status from StatusHistories (same logic as DashboardJobOpportunity.Status)
        var currentStatus = lead.StatusHistories?.OrderByDescending(x => x.Date).FirstOrDefault()?.Status;

        if (currentStatus?.Id == newStatus?.Id) return true;

        switch (currentStatus?.Name)
        {
            case LeadStatus.New:
                // A lead can only go from new status, to Quoted or Lost or Rejected
                return newStatus?.Name is LeadStatus.Quoted or LeadStatus.Lost or LeadStatus.Rejected;
                
            case LeadStatus.Quoted:
                // A lead can only go from Quoted to won or lost or rejected
                if (newStatus?.Name is not (LeadStatus.Won or LeadStatus.Lost or LeadStatus.Rejected)) return false;
                break;
                
            case LeadStatus.Won:
                // A lead can go from Won to Lost or Reject
                return newStatus?.Name is LeadStatus.Lost or LeadStatus.Rejected;
                
            case LeadStatus.Lost:
            case LeadStatus.Rejected:
                // No other lead status transition are possible!
                return false;
        }

        // Additional validation when transitioning TO Quoted
        if (newStatus?.Name == LeadStatus.Quoted)
        {
            var folderPath = await GetLeadFolderPathAsync(lead.Id);
            var quoteFiles = Directory.GetFiles(folderPath, "*quote*.pdf", SearchOption.TopDirectoryOnly);
            if (!quoteFiles.Any())
            {
                // To be safe, any pdf might be considered a quote, let's just check if ANY pdf exists
                var anyPdf = Directory.GetFiles(folderPath, "*.pdf", SearchOption.TopDirectoryOnly);
                if (!anyPdf.Any()) return false;
            }
        }

        return true;
    }

    public async Task ChangeLeadStatusAsync(Lead lead, LeadStatus newStatus, string? reason = null)
    {
        // Compute current status from StatusHistories
        var currentStatus = lead.StatusHistories?.OrderByDescending(x => x.Date).FirstOrDefault()?.Status;

        if (currentStatus?.Id == newStatus?.Id) return;

        // Validation
        if (!await ValidateStateTransitionAsync(lead, newStatus))
        {
            throw new InvalidOperationException($"Cannot transition from {currentStatus?.Name} to {newStatus?.Name}. Requirements not met.");
        }

        if ((newStatus?.Name == LeadStatus.Lost || newStatus?.Name == LeadStatus.Rejected) && string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required when marking a lead as Lost or Rejected.", nameof(reason));
        }

        // Add history record
        lead.StatusHistories ??= new List<LeadStatusHistory>();
        lead.StatusHistories.Add(new LeadStatusHistory
        {
            Status = new LeadStatus { Id = newStatus!.Id, Name = newStatus.Name },
            Date = DateTime.Now,
            Reason = reason
        });

        await _jobLeadRepository.UpsertLeadAsync(lead);
    }
}
