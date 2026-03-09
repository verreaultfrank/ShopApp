using JobHunter.Application.Interfaces;
using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;

namespace JobHunter.Application.Services;

public class StockTypeService : IStockTypeService
{
    private readonly IStockTypeRepository _repository;

    public StockTypeService(IStockTypeRepository repository)
    {
        _repository = repository;
    }

    public Task<StockType?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<IEnumerable<StockType>> GetAllAsync() => _repository.GetAllAsync();
}
