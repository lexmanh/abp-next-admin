using LY.MicroService.AuthServer.EntityFrameworkCore;
using LY.MicroService.BackendAdmin.EntityFrameworkCore;
using LY.MicroService.IdentityServer.EntityFrameworkCore;
using LY.MicroService.LocalizationManagement.EntityFrameworkCore;
using LY.MicroService.Platform.EntityFrameworkCore;
using LY.MicroService.RealtimeMessage.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace LY.MicroService.All.DbMigrator;

[DependsOn(
    typeof(BackendAdminMigrationsEntityFrameworkCoreModule),
    typeof(PlatformMigrationsEntityFrameworkCoreModule),
    // Localization Management
    typeof(LocalizationManagementMigrationsEntityFrameworkCoreModule),
    // Identity Server
    typeof(IdentityServerMigrationsEntityFrameworkCoreModule),
    // Realtime Message
    typeof(RealtimeMessageMigrationsEntityFrameworkCoreModule),
    // Auth Server
    typeof(AuthServerMigrationsEntityFrameworkCoreModule),
    // // Workflow
    // typeof(WorkflowMigrationsEntityFrameworkCoreModule),
    // // Webhooks
    // typeof(WebhooksMigrationsEntityFrameworkCoreModule),
    typeof(AbpAutofacModule)
    )]
public partial class AllDbMigratorModule : AbpModule
{
}
