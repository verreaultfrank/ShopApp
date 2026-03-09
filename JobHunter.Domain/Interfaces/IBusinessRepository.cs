using JobHunter.Domain.Models;

namespace JobHunter.Domain.Interfaces;

public interface IBusinessRepository
{
    Task<IEnumerable<Business>> GetAllAsync();
    Task<Business?> GetByIdAsync(int id);
    Task<Business> UpsertAsync(Business business);
}
