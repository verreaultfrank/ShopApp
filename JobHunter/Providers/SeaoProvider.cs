using JobHunter.Domain.Models;
using Microsoft.Extensions.Logging;

namespace JobHunterCore.Providers;

/// <summary>
/// SEAO (Système électronique d'appel d'offres) provides open data in JSON/XML for provincial Quebec contracts.
/// </summary>
public class SeaoProvider : RestApiJobProvider
{
    public SeaoProvider(HttpClient httpClient, ILogger<SeaoProvider> logger) 
        : base(httpClient, logger)
    {
    }

    public override string ProviderName => "SEAO Quebec";
    public override bool SupportsAutomation => false;
    public override bool SupportsApiQuoting => false;

    protected override HttpRequestMessage BuildRequest()
    {
        // Interfacing with DonneesQuebec API for SEAO JSON dataset
        return new HttpRequestMessage(HttpMethod.Get, "https://www.donneesquebec.ca/recherche/api/3/action/package_show?id=seao");
    }

    protected override async Task<IEnumerable<JobOpportunity>> ParseResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Parsing SEAO data dump.");
        var opportunities = new List<JobOpportunity>();
        
        // Mock parsing
        opportunities.Add(new JobOpportunity 
        {
            Id = "SEAO-9990",
            Title = "Usinage de pièces d'aluminium - Ministère de Transports",
            IsAutomationPossible = true
        });

        return opportunities;
    }
}
