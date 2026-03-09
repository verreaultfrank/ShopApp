using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;
using JobHunter.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public class StockItemRepository : IStockItemRepository
{
    private readonly JobHunterDbContext _context;

    public StockItemRepository(JobHunterDbContext context)
    {
        _context = context;
    }

    public async Task<StockItem?> GetByIdAsync(int id)
    {
        var entity = await _context.StockItems
            .Include(e => e.Material)
            .Include(e => e.StockType)
            .FirstOrDefaultAsync(e => e.Id == id);
        return entity == null ? null : MapToDomain(entity);
    }

    public async Task<IEnumerable<StockItem>> GetAllAsync()
    {
        var entities = await _context.StockItems
            .Include(e => e.Material)
            .Include(e => e.StockType)
            .ToListAsync();
        return entities.Select(MapToDomain);
    }

    public async Task<IEnumerable<StockItem>> GetFilteredAsync(
        string? searchText,
        List<string>? stockTypes,
        List<string>? materialCategories,
        int page,
        int pageSize,
        string? sortBy)
    {
        IQueryable<StockItemEntity> query = _context.StockItems
            .Include(e => e.Material)
            .Include(e => e.StockType);

        // ── Search ──
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            var search = searchText.Trim().ToLower();
            query = query.Where(e =>
                (e.StockType != null && e.StockType.Name.ToLower().Contains(search)) ||
                (e.Material != null && e.Material.Name.ToLower().Contains(search)) ||
                (e.Material != null && e.Material.Category.ToLower().Contains(search)) ||
                (e.Location != null && e.Location.ToLower().Contains(search)));
        }

        // ── Filters ──
        if (stockTypes != null && stockTypes.Count > 0)
        {
            query = query.Where(e => e.StockType != null && stockTypes.Contains(e.StockType.Name));
        }

        if (materialCategories != null && materialCategories.Count > 0)
        {
            query = query.Where(e => e.Material != null && materialCategories.Contains(e.Material.Category));
        }

        // ── Sort ──
        query = sortBy switch
        {
            "MaterialAsc"  => query.OrderBy(e => e.Material != null ? e.Material.Name : ""),
            "MaterialDesc" => query.OrderByDescending(e => e.Material != null ? e.Material.Name : ""),
            "TypeAsc"      => query.OrderBy(e => e.StockType != null ? e.StockType.Name : ""),
            "TypeDesc"     => query.OrderByDescending(e => e.StockType != null ? e.StockType.Name : ""),
            "QuantityAsc"  => query.OrderBy(e => e.Quantity),
            "QuantityDesc" => query.OrderByDescending(e => e.Quantity),
            "LengthAsc"    => query.OrderBy(e => e.Length),
            "LengthDesc"   => query.OrderByDescending(e => e.Length),
            _              => query.OrderBy(e => e.Id)
        };

        // ── Page ──
        var entities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return entities.Select(MapToDomain);
    }

    public async Task AddAsync(StockItem item)
    {
        var entity = MapToEntity(item, new StockItemEntity());
        _context.StockItems.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(StockItem item)
    {
        var entity = await _context.StockItems.FindAsync(item.Id);
        if (entity != null)
        {
            MapToEntity(item, entity);
            _context.StockItems.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.StockItems.FindAsync(id);
        if (entity != null)
        {
            _context.StockItems.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // ── Mapping ──

    private StockItem MapToDomain(StockItemEntity entity)
    {
        var domain = new StockItem
        {
            Id = entity.Id,
            StockTypeId = entity.StockTypeId,
            MaterialId = entity.MaterialId,
            OutsideDiameter = entity.OutsideDiameter,
            InsideDiameter = entity.InsideDiameter,
            WallThickness = entity.WallThickness,
            Width = entity.Width,
            Height = entity.Height,
            Thickness = entity.Thickness,
            Length = entity.Length,
            AcrossFlats = entity.AcrossFlats,
            LegLength = entity.LegLength,
            LegWidth = entity.LegWidth,
            FlangeWidth = entity.FlangeWidth,
            WebDepth = entity.WebDepth,
            Quantity = entity.Quantity,
            Location = entity.Location
        };

        if (entity.StockType != null)
        {
            domain.StockType = new StockType
            {
                Id = entity.StockType.Id,
                Name = entity.StockType.Name
            };
        }

        if (entity.Material != null)
        {
            domain.Material = new Material
            {
                Id = entity.Material.Id,
                Name = entity.Material.Name,
                Category = entity.Material.Category,
                AmsDesignation = entity.Material.AmsDesignation,
                UnsDesignation = entity.Material.UnsDesignation,
                IsoDesignation = entity.Material.IsoDesignation,
                Form = entity.Material.Form,
                TemperCondition = entity.Material.TemperCondition
            };
        }

        return domain;
    }

    private StockItemEntity MapToEntity(StockItem domain, StockItemEntity entity)
    {
        entity.StockTypeId = domain.StockTypeId;
        entity.MaterialId = domain.MaterialId;
        entity.OutsideDiameter = domain.OutsideDiameter;
        entity.InsideDiameter = domain.InsideDiameter;
        entity.WallThickness = domain.WallThickness;
        entity.Width = domain.Width;
        entity.Height = domain.Height;
        entity.Thickness = domain.Thickness;
        entity.Length = domain.Length;
        entity.AcrossFlats = domain.AcrossFlats;
        entity.LegLength = domain.LegLength;
        entity.LegWidth = domain.LegWidth;
        entity.FlangeWidth = domain.FlangeWidth;
        entity.WebDepth = domain.WebDepth;
        entity.Quantity = domain.Quantity;
        entity.Location = domain.Location;
        return entity;
    }
}
