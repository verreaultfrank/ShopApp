using JobHunter.Domain.Models;

namespace JobHunter.Domain.Interfaces;

public interface IJobStatusRepository
{
    Task<IEnumerable<LeadStatus>> GetAllStatusesAsync();
}
