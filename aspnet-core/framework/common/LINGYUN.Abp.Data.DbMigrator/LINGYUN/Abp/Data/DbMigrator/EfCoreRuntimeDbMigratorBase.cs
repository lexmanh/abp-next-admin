using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore.Migrations;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace LINGYUN.Abp.Data.DbMigrator;
public abstract class EfCoreRuntimeDbMigratorBase<TDbContext> : EfCoreRuntimeDatabaseMigratorBase<TDbContext>
    where TDbContext : DbContext, IEfCoreDbContext
{
    protected AbpDataDbMigratorOptions DataDbMigratorOptions { get; }
    
    protected EfCoreRuntimeDbMigratorBase(
        string databaseName,
        IUnitOfWorkManager unitOfWorkManager,
        IServiceProvider serviceProvider,
        ICurrentTenant currentTenant,
        IAbpDistributedLock abpDistributedLock,
        IDistributedEventBus distributedEventBus,
        ILoggerFactory loggerFactory,
        IOptions<AbpDataDbMigratorOptions> dataDbMigratorOptions)
        : base(databaseName, unitOfWorkManager, serviceProvider, currentTenant, abpDistributedLock, distributedEventBus, loggerFactory)
    {
        DataDbMigratorOptions = dataDbMigratorOptions.Value;
    }

    protected async virtual Task LockAndApplyDatabaseWithTenantMigrationsAsync(Guid tenantId)
    {
        Logger.LogInformation($"Trying to acquire the distributed lock for database migration: {DatabaseName} with tenant: {tenantId}.");

        var schemaMigrated = false;

        await using (var handle = await DistributedLock.TryAcquireAsync("DatabaseMigration_" + DatabaseName + "_Tenant" + tenantId.ToString()))
        {
            if (handle is null)
            {
                Logger.LogInformation($"Distributed lock could not be acquired for database migration: {DatabaseName} with tenant: {tenantId}. Operation cancelled.");
                return;
            }

            Logger.LogInformation($"Distributed lock is acquired for database migration: {DatabaseName} with tenant: {tenantId}...");

            using (CurrentTenant.Change(tenantId))
            {
                // Create database tables if needed
                using var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false);
                var dbContext = await ServiceProvider
                    .GetRequiredService<IDbContextProvider<TDbContext>>()
                    .GetDbContextAsync();

                var pendingMigrations = await dbContext
                    .Database
                    .GetPendingMigrationsAsync();

                if (pendingMigrations.Any())
                {
                    await dbContext.Database.MigrateAsync();
                    schemaMigrated = true;
                }
                else
                {
                    // Always run migrations to ensure the database is up-to-date
                    Logger.LogInformation($"No pending migrations found for database: {DatabaseName} with tenant: {tenantId}. But ensuring the database is up-to-date.");
                    await dbContext.Database.MigrateAsync();
                }

                await uow.CompleteAsync();

                await SeedAsync();
            }

            if (schemaMigrated || AlwaysSeedTenantDatabases)
            {
                await DistributedEventBus.PublishAsync(
                    new AppliedDatabaseMigrationsEto
                    {
                        DatabaseName = DatabaseName,
                        TenantId = null
                    }
                );
            }
        }

        Logger.LogInformation($"Distributed lock has been released for database migration: {DatabaseName} with tenant: {tenantId}...");
    }
    
    /// <summary>
    /// Overrides the method to apply database migrations for the current tenant or host database.
    /// </summary>
    protected async override Task LockAndApplyDatabaseMigrationsAsync()
    {
        Logger.LogInformation($"Trying to acquire the distributed lock for database migration: {DatabaseName}.");

        var schemaMigrated = false;
        
        await using (var handle = await DistributedLock.TryAcquireAsync("DatabaseMigration_" + DatabaseName))
        {
            if (handle is null)
            {
                Logger.LogInformation($"Distributed lock could not be acquired for database migration: {DatabaseName}. Operation cancelled.");
                return;
            }
            
            Logger.LogInformation($"Distributed lock is acquired for database migration: {DatabaseName}...");

            using (CurrentTenant.Change(null))
            {
                // Create database tables if needed
                using (var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
                {
                    var dbContext = await ServiceProvider
                        .GetRequiredService<IDbContextProvider<TDbContext>>()
                        .GetDbContextAsync();

                    var pendingMigrations = await dbContext
                        .Database
                        .GetPendingMigrationsAsync();

                    if (pendingMigrations.Any())
                    {
                        await dbContext.Database.MigrateAsync();
                        schemaMigrated = true;
                    }
                    else
                    {
                        // TODO: Always run migrations to ensure the database is up-to-date
                        Logger.LogInformation($"No pending migrations found for database: {DatabaseName}. But ensuring the database is up-to-date.");
                        await dbContext.Database.MigrateAsync();
                    }

                    await uow.CompleteAsync();
                }
            }

            await SeedAsync();
            
            if (schemaMigrated || AlwaysSeedTenantDatabases)
            {
                await DistributedEventBus.PublishAsync(
                    new AppliedDatabaseMigrationsEto
                    {
                        DatabaseName = DatabaseName,
                        TenantId = null
                    }
                );
            }
        }
        
        Logger.LogInformation($"Distributed lock has been released for database migration: {DatabaseName}...");
    }

    protected override Task SeedAsync()
    {
        if(!DataDbMigratorOptions.AllowSeedData)
        {
            Logger.LogInformation($"Data seeding is disabled for database: {DatabaseName}. Skipping seed operation.");
            return Task.CompletedTask;
        }
        
        return base.SeedAsync();
    }
}
