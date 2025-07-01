using LY.MicroService.AuthServer.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.IO;
using Volo.Abp.Modularity.PlugIns;

namespace LY.MicroService.AuthServer;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        try
        {
            Console.Title = "AuthServer";
            Log.Information("Starting AuthServer.");
 
            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .ConfigureAppConfiguration((context, config) =>
                {
                    var configuration = config.Build();
                    var agileConfigEnabled = configuration["AgileConfig:IsEnabled"];
                    if (agileConfigEnabled.IsNullOrEmpty() || bool.Parse(agileConfigEnabled))
                    {
                        config.AddAgileConfig(new AgileConfig.Client.ConfigClient(configuration));
                    }
                })
                .UseSerilog((context, provider, config) =>
                {
                    config.ReadFrom.Configuration(context.Configuration);
                });
            await builder.AddApplicationAsync<AuthServerModule>(options =>
            {
                AuthServerModule.ApplicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME")
                    ?? AuthServerModule.ApplicationName;
                options.ApplicationName = AuthServerModule.ApplicationName;
                // Get user confidential configuration from environment variables, suitable for container testing
                options.Configuration.UserSecretsId = Environment.GetEnvironmentVariable("APPLICATION_USER_SECRETS_ID");
                // If the container does not specify a user secret, read from the project
                options.Configuration.UserSecretsAssembly = typeof(AuthServerModule).Assembly;
                // Search all files in the Modules directory as plug-ins
                // Undisplay modules that reference all other projects and refer to them via plug-ins instead.
                var pluginFolder = Path.Combine(
                        Directory.GetCurrentDirectory(), "Modules");
                DirectoryHelper.CreateIfNotExists(pluginFolder);
                options.PlugInSources.AddFolder(
                    pluginFolder,
                    SearchOption.AllDirectories);
            });
            var app = builder.Build();
            await app.InitializeApplicationAsync();
            using (var scope = app.Services.CreateScope())
            {
                var migrationService = scope.ServiceProvider.GetRequiredService<AuthServerDbMigrationService>();
                await migrationService.CheckAndApplyDatabaseMigrationsAsync();
            }
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            Console.WriteLine("Host terminated unexpectedly!");
            Console.WriteLine(ex.ToString());
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}

