using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace JobHunter.Infrastructure;

public class JobHunterDbContextFactory : IDesignTimeDbContextFactory<JobHunterDbContext>
{
    public JobHunterDbContext CreateDbContext(string[] args)
    {
        // Build configuration
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("../JobHunterDashboard/appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<JobHunterDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        optionsBuilder.UseSqlServer(connectionString);

        return new JobHunterDbContext(optionsBuilder.Options);
    }
}
