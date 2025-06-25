using LINGYUN.Platform.Datas;
using LINGYUN.Platform.Layouts;
using LINGYUN.Platform.Menus;
using LINGYUN.Platform.Routes;
using LINGYUN.Platform.Utils;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using ValueType = LINGYUN.Platform.Datas.ValueType;

namespace LINGYUN.Abp.UI.Navigation.VueVbenAdmin;

public class VueVbenAdminNavigationSeedContributor : NavigationSeedContributor
{
    private static int _lastCodeNumber = 0;
    protected ICurrentTenant CurrentTenant { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected IRouteDataSeeder RouteDataSeeder { get; }
    protected IDataDictionaryDataSeeder DataDictionaryDataSeeder { get; }
    protected IMenuRepository MenuRepository { get; }
    protected ILayoutRepository LayoutRepository { get; }
    protected AbpUINavigationVueVbenAdminOptions Options { get; }

    public VueVbenAdminNavigationSeedContributor(
        ICurrentTenant currentTenant,
        IRouteDataSeeder routeDataSeeder,
        IMenuRepository menuRepository,
        ILayoutRepository layoutRepository,
        IGuidGenerator guidGenerator,
        IDataDictionaryDataSeeder dataDictionaryDataSeeder,
        IOptions<AbpUINavigationVueVbenAdminOptions> options)
    {
        CurrentTenant = currentTenant;
        GuidGenerator = guidGenerator;
        RouteDataSeeder = routeDataSeeder;
        MenuRepository = menuRepository;
        LayoutRepository = layoutRepository;
        DataDictionaryDataSeeder = dataDictionaryDataSeeder;

        Options = options.Value;
    }

    public override async Task SeedAsync(NavigationSeedContext context)
    {
        var uiDataItem = await SeedUIFrameworkDataAsync(CurrentTenant.Id);

        var layoutData = await SeedLayoutDataAsync(CurrentTenant.Id);

        var layout = await SeedDefaultLayoutAsync(layoutData, uiDataItem);

        var latMenu = await MenuRepository.GetLastMenuAsync();

        if (int.TryParse(CodeNumberGenerator.GetLastCode(latMenu?.Code ?? "0"), out int _lastNumber))
        {
            Interlocked.Exchange(ref _lastCodeNumber, _lastNumber);
        }

        await SeedDefinitionMenusAsync(layout, layoutData, context.Menus, context.MultiTenancySides);
    }

    private async Task SeedDefinitionMenusAsync(
        Layout layout,
        Data data, 
        IReadOnlyCollection<ApplicationMenu> menus,
        MultiTenancySides multiTenancySides)
    {
        foreach (var menu in menus)
        {
            if (!menu.MultiTenancySides.HasFlag(multiTenancySides))
            {
                continue;
            }

            var menuMeta = new Dictionary<string, object>()
            {
                { "title", menu.DisplayName },
                { "icon", menu.Icon ?? "" },
                { "orderNo", menu.Order },
                { "hideTab", false },
                { "ignoreAuth", false },
            };
            foreach (var prop in menu.ExtraProperties)
            {
                if (menuMeta.ContainsKey(prop.Key))
                {
                    menuMeta[prop.Key] = prop.Value;
                }
                else
                {
                    menuMeta.Add(prop.Key, prop.Value);
                }
            }

            var seedMenu = await SeedMenuAsync(
                layout:         layout,
                data:           data,
                name:           menu.Name,
                path:           menu.Url,
                code:           CodeNumberGenerator.CreateCode(GetNextCode()),
                component:      layout.Path,
                displayName:    menu.DisplayName,
                redirect:       menu.Redirect,
                description:    menu.Description,
                parentId:       null,
                tenantId:       layout.TenantId,
                meta:           menuMeta,
                roles:          new string[] { "admin" });

            await SeedDefinitionMenuItemsAsync(layout, data, seedMenu, menu.Items, multiTenancySides);
        }
    }

    private async Task SeedDefinitionMenuItemsAsync(
        Layout layout, 
        Data data, 
        Menu menu, 
        ApplicationMenuList items,
        MultiTenancySides multiTenancySides)
    {
        int index = 1;
        foreach (var item in items)
        {
            if (!item.MultiTenancySides.HasFlag(multiTenancySides))
            {
                continue;
            }

            var menuMeta = new Dictionary<string, object>()
            {
                { "title", item.DisplayName },
                { "icon", item.Icon ?? "" },
                { "orderNo", item.Order },
                { "hideTab", false },
                { "ignoreAuth", false },
            };
            foreach (var prop in item.ExtraProperties)
            {
                if (menuMeta.ContainsKey(prop.Key))
                {
                    menuMeta[prop.Key] = prop.Value;
                }
                else
                {
                    menuMeta.Add(prop.Key, prop.Value);
                }
            }

            var seedMenu = await SeedMenuAsync(
                layout: layout,
                data: data,
                name: item.Name,
                path: item.Url,
                code: CodeNumberGenerator.AppendCode(menu.Code, CodeNumberGenerator.CreateCode(index)),
                component: item.Component.IsNullOrWhiteSpace() ? layout.Path : item.Component,
                displayName: item.DisplayName,
                redirect: item.Redirect,
                description: item.Description,
                parentId: menu.Id,
                tenantId: menu.TenantId,
                meta: menuMeta,
                roles: new string[] { "admin" });

            await SeedDefinitionMenuItemsAsync(layout, data, seedMenu, item.Items, multiTenancySides);

            index++;
        }
    }

    private async Task<Menu> SeedMenuAsync(
        Layout layout,
        Data data,
        string name,
        string path,
        string code,
        string component,
        string displayName,
        string redirect = "",
        string description = "",
        Guid? parentId = null,
        Guid? tenantId = null,
        Dictionary<string, object> meta = null,
        string[] roles = null,
        Guid[] users = null,
        bool isPublic = false
        )
    {
        var menu = await RouteDataSeeder.SeedMenuAsync(
            layout,
            name,
            path,
            code,
            component,
            displayName,
            redirect,
            description,
            parentId,
            tenantId,
            isPublic
            );
        foreach (var item in data.Items)
        {
            menu.SetProperty(item.Name, item.DefaultValue);
        }
        if (meta != null)
        {
            foreach (var item in meta)
            {
                menu.SetProperty(item.Key, item.Value);
            }
        }

        if (roles != null)
        {
            foreach (var role in roles)
            {
                await RouteDataSeeder.SeedRoleMenuAsync(role, menu, tenantId);
            }
        }

        if (users != null)
        {
            foreach (var user in users)
            {
                await RouteDataSeeder.SeedUserMenuAsync(user, menu, tenantId);
            }
        }

        return menu;
    }

    private async Task<DataItem> SeedUIFrameworkDataAsync(Guid? tenantId)
    {
        var data = await DataDictionaryDataSeeder
            .SeedAsync(
                "UI Framework",
                CodeNumberGenerator.CreateCode(10),
                "Khung giao diện",
                "UI Framework",
                null,
                tenantId,
                true);

        data.AddItem(
            GuidGenerator,
            Options.UI,
            Options.UI,
            Options.UI,
            ValueType.String,
            Options.UI,
            isStatic: true);

        return data.FindItem(Options.UI);
    }

    private async Task<Layout> SeedDefaultLayoutAsync(Data data, DataItem uiDataItem)
    {
        var layout = await RouteDataSeeder.SeedLayoutAsync(
           Options.LayoutName,
           Options.LayoutPath,
           Options.LayoutName,
           data.Id,
           uiDataItem.Name,
           "",
           Options.LayoutName,
           data.TenantId
           );

        return layout;
    }

    private async Task<Data> SeedLayoutDataAsync(Guid? tenantId)
    {
        var data = await DataDictionaryDataSeeder
            .SeedAsync(
                Options.LayoutName,
                CodeNumberGenerator.CreateCode(10),
                "Ràng buộc bố cục Vben Admin",
                "Từ điển meta bố cục Vben Admin",
                null,
                tenantId,
                true);

        data.AddItem(
            GuidGenerator,
            "hideMenu",
            "Không hiển thị trong menu",
            "false",
            ValueType.Boolean,
            "Đường dẫn hiện tại không hiển thị trong menu",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "icon",
            "Biểu tượng",
            "",
            ValueType.String,
            "Biểu tượng, cũng là biểu tượng menu",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "currentActiveMenu",
            "Menu hiện tại được kích hoạt",
            "",
            ValueType.String,
            "Dùng để cấu hình đường dẫn menu được kích hoạt bên trái khi ở trang chi tiết",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "ignoreKeepAlive",
            "Bộ nhớ đệm KeepAlive",
            "false",
            ValueType.Boolean,
            "Có bỏ qua bộ nhớ đệm KeepAlive hay không",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "frameSrc",
            "Địa chỉ IFrame",
            "",
            ValueType.String,
            "Địa chỉ của iframe nhúng",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "transitionName",
            "Hiệu ứng chuyển đổi đường dẫn",
            "",
            ValueType.String,
            "Chỉ định tên hiệu ứng chuyển đổi cho đường dẫn này",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "roles",
            "Vai trò có thể truy cập",
            "",
            ValueType.Array,
            "Vai trò có thể truy cập, chỉ có hiệu lực khi chế độ phân quyền là Role",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "title",
            "Tiêu đề đường dẫn",
            "",
            ValueType.String,
            "Tiêu đề đường dẫn thường là bắt buộc",
            false,
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "carryParam",
            "Hiển thị trên trang tab",
            "false",
            ValueType.Boolean,
            "Nếu đường dẫn này mang tham số và cần hiển thị trên trang tab, thì cần đặt là true",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "hideBreadcrumb",
            "Ẩn breadcrumb",
            "false",
            ValueType.Boolean,
            "Ẩn hiển thị đường dẫn này trên breadcrumb",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "ignoreAuth",
            "Bỏ qua quyền",
            "false",
            ValueType.Boolean,
            "Có bỏ qua quyền hay không, chỉ có hiệu lực khi chế độ phân quyền là Role",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "hideChildrenInMenu",
            "Ẩn tất cả menu con",
            "false",
            ValueType.Boolean,
            "Ẩn tất cả menu con",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "hideTab",
            "Không hiển thị trên trang tab",
            "false",
            ValueType.Boolean,
            "Đường dẫn hiện tại không hiển thị trên trang tab",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "affix",
            "Cố định trang tab",
            "false",
            ValueType.Boolean,
            "Có cố định trang tab hay không",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "requiredFeatures",
            "Các tính năng cần thiết",
            "",
            ValueType.String,
            "Nhiều tính năng được phân tách bằng dấu phẩy tiếng Anh",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "dynamicLevel",
            "Số trang tab có thể mở",
            "",
            ValueType.Numeic,
            "Số trang tab có thể mở cho đường dẫn động",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "hidePathForChildren",
            "Bỏ qua đường dẫn cấp này",
            "",
            ValueType.Boolean,
            "Có bỏ qua đường dẫn cấp này trong đường dẫn đầy đủ của menu con hay không. Hiệu lực từ phiên bản 2.5.3 trở lên",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "orderNo",
            "Sắp xếp menu",
            "",
            ValueType.Numeic,
            "Sắp xếp menu, chỉ có hiệu lực đối với cấp đầu tiên",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "realPath",
            "Đường dẫn thực tế",
            "",
            ValueType.String,
            "Đường dẫn thực tế của đường dẫn động, tức là loại bỏ phần động của đường dẫn",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "frameFormat",
            "Định dạng IFrame",
            "false",
            ValueType.Boolean,
            "Định dạng frame mở rộng, {token}: Truyền tiêu đề yêu cầu token trên trang iframe được mở");

        return data;
    }

    private int GetNextCode()
    {
        Interlocked.Increment(ref _lastCodeNumber);
        return _lastCodeNumber;
    }
}