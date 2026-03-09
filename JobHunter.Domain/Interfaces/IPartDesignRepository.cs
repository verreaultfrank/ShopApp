using JobHunter.Domain.Models;

namespace JobHunter.Domain.Interfaces;

public interface IPartDesignRepository
{
    Task<IEnumerable<PartDesign>> GetAllAsync();
    Task<IEnumerable<PartDesign>> SearchAsync(string searchTerm);
}
