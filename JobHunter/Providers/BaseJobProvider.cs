using JobHunterCore.Interfaces;
using JobHunter.Domain.Models;
using Microsoft.Extensions.Logging;

namespace JobHunterCore.Providers;

public abstract class BaseJobProvider : IJobProvider
{
    protected readonly ILogger<BaseJobProvider> _logger;

    protected BaseJobProvider(ILogger<BaseJobProvider> logger)
    {
        _logger = logger;
    }

    public abstract string ProviderName { get; }
    public abstract bool SupportsAutomation { get; }
    public abstract bool SupportsApiQuoting { get; }

    public async Task<IEnumerable<Lead>> FetchJobsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting job fetch for provider: {ProviderName}", ProviderName);
        try
        {
            return await ExecuteFetchAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching jobs from {ProviderName}", ProviderName);
            return Enumerable.Empty<Lead>();
        }
    }

    /// <summary>
    /// Underlying abstract execution pattern to ensure error handling is wrapped globally.
    /// </summary>
    protected abstract Task<IEnumerable<Lead>> ExecuteFetchAsync(CancellationToken cancellationToken);

    public virtual Task<bool> SubmitQuoteAsync(Lead job, Quote quote)
    {
        if (!SupportsApiQuoting)
        {
            _logger.LogWarning("Provider {ProviderName} does not support automated API quoting.", ProviderName);
            return Task.FromResult(false);
        }

        return ExecuteSubmitQuoteAsync(job, quote);
    }

    protected virtual Task<bool> ExecuteSubmitQuoteAsync(Lead job, Quote quote)
    {
        return Task.FromResult(false);
    }
}
