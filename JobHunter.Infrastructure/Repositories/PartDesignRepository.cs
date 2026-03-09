using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public class PartDesignRepository : IPartDesignRepository
{
    private readonly IDbContextFactory<JobHunterDbContext> _contextFactory;

    public PartDesignRepository(IDbContextFactory<JobHunterDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<PartDesign>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var entities = await context.PartDesigns.ToListAsync();
        return entities.Select(e => new PartDesign { Id = e.Id, Name = e.Name, CadFilePath = e.CadFilePath });
    }

    public async Task<IEnumerable<PartDesign>> SearchAsync(string searchTerm)
    {
        using var context = await _contextFactory.CreateDbContextAsync();
        var query = context.PartDesigns.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(p => p.Name.Contains(searchTerm) || p.CadFilePath.Contains(searchTerm));
        }
        
        var entities = await query.ToListAsync();
        return entities.Select(e => new PartDesign { Id = e.Id, Name = e.Name, CadFilePath = e.CadFilePath });
    }
}
