using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;
using JobHunter.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace JobHunter.Infrastructure.Repositories;

public class MaterialRepository : IMaterialRepository
{
    private readonly JobHunterDbContext _context;

    public MaterialRepository(JobHunterDbContext context)
    {
        _context = context;
    }

    public async Task<Material?> GetByIdAsync(int id)
    {
        var entity = await _context.Materials.FindAsync(id);
        if (entity == null) return null;
        return MapToDomain(entity);
    }

    public async Task<IEnumerable<Material>> GetAllAsync()
    {
        var entities = await _context.Materials.ToListAsync();
        return entities.Select(MapToDomain);
    }

    public async Task AddAsync(Material material)
    {
        var entity = MapToEntity(material, new MaterialEntity());
        _context.Materials.Add(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Material material)
    {
        var entity = await _context.Materials.FindAsync(material.Id);
        if (entity != null)
        {
            MapToEntity(material, entity);
            _context.Materials.Update(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _context.Materials.FindAsync(id);
        if (entity != null)
        {
            _context.Materials.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    private Material MapToDomain(MaterialEntity entity)
    {
        return new Material
        {
            Id = entity.Id,
            Name = entity.Name,
            Category = entity.Category,
            AmsDesignation = entity.AmsDesignation,
            UnsDesignation = entity.UnsDesignation,
            IsoDesignation = entity.IsoDesignation,
            Form = entity.Form,
            TemperCondition = entity.TemperCondition
        };
    }

    private MaterialEntity MapToEntity(Material domain, MaterialEntity entity)
    {
        entity.Name = domain.Name;
        entity.Category = domain.Category;
        entity.AmsDesignation = domain.AmsDesignation;
        entity.UnsDesignation = domain.UnsDesignation;
        entity.IsoDesignation = domain.IsoDesignation;
        entity.Form = domain.Form;
        entity.TemperCondition = domain.TemperCondition;
        return entity;
    }
}
