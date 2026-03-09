using JobHunter.Application.Interfaces;
using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;

namespace JobHunter.Application.Services;

public class BusinessService : IBusinessService
{
    private readonly IBusinessRepository _repository;

    public BusinessService(IBusinessRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Business>> GetAllBusinessesAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Business?> GetBusinessWithContactsAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }
}
