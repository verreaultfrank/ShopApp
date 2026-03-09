using JobHunter.Domain.Models;

namespace JobHunter.Domain.Interfaces;

public interface IStockTypeRepository
{
    Task<StockType?> GetByIdAsync(int id);
    Task<IEnumerable<StockType>> GetAllAsync();
}
