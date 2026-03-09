using JobHunter.Domain.Models;
using Microsoft.Extensions.Logging;

namespace JobHunterCore.Providers;

/// <summary>
/// MERX provides APIs often via a developer portal / SOAP / REST depending on the client tier, very common for municipalities and private.
/// </summary>
public class MerxProvider : RestApiJobProvider
{
    public MerxProvider(HttpClient httpClient, ILogger<MerxProvider> logger) 
        : base(httpClient, logger)
    {
    }

    public override string ProviderName => "MERX B2B & Public";
    public override bool SupportsAutomation => false; // Requires premium
    public override bool SupportsApiQuoting => false;

    protected override HttpRequestMessage BuildRequest()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.merx.com/v1/solicitations/active");
        // Requires authentication based on account tier
        request.Headers.Add("Authorization", "Bearer YOUR_MERX_TOKEN");
        return request;
    }

    protected override async Task<IEnumerable<JobOpportunity>> ParseResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var opportunities = new List<JobOpportunity>();
        _logger.LogInformation("Parsing MERX solicitation response.");
        return opportunities;
    }
}
