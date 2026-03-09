using JobHunter.Domain.Models;

namespace JobHunter.Application.Interfaces;

public interface IAnalyticsService
{
    Task<List<string>> GetTablesAsync();
    Task<List<string>> GetColumnsAsync(string tableName);
    Task<List<Dictionary<string, object>>> ExecuteQueryAsync(AnalyticsQueryConfig config);
}
