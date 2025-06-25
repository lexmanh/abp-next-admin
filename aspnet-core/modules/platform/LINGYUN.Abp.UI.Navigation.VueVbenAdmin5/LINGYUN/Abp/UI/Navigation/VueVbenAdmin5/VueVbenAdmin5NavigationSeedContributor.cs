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

namespace LINGYUN.Abp.UI.Navigation.VueVbenAdmin5;

public class VueVbenAdmin5NavigationSeedContributor : NavigationSeedContributor
{
    private static int _lastCodeNumber = 0;
    protected ICurrentTenant CurrentTenant { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected IRouteDataSeeder RouteDataSeeder { get; }
    protected IDataDictionaryDataSeeder DataDictionaryDataSeeder { get; }
    protected IMenuRepository MenuRepository { get; }
    protected ILayoutRepository LayoutRepository { get; }
    protected AbpUINavigationVueVbenAdmin5Options Options { get; }

    public VueVbenAdmin5NavigationSeedContributor(
        ICurrentTenant currentTenant,
        IRouteDataSeeder routeDataSeeder,
        IMenuRepository menuRepository,
        ILayoutRepository layoutRepository,
        IGuidGenerator guidGenerator,
        IDataDictionaryDataSeeder dataDictionaryDataSeeder,
        IOptions<AbpUINavigationVueVbenAdmin5Options> options)
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
                { "icon", menu.Icon ?? "" },
                { "order", menu.Order },
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
                { "icon", item.Icon ?? "" },
                { "order", item.Order },
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
                CodeNumberGenerator.CreateCode(30),
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
                CodeNumberGenerator.CreateCode(40),
                "Ràng buộc bố cục Vben5 Admin",
                "Ràng buộc bố cục mẫu Vben5 Admin",
                null,
                tenantId,
                true);

