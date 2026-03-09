using JobHunter.Domain.Models;

namespace JobHunter.Domain.Interfaces;

public interface IJobOpportunityRepository
{
    Task<JobOpportunity?> GetByIdAsync(string id);
    Task<IEnumerable<JobOpportunity>> GetAllAsync();
    Task AddAsync(JobOpportunity job);
    Task UpdateAsync(JobOpportunity job);
    Task DeleteAsync(string id);
}
