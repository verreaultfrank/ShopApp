using Microsoft.EntityFrameworkCore;
using JobHunter.Domain.Models;
using JobHunter.Domain.Interfaces;
using JobHunter.Infrastructure.Entities;

namespace JobHunter.Infrastructure.Repositories;

public class BusinessRepository : IBusinessRepository
{
    private readonly IDbContextFactory<JobHunterDbContext> _contextFactory;

    public BusinessRepository(IDbContextFactory<JobHunterDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<IEnumerable<Business>> GetAllAsync()
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var entities = await context.Businesses
            .Include(b => b.Contacts)
            .OrderBy(b => b.Name)
            .ToListAsync();

        return entities.Select(MapToDomain).ToList();
    }

    public async Task<Business?> GetByIdAsync(int id)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var entity = await context.Businesses
            .Include(b => b.Contacts)
            .FirstOrDefaultAsync(b => b.Id == id);

        return entity != null ? MapToDomain(entity) : null;
    }

    public async Task<Business> UpsertAsync(Business business)
    {
        using var context = await _contextFactory.CreateDbContextAsync();

        var existing = business.Id > 0
            ? await context.Businesses.Include(b => b.Contacts).FirstOrDefaultAsync(b => b.Id == business.Id)
            : null;

        if (existing == null)
        {
            var entity = MapToEntity(business);
            context.Businesses.Add(entity);
            await context.SaveChangesAsync();
            return MapToDomain(entity);
        }

        existing.Name = business.Name;
        existing.Address = business.Address;
        existing.Website = business.Website;
        existing.Email = business.Email;

        // Sync contacts
        var incomingIds = business.Contacts.Where(c => c.Id > 0).Select(c => c.Id).ToHashSet();
        existing.Contacts.RemoveAll(c => !incomingIds.Contains(c.Id));

        foreach (var contact in business.Contacts)
        {
            if (contact.Id > 0)
            {
                var existingContact = existing.Contacts.FirstOrDefault(c => c.Id == contact.Id);
                if (existingContact != null)
                {
                    existingContact.FirstName = contact.FirstName;
                    existingContact.LastName = contact.LastName;
                    existingContact.Email = contact.Email;
                    existingContact.Phone = contact.Phone;
                    existingContact.Mobile = contact.Mobile;
                    existingContact.Title = contact.Title;
                }
            }
            else
            {
                existing.Contacts.Add(new ContactEntity
                {
                    BusinessId = existing.Id,
                    FirstName = contact.FirstName,
                    LastName = contact.LastName,
                    Email = contact.Email,
                    Phone = contact.Phone,
                    Mobile = contact.Mobile,
                    Title = contact.Title
                });
            }
        }

        await context.SaveChangesAsync();
        return MapToDomain(existing);
    }

    private static Business MapToDomain(BusinessEntity entity)
    {
        return new Business
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            Website = entity.Website,
            Email = entity.Email,
            Contacts = entity.Contacts?.Select(c => new Contact
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
        };
    }

    private static BusinessEntity MapToEntity(Business business)
    {
        return new BusinessEntity
        {
            Name = business.Name,
            Address = business.Address,
            Website = business.Website,
            Email = business.Email,
            Contacts = business.Contacts?.Select(c => new ContactEntity
            {
                FirstName = c.FirstName,
                LastName = c.LastName,
                Email = c.Email,
                Phone = c.Phone,
                Mobile = c.Mobile,
                Title = c.Title
            }).ToList() ?? new List<ContactEntity>()
        };
    }
}