        data.AddItem(
            GuidGenerator,
            "title",
            "Tiêu đề",
            "",
            ValueType.String,
            "Dùng để cấu hình tiêu đề trang, sẽ hiển thị trong menu và trang tab. Thường được sử dụng cùng với quốc tế hóa.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "icon",
            "Biểu tượng",
            "",
            ValueType.String,
            "Dùng để cấu hình biểu tượng trang, sẽ hiển thị trong menu và trang tab. Thường được sử dụng cùng với thư viện biểu tượng, nếu là liên kết http, sẽ tự động tải hình ảnh.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "activeIcon",
            "Biểu tượng kích hoạt",
            "",
            ValueType.String,
            "Dùng để cấu hình biểu tượng kích hoạt của trang, sẽ hiển thị trong menu. Thường được sử dụng cùng với thư viện biểu tượng, nếu là liên kết http, sẽ tự động tải hình ảnh.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "keepAlive",
            "Có bật bộ nhớ đệm không",
            "true",
            ValueType.Boolean,
            "Dùng để cấu hình xem trang có bật bộ nhớ đệm hay không, khi bật thì trang sẽ được lưu vào bộ nhớ đệm, không tải lại, chỉ có hiệu lực khi trang tab được bật.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "hideInMenu",
            "Có ẩn trong menu không",
            "false",
            ValueType.Boolean,
            "Dùng để cấu hình xem trang có được ẩn trong menu hay không, khi ẩn thì trang sẽ không hiển thị trong menu.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "hideInTab",
            "Có ẩn trong trang tab không",
            "false",
            ValueType.Boolean,
            "Dùng để cấu hình xem trang có được ẩn trong trang tab hay không, khi ẩn thì trang sẽ không hiển thị trong trang tab.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "hideInBreadcrumb",
            "Có ẩn trong breadcrumb không",
            "false",
            ValueType.Boolean,
            "Dùng để cấu hình xem trang có được ẩn trong breadcrumb hay không, khi ẩn thì trang sẽ không hiển thị trong breadcrumb.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "hideChildrenInMenu",
            "Có ẩn menu con không",
            "false",
            ValueType.Boolean,
            "Dùng để cấu hình xem các trang con của trang có được ẩn trong menu hay không, khi ẩn thì các trang con sẽ không hiển thị trong menu.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "authority",
            "Quyền trang",
            "",
            ValueType.Array,
            "Dùng để cấu hình quyền của trang, chỉ người dùng có quyền tương ứng mới có thể truy cập trang, nếu không cấu hình thì không cần quyền.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "badge",
            "Huy hiệu trang",
            "",
            ValueType.String,
            "Dùng để cấu hình huy hiệu của trang, sẽ hiển thị trong menu.",
            isStatic: true);
        data.AddItem(
           GuidGenerator,
           "badgeType",
           "Loại huy hiệu",
           "normal",
           ValueType.String,
           "Dùng để cấu hình loại huy hiệu của trang, dot là chấm đỏ, normal là văn bản.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "badgeVariants",
            "Màu huy hiệu",
            "success",
            ValueType.String,
            "Dùng để cấu hình màu huy hiệu của trang, 'default' | 'destructive' | 'primary' | 'success' | 'warning' | string",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "activePath",
            "Menu hiện tại được kích hoạt",
            "",
            ValueType.String,
            "Dùng để cấu hình menu hiện tại được kích hoạt, đôi khi trang không hiển thị trong menu, cần kích hoạt menu cấp trên thì sử dụng.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "affixTab",
            "Có cố định trang tab không",
            "false",
            ValueType.Boolean,
            "Dùng để cấu hình xem trang có cố định trang tab hay không, khi cố định thì trang không thể đóng.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "affixTabOrder",
            "Sắp xếp trang tab cố định",
            "0",
            ValueType.Numeic,
            "Dùng để cấu hình sắp xếp trang tab cố định, sử dụng sắp xếp tăng dần.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "iframeSrc",
            "Địa chỉ trang nhúng",
            "",
            ValueType.String,
            "Dùng để cấu hình địa chỉ iframe của trang nhúng, khi thiết lập thì trang tương ứng sẽ được nhúng trong trang hiện tại.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "ignoreAccess",
            "Có bỏ qua quyền không",
            "false",
            ValueType.Boolean,
            "Dùng để cấu hình xem trang có bỏ qua quyền hay không, có thể truy cập trực tiếp.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "link",
            "Đường dẫn chuyển hướng ngoài",
            "",
            ValueType.String,
            "Dùng để cấu hình đường dẫn chuyển hướng ngoài, sẽ mở trong cửa sổ mới.",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "maxNumOfOpenTab",
            "Số lượng tối đa trang tab mở",
            "-1",
            ValueType.Numeic,
            "Dùng để cấu hình số lượng tối đa trang tab có thể mở, khi thiết lập thì sẽ tự động đóng trang tab mở sớm nhất khi mở trang tab mới (chỉ có hiệu lực khi mở trang tab cùng tên).",
            isStatic: true);
        data.AddItem(
            GuidGenerator,
            "menuVisibleWithForbidden",
            "Có hiển thị menu khi không có quyền không",
            "false",
            ValueType.Boolean,
            "Dùng để cấu hình xem trang có thể hiển thị trong menu nhưng truy cập sẽ bị chuyển hướng đến 403.");
        data.AddItem(
            GuidGenerator,
            "openInNewWindow",
            "Có mở trong cửa sổ mới không",
            "false",
            ValueType.Boolean,
            "Khi đặt là true, trang sẽ được mở trong cửa sổ mới.");
        data.AddItem(
            GuidGenerator,
            "order",
            "Sắp xếp trang",
            "0",
            ValueType.Numeic,
            "Dùng để cấu hình sắp xếp trang, dùng cho sắp xếp từ đường dẫn đến menu. Lưu ý: sắp xếp chỉ có hiệu lực với menu cấp một, sắp xếp menu cấp hai cần thiết lập theo thứ tự mã trong menu cấp một tương ứng.");
        data.AddItem(
            GuidGenerator,
            "noBasicLayout",
            "Có không sử dụng bố cục cơ bản không",
            "false",
            ValueType.Boolean,
            "Dùng để cấu hình xem đường dẫn hiện tại có sử dụng bố cục cơ bản hay không, chỉ có hiệu lực khi ở cấp cao nhất. Mặc định, tất cả đường dẫn sẽ được bao bọc trong bố cục cơ bản (bao gồm thanh trên cùng và điều hướng bên cạnh), nếu trang của bạn không cần các thành phần này, có thể đặt noBasicLayout là true.");

        return data;
    }

    private int GetNextCode()
    {
        Interlocked.Increment(ref _lastCodeNumber);
        return _lastCodeNumber;
    }
}