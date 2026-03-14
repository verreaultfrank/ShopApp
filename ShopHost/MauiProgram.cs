using JobHunter.Application.Interfaces;
using JobHunter.Application.Services;
using JobHunter.Domain.Interfaces;
using JobHunter.Infrastructure;
using JobHunter.Infrastructure.Repositories;
using JobHunterDashboard.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;


namespace ShopHost
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            try
            {
                var builder = MauiApp.CreateBuilder();
                builder
                    .UseMauiApp<App>()
                    .ConfigureFonts(fonts =>
                    {
                        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    });



                var a = Assembly.GetExecutingAssembly();
                using var stream = a.GetManifestResourceStream("ShopHost.appsettings.json");

                var config = new ConfigurationBuilder()
                    .AddJsonStream(stream!)
                    .Build();

                builder.Configuration.AddConfiguration(config);

                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "";

                builder.Services.AddDbContextFactory<JobHunterDbContext>(options =>
                    options.UseSqlServer(connectionString));

                builder.Services.AddSingleton<JobHunter.Infrastructure.Services.IEmailService, JobHunter.Infrastructure.Services.EmailService>();
                builder.Services.AddSingleton<IJobLeadRepository, LeadRepository>();
                builder.Services.AddSingleton<ILeadWorkflowService, LeadWorkflowService>();
                builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
                builder.Services.AddSingleton<IJobStatusRepository, LeadStatusRepository>();
                builder.Services.AddSingleton<IBusinessRepository, BusinessRepository>();
                builder.Services.AddSingleton<IBusinessService, BusinessService>();

                builder.Services.AddTransient<MainViewModel>();
                builder.Services.AddTransient<JobHunterDashboard.Views.MainPage>();

                builder.Services.AddTransient<SettingsViewModel>();
                builder.Services.AddTransient<JobHunterDashboard.Views.SettingsPage>();

                builder.Services.AddSingleton<IPartDesignRepository, PartDesignRepository>();
                builder.Services.AddTransient<PartDesignsViewModel>();
                builder.Services.AddTransient<JobHunterDashboard.Views.PartDesignsPage>();

                builder.Services.AddSingleton<IMaterialRepository, MaterialRepository>();
                builder.Services.AddSingleton<IStockTypeRepository, StockTypeRepository>();
                builder.Services.AddSingleton<IStockTypeService, StockTypeService>();
                builder.Services.AddSingleton<IStockItemRepository, StockItemRepository>();
                builder.Services.AddSingleton<IStockItemService, StockItemService>();
                builder.Services.AddTransient<InventoryViewModel>();
                builder.Services.AddTransient<JobHunterDashboard.Views.InventoryPage>();

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
}
