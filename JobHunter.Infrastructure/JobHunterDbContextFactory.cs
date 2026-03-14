using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace JobHunter.Infrastructure;

public class JobHunterDbContextFactory : IDesignTimeDbContextFactory<JobHunterDbContext>
{
    public JobHunterDbContext CreateDbContext(string[] args)
    {
        var assemblyFolder = Path.GetDirectoryName(typeof(JobHunterDbContextFactory).Assembly.Location)!;

        // Find the config file by walking up the directory tree instead of hard-coded ".." segments.
        var configFile = FindConfigFile(assemblyFolder, "JobHunterDashboard", "appsettings.json");

        IConfigurationRoot configuration;
        if (configFile != null)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(configFile)!)
                .AddJsonFile(Path.GetFileName(configFile), optional: false, reloadOnChange: false);
            configuration = builder.Build();
        }
        else
        {
            // Fallback to environment variables if the file wasn't found.
            configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .Build();
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("JOBHUNTER__CONNECTION");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string not found. Ensure appsettings.json is reachable or set JOBHUNTER__CONNECTION environment variable.");

        var optionsBuilder = new DbContextOptionsBuilder<JobHunterDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new JobHunterDbContext(optionsBuilder.Options);
    }

    private static string? FindConfigFile(string startFolder, string targetFolderName, string fileName)
    {
        var dir = new DirectoryInfo(startFolder);
        while (dir != null)
        {
            // Prefer JobHunterDashboard/appsettings.json when present
            var candidate = Path.Combine(dir.FullName, targetFolderName, fileName);
            if (File.Exists(candidate))
                return candidate;

            // Also accept an appsettings.json at the current level
            var candidate2 = Path.Combine(dir.FullName, fileName);
            if (File.Exists(candidate2))
                return candidate2;

            dir = dir.Parent;
        }
        return null;
    }
}
