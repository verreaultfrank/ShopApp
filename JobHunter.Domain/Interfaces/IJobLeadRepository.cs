using JobHunter.Domain.Models;

namespace JobHunter.Domain.Interfaces;

public interface IJobLeadRepository
{
    Task<IEnumerable<Lead>> GetLeadsAsync(string searchText = "", IEnumerable<string>? providers = null, IEnumerable<string>? statuses = null, int pageNumber = 1, int pageSize = 20, LeadSortOption sortBy = LeadSortOption.NewestFirst);
    Task UpsertLeadAsync(Lead job);
}
