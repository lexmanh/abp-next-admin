using LY.MicroService.AuthServer.EntityFrameworkCore;
using LY.MicroService.BackendAdmin.EntityFrameworkCore;
using LY.MicroService.IdentityServer.EntityFrameworkCore;
using LY.MicroService.LocalizationManagement.EntityFrameworkCore;
using LY.MicroService.Platform.EntityFrameworkCore;
using LY.MicroService.RealtimeMessage.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Volo.Abp;
using Volo.Abp.Data;

namespace LY.MicroService.All.DbMigrator;

public class AllDbMigratorHostedService : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IConfiguration _configuration;

    public AllDbMigratorHostedService(
        IHostApplicationLifetime hostApplicationLifetime, 
        IConfiguration configuration)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _configuration = configuration;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var application = await AbpApplicationFactory
            .CreateAsync<AllDbMigratorModule>(options =>
        {
            // Get user confidential configuration from environment variables, suitable for container testing
            options.Configuration.UserSecretsId = Environment.GetEnvironmentVariable("APPLICATION_USER_SECRETS_ID");
            // If the container does not specify a user secret, read from the project
            options.Configuration.UserSecretsAssembly = typeof(AllDbMigratorHostedService).Assembly;
            options.Services.ReplaceConfiguration(_configuration);
            options.UseAutofac();
            options.Services.AddLogging(c => c.AddSerilog());
            options.AddDataMigrationEnvironment();
        });
        await application.InitializeAsync();
        
        Log.Information("Starting migration for all databases...");
        

        // 3. Migrate Language Database
        Log.Information("Migrating Localization Management Database...");
        try
        {
            await application
                .ServiceProvider
                .GetRequiredService<LocalizationManagementDbMigrationService>()
                .CheckAndApplyDatabaseMigrationsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the Localization Management Database.");
            throw;
        }
        
        // 2. Migrate Administration Database
        Log.Information("Migrating Backend Admin Database...");
        await application
            .ServiceProvider
            .GetRequiredService<BackendAdminDbMigrationService>()
            .CheckAndApplyDatabaseMigrationsAsync();
        
        // 1. Migrate Platform Database
        Log.Information("Migrating Platform Database...");
        try
        {
            await application
                .ServiceProvider
                .GetRequiredService<PlatformDbMigrationService>()
                .CheckAndApplyDatabaseMigrationsAsync();

            
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the Platform or Backend Admin Database.");
            throw;
        }

        // 4. Migrate Auth Server Database
        Log.Information("Migrating Auth Server Database...");
        try
        {
            await application
                .ServiceProvider
                .GetRequiredService<AuthServerDbMigrationService>()
                .CheckAndApplyDatabaseMigrationsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the Auth Server Database.");
            throw;
        }

        // 5. Migrate Identity Server Database
        Log.Information("Migrating Identity Server Database...");
        try
        {
            await application
                .ServiceProvider
                .GetRequiredService<IdentityServerDbMigrationService>()
                .CheckAndApplyDatabaseMigrationsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the Identity Server Database.");
            throw;
        }

        // 6. Migrate Message Database
        Log.Information("Migrating Realtime Message Database...");
        try
        {
            await application
                .ServiceProvider
                .GetRequiredService<RealtimeMessageDbMigrationService>()
                .CheckAndApplyDatabaseMigrationsAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while migrating the Realtime Message Database.");
            throw;
        }

        // 7. Migrate Task Database
        // await application
        //     .ServiceProvider
        //     .GetRequiredService<TaskManagementDbMigrationService>()
        //     .CheckAndApplyDatabaseMigrationsAsync();
        
        // 8. Migrate Workflow Database
        // await application
        //     .ServiceProvider
        //     .GetRequiredService<WorkflowManagementDbMigrationService>()
        //     .CheckAndApplyDatabaseMigrationsAsync();
        
        // 9. Migrate Webhook Database
        // await application
        //     .ServiceProvider
        //     .GetRequiredService<WebhookManagementDbMigrationService>()
        //     .CheckAndApplyDatabaseMigrationsAsync();
        
        Log.Information("All databases migration completed successfully.");

        await application.ShutdownAsync();

        _hostApplicationLifetime.StopApplication();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}

