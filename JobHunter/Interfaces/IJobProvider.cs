using JobHunter.Domain.Models;

namespace JobHunterCore.Interfaces;

public interface IJobProvider
{
    string ProviderName { get; }
    
    /// <summary>
    /// Indicates if this provider exposes structured data (APIs) suitable for complete automation, 
    /// or if it relies on scraping/PDF reading.
    /// </summary>
    bool SupportsAutomation { get; }

    Task<IEnumerable<JobOpportunity>> FetchJobsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Indicates if this provider supports automated quoting via API.
    /// </summary>
    bool SupportsApiQuoting { get; }

    /// <summary>
    /// Submits a quote to the provider via API.
    /// </summary>
    Task<bool> SubmitQuoteAsync(JobOpportunity job, Quote quote);
}
