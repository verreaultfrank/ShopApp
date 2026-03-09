using Microsoft.EntityFrameworkCore;
using JobHunter.Domain.Models;
using JobHunter.Domain.Interfaces;
using JobHunter.Infrastructure.Entities;

namespace JobHunter.Infrastructure.Repositories;

public class JobLeadRepository : IJobLeadRepository
{
    private readonly IDbContextFactory<JobHunterDbContext> _contextFactory;

    public JobLeadRepository(IDbContextFactory<JobHunterDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<JobOpportunity>> GetLeadsAsync(string searchText = "", IEnumerable<string>? providers = null, IEnumerable<string>? statuses = null, int pageNumber = 1, int pageSize = 20, JobSortOption sortBy = JobSortOption.NewestFirst)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.JobLeads
            .Include(j => j.MaterialLinks)
            .ThenInclude(l => l.Material)
            .Include(j => j.PartDesignLinks)
                .ThenInclude(l => l.PartDesign)
            .Include(j => j.StatusHistories)
                .ThenInclude(h => h.Status)
            .Include(j => j.Business)
                .ThenInclude(b => b!.Contacts)
            .Include(j => j.PreferredContact)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchText))
        {
            query = query.Where(j => j.Title.Contains(searchText) || j.Description.Contains(searchText));
        }
            
        if (providers != null && providers.Any())
        {
            query = query.Where(j => j.Business != null && providers.Contains(j.Business.Name));
        }
            
        if (statuses != null && statuses.Any())
        {
            query = query.Where(j => statuses.Contains(j.StatusHistories.OrderByDescending(h => h.Date).FirstOrDefault()!.Status.Name));
        }

        query = sortBy switch
        {
            JobSortOption.NewestFirst => query.OrderByDescending(j => j.DateFetched),
            JobSortOption.ClosingDateSoonest => query.OrderBy(j => j.ClosingDate),
            JobSortOption.PriceHighest => query.OrderByDescending(j => j.AskedPrice ?? j.EstimatedValue ?? 0),
            JobSortOption.PriceLowest => query.OrderBy(j => j.AskedPrice ?? j.EstimatedValue ?? decimal.MaxValue),
            _ => query.OrderByDescending(j => j.DateFetched),
        };
        
        var leads = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var result = leads.Select(row => new JobOpportunity
        {
            Id = row.Id,
            Title = row.Title,
            Url = row.Url,
            ClosingDate = row.ClosingDate,
            IsAutomationPossible = row.IsAutomationPossible,
            DateFetched = row.DateFetched,
            Description = row.Description,
            MachiningProcessInferred = string.IsNullOrEmpty(row.MachiningProcessInferredCsv) 
                ? Array.Empty<MachiningProcess>() 
                : row.MachiningProcessInferredCsv.Split(',').Select(Enum.Parse<MachiningProcess>).ToArray(),
            EstimatedValue = row.EstimatedValue,
            AskedPrice = row.AskedPrice,
            PublishedDate = row.PublishedDate,
            Metadata = string.IsNullOrEmpty(row.MetadataJson) 
                ? new Dictionary<string, string>() 
                : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(row.MetadataJson) ?? new Dictionary<string, string>(),
            ProviderJobId = row.ProviderJobId,
            BusinessId = row.BusinessId,
            Business = row.Business != null ? new Business
            {
                Id = row.Business.Id,
                Name = row.Business.Name,
                Address = row.Business.Address,
                Website = row.Business.Website,
                Email = row.Business.Email,
                Contacts = row.Business.Contacts?.Select(c => new Contact
                {
                    Id = c.Id,
                    BusinessId = c.BusinessId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Email = c.Email,
                    Phone = c.Phone,
                    Mobile = c.Mobile,
                    Title = c.Title
                }).ToList() ?? new List<Contact>()
            } : null,
            PreferredContactId = row.PreferredContactId,
            PreferredContact = row.PreferredContact != null ? new Contact
            {
                Id = row.PreferredContact.Id,
                BusinessId = row.PreferredContact.BusinessId,
                FirstName = row.PreferredContact.FirstName,
                LastName = row.PreferredContact.LastName,
                Email = row.PreferredContact.Email,
                Phone = row.PreferredContact.Phone,
                Mobile = row.PreferredContact.Mobile,
                Title = row.PreferredContact.Title
            } : null,
            Materials = row.MaterialLinks?.Select(l => new Material 
            { 
                Id = l.Material.Id, 
                Name = l.Material.Name 
            }).ToList() ?? new List<Material>(),
            PartDesigns = row.PartDesignLinks?.Select(l => new PartDesign
            {
                Id = l.PartDesign.Id,
                Name = l.PartDesign.Name,
                CadFilePath = l.PartDesign.CadFilePath
            }).ToList() ?? new List<PartDesign>(),
            StatusHistories = row.StatusHistories?.Select(h => new LeadStatusHistory
            {
                Id = h.Id,
                Status = new JobStatus { Id = h.Status.Id, Name = h.Status.Name },
                Date = h.Date,
                Reason = h.Reason
            }).OrderByDescending(h => h.Date).ToList() ?? new List<LeadStatusHistory>()
        }).ToList();
        
        return result;
    }

    public async Task UpsertLeadAsync(JobOpportunity job)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var existing = await context.JobLeads
            .Include(j => j.MaterialLinks)
            .Include(j => j.PartDesignLinks)
            .Include(j => j.StatusHistories)
            .FirstOrDefaultAsync(j => j.Id == job.Id);
        
        if (existing == null)
        {
            var newEntity = new JobOpportunityEntity
            {
                Id = job.Id,
                Title = job.Title,
                Url = job.Url,
                ClosingDate = job.ClosingDate,
                IsAutomationPossible = job.IsAutomationPossible,
                DateFetched = job.DateFetched,
                Description = job.Description,
                MachiningProcessInferredCsv = job.MachiningProcessInferred != null && job.MachiningProcessInferred.Length > 0 ? string.Join(",", job.MachiningProcessInferred) : null,
                EstimatedValue = job.EstimatedValue,
                AskedPrice = job.AskedPrice,
                PublishedDate = job.PublishedDate,
                MetadataJson = System.Text.Json.JsonSerializer.Serialize(job.Metadata),
                ProviderJobId = job.ProviderJobId,
                BusinessId = job.BusinessId,
                PreferredContactId = job.PreferredContactId,
                MaterialLinks = job.Materials?.Select(m => new JobMaterialLinkEntity
                {
                    MaterialId = m.Id,
                    JobId = job.Id
                }).ToList() ?? new List<JobMaterialLinkEntity>(),
                PartDesignLinks = job.PartDesigns?.Select(p => new JobPartDesignLinkEntity
                {
                    JobLeadId = job.Id,
                    PartDesignId = p.Id > 0 ? p.Id : 0,
                    PartDesign = p.Id == 0 ? new PartDesignEntity { Name = p.Name, CadFilePath = p.CadFilePath } : null!
                }).ToList() ?? new List<JobPartDesignLinkEntity>(),
                StatusHistories = job.StatusHistories?.Select(h => new LeadStatusHistoryEntity
                {
                    JobStatusId = h.Status.Id,
                    Date = h.Date,
                    Reason = h.Reason
                }).ToList() ?? new List<LeadStatusHistoryEntity>()
            };
            context.JobLeads.Add(newEntity);
        }
        else
        {
            existing.Title = job.Title;
            existing.Url = job.Url;
            existing.ClosingDate = job.ClosingDate;
            existing.IsAutomationPossible = job.IsAutomationPossible;
            existing.Description = job.Description;
            existing.MachiningProcessInferredCsv = job.MachiningProcessInferred != null && job.MachiningProcessInferred.Length > 0 ? string.Join(",", job.MachiningProcessInferred) : null;
            existing.EstimatedValue = job.EstimatedValue;
            existing.AskedPrice = job.AskedPrice;
            existing.PublishedDate = job.PublishedDate;
            existing.MetadataJson = System.Text.Json.JsonSerializer.Serialize(job.Metadata);
            existing.ProviderJobId = job.ProviderJobId;
            existing.DateFetched = job.DateFetched;
            existing.BusinessId = job.BusinessId;
            existing.PreferredContactId = job.PreferredContactId;

            // Update material links
            if (job.Materials != null)
            {
                existing.MaterialLinks.Clear();
                foreach (var mat in job.Materials)
                {
                    existing.MaterialLinks.Add(new JobMaterialLinkEntity
                    {
                        MaterialId = mat.Id,
                        JobId = job.Id
                    });
                }
            }

            // Update part design links
            if (job.PartDesigns != null)
            {
                var existingPartIds = job.PartDesigns.Where(p => p.Id > 0).Select(p => p.Id).ToList();
                existing.PartDesignLinks.RemoveAll(l => !existingPartIds.Contains(l.PartDesignId) && l.PartDesignId > 0);
                
                foreach (var pd in job.PartDesigns)
                {
                    if (pd.Id > 0 && !existing.PartDesignLinks.Any(l => l.PartDesignId == pd.Id))
                    {
                        existing.PartDesignLinks.Add(new JobPartDesignLinkEntity { PartDesignId = pd.Id, JobLeadId = job.Id });
                    }
                    else if (pd.Id == 0)
                    {
                        var newLink = new JobPartDesignLinkEntity 
                        { 
                            JobLeadId = job.Id,
                            PartDesign = new PartDesignEntity { Name = pd.Name, CadFilePath = pd.CadFilePath }
                        };
                        existing.PartDesignLinks.Add(newLink);
                    }
                }
            }

            // Sync status histories (only add new ones)
            if (job.StatusHistories != null)
            {
                foreach (var hist in job.StatusHistories)
                {
                    if (hist.Id == 0)
                    {
                        existing.StatusHistories.Add(new LeadStatusHistoryEntity
                        {
                            JobStatusId = hist.Status.Id,
                            Date = hist.Date,
                            Reason = hist.Reason
                        });
                    }
                }
            }
        }

        await context.SaveChangesAsync();
        
        // After saving, back-propagate the new IDs to the domain model
        if (job.PartDesigns != null && existing != null)
        {
            foreach (var link in existing.PartDesignLinks)
            {
                var pd = job.PartDesigns.FirstOrDefault(p => p.Name == link.PartDesign?.Name && p.Id == 0);
                if (pd != null && link.PartDesign != null) pd.Id = link.PartDesign.Id;
            }
        }
        if (job.StatusHistories != null && existing != null)
        {
            var newHistories = existing.StatusHistories.Where(h => job.StatusHistories.Any(jh => jh.Id == 0 && jh.Date == h.Date)).ToList();
            foreach (var nh in newHistories)
            {
                var domH = job.StatusHistories.FirstOrDefault(jh => jh.Id == 0 && jh.Date == nh.Date);
                if (domH != null) domH.Id = nh.Id;
            }
        }
    }
}
