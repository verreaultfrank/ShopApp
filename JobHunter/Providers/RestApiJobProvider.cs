using JobHunter.Domain.Models;
using Microsoft.Extensions.Logging;

namespace JobHunterCore.Providers;

/// <summary>
/// A more specialized abstraction for any provider that relies on REST API over HTTP.
/// </summary>
public abstract class RestApiJobProvider : BaseJobProvider
{
    protected readonly HttpClient _httpClient;

    protected RestApiJobProvider(HttpClient httpClient, ILogger<RestApiJobProvider> logger) 
        : base(logger)
    {
        _httpClient = httpClient;
    }

    public override bool SupportsAutomation => true;

    protected override async Task<IEnumerable<JobOpportunity>> ExecuteFetchAsync(CancellationToken cancellationToken)
    {
        var request = BuildRequest();
        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await ParseResponseAsync(response, cancellationToken);
    }

    protected abstract HttpRequestMessage BuildRequest();
    protected abstract Task<IEnumerable<JobOpportunity>> ParseResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken);
}
