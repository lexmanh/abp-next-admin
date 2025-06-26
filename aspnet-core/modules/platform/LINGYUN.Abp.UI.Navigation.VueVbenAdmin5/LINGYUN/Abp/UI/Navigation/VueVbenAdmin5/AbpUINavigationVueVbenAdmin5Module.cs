using LINGYUN.Platform;
using Volo.Abp.Modularity;

namespace LINGYUN.Abp.UI.Navigation.VueVbenAdmin5;

[DependsOn(
    typeof(AbpUINavigationModule),
    typeof(PlatformDomainModule))]
public class AbpUINavigationVueVbenAdmin5Module : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            // Uncomment the following line to enable VueVbenAdmin5 navigation seed contributor
            // options.NavigationSeedContributors.Add<VueVbenAdmin5NavigationSeedContributor>();
        });
    }
}
