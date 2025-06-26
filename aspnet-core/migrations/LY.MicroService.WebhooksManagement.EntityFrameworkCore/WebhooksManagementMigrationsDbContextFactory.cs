using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LY.MicroService.WebhooksManagement.EntityFrameworkCore;

public class WebhooksManagementMigrationsDbContextFactory : IDesignTimeDbContextFactory<WebhooksManagementMigrationsDbContext>
{
    public WebhooksManagementMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("Default");

        var builder = new DbContextOptionsBuilder<WebhooksManagementMigrationsDbContext>()
            // .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            // Use PostgreSQL
            .UseNpgsql(connectionString, npgsqlOptions =>
            {
                System.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            })
            ;

        return new WebhooksManagementMigrationsDbContext(builder!.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../LY.MicroService.WebhooksManagement.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
