using JobHunterCore.Interfaces;
using JobHunterCore.Providers;
using JobHunterCore.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    // 1. Register HttpClient with Dependency Injection for resilient REST calls
    services.AddHttpClient<CanadaBuysProvider>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = true
        });
    services.AddHttpClient<SeaoProvider>();
    services.AddHttpClient<MerxProvider>();
    services.AddHttpClient<XometryProvider>();

    // 2. Register all Providers mapped to the interface.
    // When JobHunterWorker asks for IEnumerable<IJobProvider>, it will get all of these!
    services.AddTransient<IJobProvider, CanadaBuysProvider>();
    services.AddTransient<IJobProvider, SeaoProvider>();
    services.AddTransient<IJobProvider, MerxProvider>();
    services.AddTransient<IJobProvider, XometryProvider>();

    // 3. Register the Background Service Worker
    services.AddHostedService<JobHunterWorker>();
});

var host = builder.Build();
host.Run();
