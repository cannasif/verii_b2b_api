using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Wms.Infrastructure.Persistence.Context;

namespace Wms.Infrastructure.Persistence;

public sealed class WmsDbContextFactory : IDesignTimeDbContextFactory<WmsDbContext>
{
    public WmsDbContext CreateDbContext(string[] args)
    {
        var basePath = ResolveConfigurationBasePath();

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("Default")
            ?? "Data Source=wms.db";

        var optionsBuilder = new DbContextOptionsBuilder<WmsDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new WmsDbContext(optionsBuilder.Options);
    }

    private static string ResolveConfigurationBasePath()
    {
        var current = new DirectoryInfo(Directory.GetCurrentDirectory());

        while (current is not null)
        {
            var sharedConfigPath = Path.Combine(current.FullName, "Shared", "Host", "WebApi", "Config", "appsettings.json");
            if (File.Exists(sharedConfigPath))
            {
                return Path.GetDirectoryName(sharedConfigPath)!;
            }

            var rootAppsettingsPath = Path.Combine(current.FullName, "appsettings.json");
            if (File.Exists(rootAppsettingsPath))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("appsettings.json bulunamadi. Design-time DbContext olusturulamiyor.");
    }
}
