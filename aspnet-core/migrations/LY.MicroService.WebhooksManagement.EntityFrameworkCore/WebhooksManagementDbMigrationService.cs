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

namespace LY.MicroService.WebhooksManagement.EntityFrameworkCore;

public class WebhooksManagementDbMigrationService : EfCoreRuntimeDbMigratorBase<WebhooksManagementMigrationsDbContext>, ITransientDependency
{
    protected IDataSeeder DataSeeder { get; }
    protected IDbSchemaMigrator DbSchemaMigrator { get; }
    protected ITenantRepository TenantRepository { get; }
    protected AbpDataDbMigratorOptions DbMigratorOptions { get; }

    public WebhooksManagementDbMigrationService(
        IDataSeeder dataSeeder,
        IDbSchemaMigrator dbSchemaMigrator,
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager unitOfWorkManager,
        IServiceProvider serviceProvider,
        IAbpDistributedLock abpDistributedLock,
        IDistributedEventBus distributedEventBus,
        IOptions<AbpDataDbMigratorOptions> dataDbMigratorOptions,
        ILoggerFactory loggerFactory)
        : base(
            ConnectionStringNameAttribute.GetConnStringName<WebhooksManagementMigrationsDbContext>(),
            unitOfWorkManager, serviceProvider, currentTenant, abpDistributedLock, distributedEventBus,
            loggerFactory, dataDbMigratorOptions)
    {
        DataSeeder = dataSeeder;
        DbSchemaMigrator = dbSchemaMigrator;
        TenantRepository = tenantRepository;
        DbMigratorOptions = dataDbMigratorOptions.Value;
    }

    protected async override Task LockAndApplyDatabaseMigrationsAsync()
    {
        await base.LockAndApplyDatabaseMigrationsAsync();

        var tenants = await TenantRepository.GetListAsync();
        foreach (var tenant in tenants.Where(x => x.IsActive))
        {
            await LockAndApplyDatabaseWithTenantMigrationsAsync(tenant.Id);
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