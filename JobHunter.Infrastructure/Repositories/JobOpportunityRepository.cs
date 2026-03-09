using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;
using JobHunter.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace JobHunter.Infrastructure.Repositories;

public class JobOpportunityRepository : IJobOpportunityRepository
{
    private readonly JobHunterDbContext _context;

    public JobOpportunityRepository(JobHunterDbContext context)
    {
        _context = context;
    }

    public async Task<JobOpportunity?> GetByIdAsync(string id)
    {
        var entity = await _context.JobLeads
            .Include(j => j.MaterialLinks)
            .ThenInclude(l => l.Material)
            .Include(j => j.StatusHistories)
            .ThenInclude(sh => sh.Status)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entity == null) return null;

        return MapToDomain(entity);
    }

    public async Task<IEnumerable<JobOpportunity>> GetAllAsync()
    {
        var entities = await _context.JobLeads
            .Include(j => j.MaterialLinks)
            .ThenInclude(l => l.Material)
            .Include(j => j.StatusHistories)
            .ThenInclude(sh => sh.Status)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task AddAsync(JobOpportunity job)
    {
        var entity = MapToEntity(job, new JobOpportunityEntity { Id = job.Id });
        
        if (!entity.StatusHistories.Any())
        {
            entity.StatusHistories.Add(new LeadStatusHistoryEntity
            {
                JobStatusId = 1, // New
                Date = DateTime.UtcNow,
                Reason = "Creation"
            });
        }
        
        _context.JobLeads.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(JobOpportunity job)
    {
        var entity = await _context.JobLeads
            .Include(j => j.MaterialLinks)
            .FirstOrDefaultAsync(e => e.Id == job.Id);

        if (entity != null)
        {
            MapToEntity(job, entity);
            _context.JobLeads.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(string id)
    {
        var entity = await _context.JobLeads.FindAsync(id);
        if (entity != null)
        {
            _context.JobLeads.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    private JobOpportunity MapToDomain(JobOpportunityEntity entity)
    {
        var job = new JobOpportunity
        {
            Id = entity.Id,
            Title = entity.Title,
            Description = entity.Description,
            BusinessId = entity.BusinessId,
            Url = entity.Url,
            PublishedDate = entity.PublishedDate,
            ClosingDate = entity.ClosingDate,
            IsAutomationPossible = entity.IsAutomationPossible,
            AskedPrice = entity.AskedPrice,
            DateFetched = entity.DateFetched,
            EstimatedValue = entity.EstimatedValue
        };

        job.StatusHistories = entity.StatusHistories.Select(sh => new LeadStatusHistory
        {
            Id = sh.Id,
            JobStatusId = sh.JobStatusId,
            Status = sh.Status != null ? new JobStatus { Id = sh.Status.Id, Name = sh.Status.Name } : null!,
            Date = sh.Date,
            Reason = sh.Reason
        }).ToList();

        if (!string.IsNullOrEmpty(entity.MetadataJson))
        {
            job.Metadata = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.MetadataJson) ?? new Dictionary<string, string>();
        }

        if (!string.IsNullOrEmpty(entity.MachiningProcessInferredCsv))
        {
            job.MachiningProcessInferred = entity.MachiningProcessInferredCsv
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => Enum.TryParse<MachiningProcess>(s.Trim(), true, out var result) ? result : MachiningProcess.Unknown)
                .Where(x => x != MachiningProcess.Unknown)
                .ToArray();
        }

        job.Materials = entity.MaterialLinks.Select(l => new Material
        {
            Id = l.Material.Id,
            Name = l.Material.Name,
            Category = l.Material.Category,
            AmsDesignation = l.Material.AmsDesignation,
            UnsDesignation = l.Material.UnsDesignation,
            IsoDesignation = l.Material.IsoDesignation,
            Form = l.Material.Form,
            TemperCondition = l.Material.TemperCondition
        }).ToList();

        return job;
    }

    private JobOpportunityEntity MapToEntity(JobOpportunity domain, JobOpportunityEntity entity)
    {
        entity.Title = domain.Title;
        entity.Description = domain.Description;
        entity.BusinessId = domain.BusinessId;
        entity.Url = domain.Url;
        entity.PublishedDate = domain.PublishedDate;
        entity.ClosingDate = domain.ClosingDate;
        entity.IsAutomationPossible = domain.IsAutomationPossible;
        entity.AskedPrice = domain.AskedPrice;
        entity.DateFetched = domain.DateFetched;
        entity.EstimatedValue = domain.EstimatedValue;

        foreach (var sh in domain.StatusHistories.Where(s => s.Id == 0))
        {
            entity.StatusHistories.Add(new LeadStatusHistoryEntity
            {
                JobStatusId = sh.JobStatusId,
                Date = sh.Date,
                Reason = sh.Reason
            });
        }

        entity.MetadataJson = JsonSerializer.Serialize(domain.Metadata);
        entity.MachiningProcessInferredCsv = domain.MachiningProcessInferred != null && domain.MachiningProcessInferred.Length > 0
            ? string.Join(",", domain.MachiningProcessInferred)
            : null;

        // Materials update is more complex, for this prototype we just clear and add
        entity.MaterialLinks.Clear();
        foreach (var mat in domain.Materials)
        {
            entity.MaterialLinks.Add(new JobMaterialLinkEntity
            {
                JobId = entity.Id,
                MaterialId = mat.Id
            });
        }

        return entity;
    }
}
