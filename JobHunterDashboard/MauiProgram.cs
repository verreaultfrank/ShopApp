using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Maui.Core.Hosting;
using JobHunter.Domain.Models;
using JobHunter.Domain.Interfaces;
using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Infrastructure;
using JobHunter.Infrastructure.Repositories;
using JobHunter.Infrastructure.Services;
using JobHunterDashboard.ViewModels;

namespace JobHunterDashboard;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        try
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureSyncfusionCore()
                .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });



        var a = Assembly.GetExecutingAssembly();
        using var stream = a.GetManifestResourceStream("JobHunterDashboard.appsettings.json");

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream!)
            .Build();

        builder.Configuration.AddConfiguration(config);

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

        builder.Services.AddDbContextFactory<JobHunterDbContext>(options =>
            options.UseSqlServer(connectionString));

        builder.Services.AddSingleton<IEmailService, EmailService>();
        builder.Services.AddSingleton<IJobLeadRepository, JobLeadRepository>();
        builder.Services.AddSingleton<ILeadWorkflowService, LeadWorkflowService>();
        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
        builder.Services.AddSingleton<IJobStatusRepository, JobStatusRepository>();
        builder.Services.AddSingleton<IBusinessRepository, BusinessRepository>();
        builder.Services.AddSingleton<IBusinessService, BusinessService>();

        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<Views.MainPage>();

        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<Views.SettingsPage>();

        builder.Services.AddSingleton<IPartDesignRepository, PartDesignRepository>();
        builder.Services.AddTransient<PartDesignsViewModel>();
        builder.Services.AddTransient<Views.PartDesignsPage>();

        builder.Services.AddSingleton<IAnalyticsRepository, AnalyticsRepository>();
        builder.Services.AddSingleton<IAnalyticsService, AnalyticsService>();
        builder.Services.AddTransient<AnalyticsViewModel>();
        builder.Services.AddTransient<Views.AnalyticsPage>();

        builder.Services.AddSingleton<IMaterialRepository, MaterialRepository>();
        builder.Services.AddSingleton<IStockTypeRepository, StockTypeRepository>();
        builder.Services.AddSingleton<IStockTypeService, StockTypeService>();
        builder.Services.AddSingleton<IStockItemRepository, StockItemRepository>();
        builder.Services.AddSingleton<IStockItemService, StockItemService>();
        builder.Services.AddTransient<InventoryViewModel>();
        builder.Services.AddTransient<Views.InventoryPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

        return builder.Build();
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText(@"C:\Temp\maui_crash.txt", ex.ToString());
            throw;
        }
    }
}
