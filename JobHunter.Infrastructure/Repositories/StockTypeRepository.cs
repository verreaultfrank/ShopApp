using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;
using JobHunter.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public class StockTypeRepository : IStockTypeRepository
{
    private readonly JobHunterDbContext _context;

    public StockTypeRepository(JobHunterDbContext context)
    {
        _context = context;
    }

    public async Task<StockType?> GetByIdAsync(int id)
    {
        var entity = await _context.StockTypes.FindAsync(id);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<IEnumerable<StockType>> GetAllAsync()
    {
        var entities = await _context.StockTypes.ToListAsync();
        return entities.Select(MapToDomain);
    }

    private StockType MapToDomain(StockTypeEntity entity)
    {
        return new StockType
        {
            Id = entity.Id,
            Name = entity.Name
        };
    }
}
