using Microsoft.EntityFrameworkCore;
using JobHunter.Domain.Models;
using JobHunter.Domain.Interfaces;

namespace JobHunter.Infrastructure.Repositories;

public class LeadStatusRepository : IJobStatusRepository
{
    private readonly IDbContextFactory<JobHunterDbContext> _contextFactory;

    public LeadStatusRepository(IDbContextFactory<JobHunterDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<LeadStatus>> GetAllStatusesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var entities = await context.JobStatuses.ToListAsync();

        return entities.Select(e => new LeadStatus
        {
            Id = e.Id,
            Name = e.Name
        }).ToList();
    }
}
