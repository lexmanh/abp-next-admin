using LINGYUN.Abp.Data.DbMigrator;
using Volo.Abp.Modularity;

namespace LY.MicroService.All.DbMigrator;

public partial class AllDbMigratorModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDataDbMigratorOptions>(options =>
        {
            options.AllowSeedData = false;
        });
    }
}
