using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace LY.MicroService.AuthServer.EntityFrameworkCore;

public class AuthServerMigrationsDbContextFactory : IDesignTimeDbContextFactory<AuthServerMigrationsDbContext>
{
    public AuthServerMigrationsDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        var connectionString = configuration.GetConnectionString("Identity");

        var builder = new DbContextOptionsBuilder<AuthServerMigrationsDbContext>()
            // .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            // Use PostgreSQL
            .UseNpgsql(connectionString, npgsqlOptions =>
            {
                System.AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            })
            ;

        return new AuthServerMigrationsDbContext(builder!.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../LY.MicroService.AuthServer.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false);

        return builder.Build();
    }
}
