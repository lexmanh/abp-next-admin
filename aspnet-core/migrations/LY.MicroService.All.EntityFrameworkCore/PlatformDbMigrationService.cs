using LINGYUN.Abp.Data.DbMigrator;
using LINGYUN.Abp.Saas.Tenants;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace LY.MicroService.All.EntityFrameworkCore;

public class PlatformDbMigrationService : EfCoreRuntimeDbMigratorBase<PlatformMigrationsDbContext>, ITransientDependency
{
    protected IDataSeeder DataSeeder { get; }
    protected IDbSchemaMigrator DbSchemaMigrator { get; }
    protected ITenantRepository TenantRepository { get; }
    
    protected AbpDataDbMigratorOptions DbMigratorOptions { get; }

    public PlatformDbMigrationService(
        IDataSeeder dataSeeder,
        IDbSchemaMigrator dbSchemaMigrator,
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        IServiceProvider serviceProvider,
        IAbpDistributedLock abpDistributedLock,
        IOptions<AbpDataDbMigratorOptions> dataDbMigratorOptions,
        IDistributedEventBus distributedEventBus,
        ILoggerFactory loggerFactory)
        : base(
            ConnectionStringNameAttribute.GetConnStringName<PlatformMigrationsDbContext>(),
            unitOfWorkManager, serviceProvider, currentTenant, abpDistributedLock, distributedEventBus, loggerFactory,
            dataDbMigratorOptions)
    {
        DataSeeder = dataSeeder;
        DbSchemaMigrator = dbSchemaMigrator;
        TenantRepository = tenantRepository;
        DbMigratorOptions = dataDbMigratorOptions.Value;
    }

    protected async override Task LockAndApplyDatabaseMigrationsAsync()
    {
        await base.LockAndApplyDatabaseMigrationsAsync();

        try{
        var tenants = await TenantRepository.GetListAsync();
        foreach (var tenant in tenants.Where(x => x.IsActive))
        {
            await LockAndApplyDatabaseWithTenantMigrationsAsync(tenant.Id);
        }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "An error occurred while applying database migrations for tenants.");
            throw;
        }
    }

    protected async override Task SeedAsync()
    {
        if (!DataDbMigratorOptions.AllowSeedData)
        {
            Logger.LogInformation("Data seeding is disabled. Skipping data seeding.");
            return;
        }
        
        Logger.LogInformation($"Executing {(!CurrentTenant.IsAvailable ? "host" : CurrentTenant.Name ?? CurrentTenant.GetId().ToString())} database seed...");

        await DataSeeder.SeedAsync(CurrentTenant.Id);
    }
}