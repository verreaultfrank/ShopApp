using JobHunter.Domain.Models;

namespace JobHunter.Domain.Interfaces;

public interface IAnalyticsRepository
{
    Task<List<string>> GetTablesAsync();
    Task<List<string>> GetColumnsAsync(string tableName);
    Task<List<Dictionary<string, object>>> ExecuteQueryAsync(AnalyticsQueryConfig config);
}
