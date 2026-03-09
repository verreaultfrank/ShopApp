using JobHunter.Domain.Models;

namespace JobHunter.Domain.Interfaces;

public interface IMaterialRepository
{
    Task<Material?> GetByIdAsync(int id);
    Task<IEnumerable<Material>> GetAllAsync();
    Task AddAsync(Material material);
    Task UpdateAsync(Material material);
    Task DeleteAsync(int id);
}
