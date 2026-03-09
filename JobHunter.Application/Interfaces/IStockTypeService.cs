using JobHunter.Domain.Models;

namespace JobHunter.Application.Interfaces;

public interface IStockTypeService
{
    Task<StockType?> GetByIdAsync(int id);
    Task<IEnumerable<StockType>> GetAllAsync();
}
