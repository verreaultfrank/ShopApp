using JobHunter.Domain.Models;

namespace JobHunter.Application.Interfaces;

public interface IBusinessService
{
    Task<IEnumerable<Business>> GetAllBusinessesAsync();
    Task<Business?> GetBusinessWithContactsAsync(int id);
}
