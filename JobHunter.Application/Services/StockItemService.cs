using JobHunter.Application.Interfaces;
using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;

namespace JobHunter.Application.Services;

public class StockItemService : IStockItemService
{
    private readonly IStockItemRepository _repository;

    public StockItemService(IStockItemRepository repository)
    {
        _repository = repository;
    }

    public Task<StockItem?> GetByIdAsync(int id) => _repository.GetByIdAsync(id);

    public Task<IEnumerable<StockItem>> GetAllAsync() => _repository.GetAllAsync();

    public Task<IEnumerable<StockItem>> GetFilteredAsync(
        string? searchText,
        List<string>? stockTypes,
        List<string>? materialCategories,
        int page,
        int pageSize,
        string? sortBy)
        => _repository.GetFilteredAsync(searchText, stockTypes, materialCategories, page, pageSize, sortBy);

    public Task AddAsync(StockItem item) => _repository.AddAsync(item);

    public Task UpdateAsync(StockItem item) => _repository.UpdateAsync(item);

    public Task DeleteAsync(int id) => _repository.DeleteAsync(id);
}
