using JobHunter.Application.Interfaces;
using JobHunter.Domain.Interfaces;
using JobHunter.Domain.Models;

namespace JobHunter.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repository;

    public AnalyticsService(IAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<List<string>> GetTablesAsync()
    {
        return _repository.GetTablesAsync();
    }

    public Task<List<string>> GetColumnsAsync(string tableName)
    {
        return _repository.GetColumnsAsync(tableName);
    }

    public Task<List<Dictionary<string, object>>> ExecuteQueryAsync(AnalyticsQueryConfig config)
    {
        // Business logic validations could be added here
        return _repository.ExecuteQueryAsync(config);
    }
}
