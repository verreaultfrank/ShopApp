using JobHunter.Domain.Models;

namespace JobHunter.Domain.Interfaces;

public interface IJobOpportunityRepository
{
    Task<Lead?> GetByIdAsync(string id);
    Task<IEnumerable<Lead>> GetAllAsync();
    Task AddAsync(Lead job);
    Task UpdateAsync(Lead job);
    Task DeleteAsync(string id);
}
