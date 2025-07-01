using LINGYUN.Abp.LocalizationManagement;
using Volo.Abp.Modularity;

namespace LY.MicroService.LocalizationManagement.DbMigrator;
public partial class LocalizationManagementDbMigratorModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationManagementOptions>(options =>
        {
            options.SaveStaticLocalizationsToDatabase = false;
        });
    }
}
