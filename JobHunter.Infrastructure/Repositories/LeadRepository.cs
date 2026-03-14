using Microsoft.EntityFrameworkCore;
using JobHunter.Domain.Models;
using JobHunter.Domain.Interfaces;
using JobHunter.Infrastructure.Entities;

namespace JobHunter.Infrastructure.Repositories;

public class LeadRepository : IJobLeadRepository
{
    private readonly IDbContextFactory<JobHunterDbContext> _contextFactory;

    public LeadRepository(IDbContextFactory<JobHunterDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<Lead>> GetLeadsAsync(string searchText = "", IEnumerable<string>? providers = null, IEnumerable<string>? statuses = null, int pageNumber = 1, int pageSize = 20, LeadSortOption sortBy = LeadSortOption.NewestFirst)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var query = context.Leads
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
            LeadSortOption.NewestFirst => query.OrderByDescending(j => j.DateFetched),
            LeadSortOption.ClosingDateSoonest => query.OrderBy(j => j.ClosingDate),
            LeadSortOption.PriceHighest => query.OrderByDescending(j => j.AskedPrice ?? j.EstimatedValue ?? 0),
            LeadSortOption.PriceLowest => query.OrderBy(j => j.AskedPrice ?? j.EstimatedValue ?? decimal.MaxValue),
            _ => query.OrderByDescending(j => j.DateFetched),
        };
        
        var leads = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var result = leads.Select(row => new Lead
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
            StatusHistories = row.StatusHistories?.Select(h => new LeadStatusHistory
            {
                Id = h.Id,
                Status = new LeadStatus { Id = h.Status.Id, Name = h.Status.Name },
                Date = h.Date,
                Reason = h.Reason
            }).OrderByDescending(h => h.Date).ToList() ?? new List<LeadStatusHistory>()
        }).ToList();
        
        return result;
    }

    public async Task UpsertLeadAsync(Lead job)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        
        var existing = await context.Leads
            .Include(j => j.StatusHistories)
            .FirstOrDefaultAsync(j => j.Id == job.Id);
        
        if (existing == null)
        {
            var newEntity = new LeadEntity
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
                StatusHistories = job.StatusHistories?.Select(h => new LeadStatusHistoryEntity
                {
                    LeadStatusId = h.Status.Id,
                    Date = h.Date,
                    Reason = h.Reason
                }).ToList() ?? new List<LeadStatusHistoryEntity>()
            };
            context.Leads.Add(newEntity);
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

            // Sync status histories (only add new ones)
            if (job.StatusHistories != null)
            {
                foreach (var hist in job.StatusHistories)
                {
                    if (hist.Id == 0)
                    {
                        existing.StatusHistories.Add(new LeadStatusHistoryEntity
                        {
                            LeadStatusId = hist.Status.Id,
                            Date = hist.Date,
                            Reason = hist.Reason
                        });
                    }
                }
            }
        }

        await context.SaveChangesAsync();
        

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
