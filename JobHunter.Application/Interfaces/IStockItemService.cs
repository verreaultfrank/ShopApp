using JobHunter.Domain.Models;

namespace JobHunter.Application.Interfaces;

public interface IStockItemService
{
    Task<StockItem?> GetByIdAsync(int id);
    Task<IEnumerable<StockItem>> GetAllAsync();
    Task<IEnumerable<StockItem>> GetFilteredAsync(
        string? searchText,
        List<string>? stockTypes,
        List<string>? materialCategories,
        int page,
        int pageSize,
        string? sortBy);
    Task AddAsync(StockItem item);
    Task UpdateAsync(StockItem item);
    Task DeleteAsync(int id);
}
