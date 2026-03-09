using Microsoft.EntityFrameworkCore;
using JobHunter.Domain.Models;
using JobHunter.Domain.Interfaces;

namespace JobHunter.Infrastructure.Repositories;

public class JobStatusRepository : IJobStatusRepository
{
    private readonly IDbContextFactory<JobHunterDbContext> _contextFactory;

    public JobStatusRepository(IDbContextFactory<JobHunterDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<JobStatus>> GetAllStatusesAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var entities = await context.JobStatuses.ToListAsync();

        return entities.Select(e => new JobStatus
        {
            Id = e.Id,
            Name = e.Name
        }).ToList();
    }
}
