using JobHunterCore.Interfaces;
using JobHunter.Domain.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobHunterCore.Services;

/// <summary>
/// A background worker service that runs once or twice a day to fetch and aggregate jobs across all providers.
/// </summary>
public class JobHunterWorker : BackgroundService
{
    private readonly IEnumerable<IJobProvider> _providers;
    private readonly ILogger<JobHunterWorker> _logger;
    private readonly TimeSpan _runInterval = TimeSpan.FromHours(12); // Twice a day

    public JobHunterWorker(IEnumerable<IJobProvider> providers, ILogger<JobHunterWorker> logger)
    {
        _providers = providers;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobHunterWorker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Starting scheduled job fetch at: {time}", DateTimeOffset.Now);

            var allJobs = new List<Lead>();

            // Iterate over all injected providers Polymorphically
            foreach (var provider in _providers)
            {
                var jobs = await provider.FetchJobsAsync(stoppingToken);
                allJobs.AddRange(jobs);
            }

            _logger.LogInformation("Fetched a total of {count} jobs across {providerCount} providers.", allJobs.Count, _providers.Count());

            await UpdateDashboardAsync(allJobs, stoppingToken);

            _logger.LogInformation("Sleeping for {interval} before next run...", _runInterval);
            await Task.Delay(_runInterval, stoppingToken);
        }
    }

    private Task UpdateDashboardAsync(IEnumerable<Lead> jobs, CancellationToken stoppingToken)
    {
        // This is where you would push the aggregated 'jobs' array to a Database (Entity Framework),
        // a Redis Cache, or directly trigger the next layer of your AI, the 'Submissionner' Agent.
        _logger.LogInformation("Dashboard updated with {count} new job listings. Triggering review pipeline.", jobs.Count());
        return Task.CompletedTask;
    }
}
