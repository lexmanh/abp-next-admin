using System;
using System.Reflection;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace LINGYUN.Abp.UI.Navigation.VueVbenAdmin;

public class AbpUINavigationVueVbenAdminNavigationDefinitionProvider : NavigationDefinitionProvider
{
    public override void Define(INavigationDefinitionContext context)
    {
        context.Add(GetDashboard());
        context.Add(GetManage());
        context.Add(GetSaas());
        context.Add(GetPlatform());
        // TODO: 网关不再需要动态管理
        // context.Add(GetApiGateway());
        context.Add(GetLocalization());
        context.Add(GetOssManagement());
        context.Add(GetTaskManagement());
        context.Add(GetWebhooksManagement());
        context.Add(GetMessages());
        context.Add(GetTextTemplating());
    }

    private static NavigationDefinition GetDashboard()
    {
        var dashboard = new ApplicationMenu(
            name: "Vben Dashboard",
            displayName: "Bảng điều khiển",
            url: "/dashboard",
            component: "",
            description: "仪表盘",
            icon: "ion:grid-outline",
            redirect: "/dashboard/workbench");

        dashboard.AddItem(
            new ApplicationMenu(
                name: "Analysis",
                displayName: "Trang phân tích",
                url: "/dashboard/analysis",
                component: "/dashboard/analysis/index",
                description: "分析页"));
        dashboard.AddItem(
           new ApplicationMenu(
               name: "Workbench",
               displayName: "Bàn làm việc",
               url: "/dashboard/workbench",
               component: "/dashboard/workbench/index",
               description: "工作台"));


        return new NavigationDefinition(dashboard);
    }

    private static NavigationDefinition GetManage()
    {
        var manage = new ApplicationMenu(
            name: "Manage",
            displayName: "Quản lý",
            url: "/manage",
            component: "",
            description: "管理",
            icon: "ant-design:control-outlined");

        var identity = manage.AddItem(
            new ApplicationMenu(
                name: "Identity",
                displayName: "Quản lý xác thực danh tính",
                url: "/manage/identity",
                component: "",
                description: "身份认证管理"));
        identity.AddItem(
          new ApplicationMenu(
              name: "User",
              displayName: "Người dùng",
              url: "/manage/identity/user",
              component: "/identity/user/index",
              description: "用户"));
        identity.AddItem(
          new ApplicationMenu(
              name: "Role",
              displayName: "Nhân vật",
              url: "/manage/identity/role",
              component: "/identity/role/index",
              description: "角色"));
        identity.AddItem(
          new ApplicationMenu(
              name: "Claim",
              displayName: "Nhận dạng danh tính",
              url: "/manage/identity/claim-types",
              component: "/identity/claim-types/index",
              description: "身份标识",
              multiTenancySides: MultiTenancySides.Host));
        identity.AddItem(
          new ApplicationMenu(
              name: "OrganizationUnits",
              displayName: "Tổ chức bộ máy",
              url: "/manage/identity/organization-units",
              component: "/identity/organization-units/index",
              description: "组织机构"));
        identity.AddItem(
          new ApplicationMenu(
              name: "SecurityLogs",
              displayName: "Nhật ký an toàn",
              url: "/manage/identity/security-logs",
              component: "/identity/security-logs/index",
              description: "安全日志")
            // 此路由需要依赖安全日志特性
            .SetProperty("requiredFeatures", "AbpAuditing.Logging.SecurityLog"));

        manage.AddItem(new ApplicationMenu(
               name: "AuditLogs",
               displayName: "Nhật ký kiểm toán",
               url: "/manage/audit-logs",
               component: "/auditing/index",
               description: "审计日志")
            // 此路由需要依赖审计日志特性
            .SetProperty("requiredFeatures", "AbpAuditing.Logging.AuditLog"));

        var settingManagement = manage.AddItem(new ApplicationMenu(
               name: "SettingManagement",
               displayName: "Quản lý cài đặt",
               url: "/manage/settings",
               component: "LAYOUT",
               description: "设置管理",
               icon: "ant-design:setting-outlined",
               multiTenancySides: MultiTenancySides.Host)
            // 此路由需要依赖设置管理特性
            .SetProperty("requiredFeatures", "SettingManagement.Enable"));
        settingManagement.AddItem(new ApplicationMenu(
               name: "SystemSettings",
               displayName: "Cài đặt hệ thống",
               url: "/manage/settings/system-setting",
               component: "/settings-management/settings/index",
               description: "系统设置",
               multiTenancySides: MultiTenancySides.Host));
        settingManagement.AddItem(new ApplicationMenu(
               name: "SettingDefinitions",
               displayName: "Thiết lập định nghĩa",
               url: "/manage/settings/definitions",
               component: "/settings-management/definitions/index",
               description: "设置定义",
               multiTenancySides: MultiTenancySides.Host));

        var featureManagement = manage.AddItem(new ApplicationMenu(
               name: "FeaturesManagement",
               displayName: "Quản lý chức năng",
               url: "/manage/feature-management",
               component: "LAYOUT",
               description: "功能管理",
               icon: "ant-design:gold-outlined",
               multiTenancySides: MultiTenancySides.Host));
        featureManagement.AddItem(new ApplicationMenu(
               name: "FeaturesGroupDefinitions",
               displayName: "Phân nhóm chức năng",
               url: "/manage/feature-management/definitions/groups",
               component: "/feature-management/definitions/groups/index",
               description: "功能分组",
               multiTenancySides: MultiTenancySides.Host));
        featureManagement.AddItem(new ApplicationMenu(
               name: "FeaturesDefinitions",
               displayName: "Định nghĩa chức năng",
               url: "/manage/feature-management/definitions/features",
               component: "/feature-management/definitions/features/index",
               description: "功能定义",
               multiTenancySides: MultiTenancySides.Host));

        var permissionManagement = manage.AddItem(new ApplicationMenu(
              name: "PermissionsManagement",
              displayName: "Quản lý quyền hạn",
              url: "/manage/permission-management",
              component: "LAYOUT",
              description: "权限管理",
              icon: "arcticons:permissionsmanager",
              multiTenancySides: MultiTenancySides.Host));
        permissionManagement.AddItem(new ApplicationMenu(
               name: "PermissionsGroupDefinitions",
               displayName: "Nhóm quyền hạn",
               url: "/manage/permission-management/definitions/groups",
               component: "/permission-management/definitions/groups/index",
               description: "权限分组",
               multiTenancySides: MultiTenancySides.Host));
        permissionManagement.AddItem(new ApplicationMenu(
               name: "PermissionsDefinitions",
               displayName: "Định nghĩa quyền hạn",
               url: "/manage/permission-management/definitions/permissions",
               component: "/permission-management/definitions/permissions/index",
               description: "权限定义",
               multiTenancySides: MultiTenancySides.Host));

        var notificationManagement = manage.AddItem(new ApplicationMenu(
              name: "RealtimeNotifications",
              displayName: "Quản lý thông báo",
              url: "/realtime/notifications",
              component: "LAYOUT",
              description: "通知管理",
              icon: "ant-design:notification-outlined",
              multiTenancySides: MultiTenancySides.Host));
        notificationManagement.AddItem(new ApplicationMenu(
               name: "NotificationsGroupDefinitions",
               displayName: "Thông báo nhóm",
               url: "/realtime/notifications/definitions/groups",
               component: "/realtime/notifications/definitions/groups/index",
               description: "通知分组",
               multiTenancySides: MultiTenancySides.Host));
        notificationManagement.AddItem(new ApplicationMenu(
               name: "NotificationsDefinitions",
               displayName: "Định nghĩa thông báo",
               url: "/realtime/notifications/definitions/notifications",
               component: "/realtime/notifications/definitions/notifications/index",
               description: "通知定义",
               multiTenancySides: MultiTenancySides.Host));

        var identityServer = manage.AddItem(
                new ApplicationMenu(
                    name: "IdentityServer",
                    displayName: "Máy chủ xác thực danh tính",
                    url: "/manage/identity-server",
                    component: "",
                    description: "身份认证服务器",
                    multiTenancySides: MultiTenancySides.Host));
        identityServer.AddItem(
            new ApplicationMenu(
                name: "Clients",
                displayName: "Khách hàng",
                url: "/manage/identity-server/clients",
                component: "/identity-server/clients/index",
                description: "客户端",
                multiTenancySides: MultiTenancySides.Host));
        identityServer.AddItem(
            new ApplicationMenu(
                name: "ApiResources",
                displayName: "Tài nguyên API",
                url: "/manage/identity-server/api-resources",
                component: "/identity-server/api-resources/index",
                description: "Api 资源",
                multiTenancySides: MultiTenancySides.Host));
        identityServer.AddItem(
            new ApplicationMenu(
                name: "IdentityResources",
                displayName: "Tài nguyên danh tính",
                url: "/manage/identity-server/identity-resources",
                component: "/identity-server/identity-resources/index",
                description: "身份资源",
                multiTenancySides: MultiTenancySides.Host));
        identityServer.AddItem(
            new ApplicationMenu(
                name: "ApiScopes",
                displayName: "Phạm vi API",
                url: "/manage/identity-server/api-scopes",
                component: "/identity-server/api-scopes/index",
                description: "Api 范围",
                multiTenancySides: MultiTenancySides.Host));
        identityServer.AddItem(
            new ApplicationMenu(
                name: "PersistedGrants",
                displayName: "Ủy quyền lâu dài",
                url: "/manage/identity-server/persisted-grants",
                component: "/identity-server/persisted-grants/index",
                description: "持久授权",
                multiTenancySides: MultiTenancySides.Host));

        var openIddict = manage.AddItem(
                new ApplicationMenu(
                    name: "OpenIddict",
                    displayName: "Máy chủ xác thực danh tính",
                    url: "/manage/openiddict",
                    component: "LAYOUT",
                    description: "身份认证服务器(OpenIddict)",
                    multiTenancySides: MultiTenancySides.Host));
        openIddict.AddItem(
            new ApplicationMenu(
                name: "OpenIddictApplications",
                displayName: "Quản lý ứng dụng",
                url: "/manage/openiddict/applications",
                component: "/openiddict/applications/index",
                description: "应用管理",
                multiTenancySides: MultiTenancySides.Host));
        openIddict.AddItem(
            new ApplicationMenu(
                name: "OpenIddictAuthorizations",
                displayName: "Quản lý ủy quyền",
                url: "/manage/openiddict/authorizations",
                component: "/openiddict/authorizations/index",
                description: "授权管理",
                multiTenancySides: MultiTenancySides.Host));
        openIddict.AddItem(
            new ApplicationMenu(
                name: "OpenIddictScopes",
                displayName: "Phạm vi Api",
                url: "/manage/openiddict/scopes",
                component: "/openiddict/scopes/index",
                description: "Api 范围",
                multiTenancySides: MultiTenancySides.Host));
        openIddict.AddItem(
            new ApplicationMenu(
                name: "OpenIddictTokens",
                displayName: "Mã thông báo ủy quyền",
                url: "/manage/openiddict/tokens",
                component: "/openiddict/tokens/index",
                description: "授权令牌",
                multiTenancySides: MultiTenancySides.Host));

        manage.AddItem(
            new ApplicationMenu(
                name: "Logs",
                displayName: "Nhật ký hệ thống",
                url: "/sys/logs",
                component: "/sys/logging/index",
                description: "系统日志",
                multiTenancySides: MultiTenancySides.Host));

        manage.AddItem(
            new ApplicationMenu(
                name: "ApiDocument",
                displayName: "Tài liệu API",
                url: "/openapi",
                component: "IFrame",
                description: "Api 文档",
                multiTenancySides: MultiTenancySides.Host)
            // TODO: 注意在部署完毕之后手动修改此菜单iframe地址
            .SetProperty("frameSrc", "http://127.0.0.1:30000/swagger/index.html"));

        manage.AddItem(
            new ApplicationMenu(
                name: "Caches",
                displayName: "Quản lý bộ nhớ đệm",
                url: "/manage/cache",
                component: "/caching-management/cache/index",
                description: "缓存管理"));

        return new NavigationDefinition(manage);
    }

    private static NavigationDefinition GetSaas()
    {
        var saas = new ApplicationMenu(
            name: "Saas",
            displayName: "Saas",
            url: "/saas",
            component: "",
            description: "Saas",
            icon: "ant-design:cloud-server-outlined",
            multiTenancySides: MultiTenancySides.Host);
        saas.AddItem(
          new ApplicationMenu(
              name: "Tenants",
              displayName: "Quản lý người thuê nhà",
              url: "/saas/tenants",
              component: "/saas/tenant/index",
              description: "租户管理",
              multiTenancySides: MultiTenancySides.Host));
        saas.AddItem(
          new ApplicationMenu(
              name: "Editions",
              displayName: "Quản lý phiên bản",
              url: "/saas/editions",
              component: "/saas/editions/index",
              description: "版本管理",
              multiTenancySides: MultiTenancySides.Host));

        return new NavigationDefinition(saas);
    }

    private static NavigationDefinition GetPlatform()
    {
        var platform = new ApplicationMenu(
            name: "Platform",
            displayName: "Quản lý nền tảng",
            url: "/platform",
            component: "",
            description: "平台管理",
            icon: "ep:platform");
        platform.AddItem(
          new ApplicationMenu(
              name: "DataDictionary",
              displayName: "Từ điển dữ liệu",
              url: "/platform/data-dic",
              component: "/platform/dataDic/index",
              description: "数据字典"));
        platform.AddItem(
          new ApplicationMenu(
              name: "Layout",
              displayName: "Bố cục",
              url: "/platform/layout",
              component: "/platform/layout/index",
              description: "布局"));
        platform.AddItem(
          new ApplicationMenu(
              name: "Menu",
              displayName: "Thực đơn",
              url: "/platform/menu",
              component: "/platform/menu/index",
              description: "菜单"));

        return new NavigationDefinition(platform);
    }

    private static NavigationDefinition GetApiGateway()
    {
        var apiGateway = new ApplicationMenu(
            name: "ApiGateway",
            displayName: "Quản lý cổng kết nối",
            url: "/api-gateway",
            component: "",
            description: "网关管理",
            icon: "ant-design:gateway-outlined",
            multiTenancySides: MultiTenancySides.Host);
        apiGateway.AddItem(
          new ApplicationMenu(
              name: "RouteGroup",
              displayName: "Phân nhóm định tuyến",
              url: "/api-gateway/group",
              component: "/api-gateway/group/index",
              description: "路由分组",
              multiTenancySides: MultiTenancySides.Host));
        apiGateway.AddItem(
          new ApplicationMenu(
              name: "GlobalConfiguration",
              displayName: "Cấu hình công cộng",
              url: "/api-gateway/global",
              component: "/api-gateway/global/index",
              description: "公共配置",
              multiTenancySides: MultiTenancySides.Host));
        apiGateway.AddItem(
          new ApplicationMenu(
              name: "Route",
              displayName: "Quản lý định tuyến",
              url: "/api-gateway/route",
              component: "/api-gateway/route/index",
              description: "路由管理",
              multiTenancySides: MultiTenancySides.Host));
        apiGateway.AddItem(
         new ApplicationMenu(
             name: "AggregateRoute",
             displayName: "Bộ định tuyến hợp nhất",
             url: "/api-gateway/aggregate",
             component: "/api-gateway/aggregate/index",
             description: "聚合路由",
             multiTenancySides: MultiTenancySides.Host));

        return new NavigationDefinition(apiGateway);
    }

    private static NavigationDefinition GetLocalization()
    {
        var localization = new ApplicationMenu(
            name: "Localization",
            displayName: "Quản lý địa phương hóa",
            url: "/localization",
            component: "",
            description: "本地化管理",
            icon: "ant-design:translation-outlined",
            multiTenancySides: MultiTenancySides.Host);
        localization.AddItem(
          new ApplicationMenu(
              name: "Languages",
              displayName: "Quản lý ngôn ngữ",
              url: "/localization/languages",
              component: "/localization/languages/index",
              description: "语言管理",
              multiTenancySides: MultiTenancySides.Host)
            );
        localization.AddItem(
          new ApplicationMenu(
              name: "Resources",
              displayName: "Quản lý tài nguyên",
              url: "/localization/resources",
              component: "/localization/resources/index",
              description: "资源管理",
              multiTenancySides: MultiTenancySides.Host)
            );
        localization.AddItem(
          new ApplicationMenu(
              name: "Texts",
              displayName: "Quản lý tài liệu",
              url: "/localization/texts",
              component: "/localization/texts/index",
              description: "文档管理",
              multiTenancySides: MultiTenancySides.Host)
            );

        return new NavigationDefinition(localization);
    }

    private static NavigationDefinition GetOssManagement()
    {
        var oss = new ApplicationMenu(
            name: "OssManagement",
            displayName: "Lưu trữ đối tượng",
            url: "/oss",
            component: "",
            description: "对象存储",
            icon: "ant-design:file-twotone");
        oss.AddItem(
          new ApplicationMenu(
              name: "Containers",
              displayName: "Quản lý container",
              url: "/oss/containers",
              component: "/oss-management/containers/index",
              description: "容器管理"));
        oss.AddItem(
          new ApplicationMenu(
              name: "Objects",
              displayName: "Quản lý tệp",
              url: "/oss/objects",
              component: "/oss-management/objects/index",
              description: "文件管理"));

        return new NavigationDefinition(oss);
    }

    private static NavigationDefinition GetTaskManagement()
    {
        var task = new ApplicationMenu(
            name: "TaskManagement",
            displayName: "Nền tảng lập lịch nhiệm vụ",
            url: "/task-management",
            component: "",
            description: "任务调度平台",
            icon: "bi:list-task");
        task.AddItem(
          new ApplicationMenu(
              name: "BackgroundJobs",
              displayName: "Quản lý nhiệm vụ",
              url: "/task-management/background-jobs",
              component: "/task-management/background-jobs/index",
              description: "任务管理"));
        task.AddItem(
          new ApplicationMenu(
              name: "BackgroundJobInfoDetail",
              displayName: "Chi tiết nhiệm vụ",
              url: "/task-management/background-jobs/:id",
              component: "/task-management/background-jobs/components/BackgroundJobInfoDetail",
              description: "任务详情")
          .SetProperty("hideMenu", "true")
          .SetProperty("hideTab", "true"));

        return new NavigationDefinition(task);
    }

    private static NavigationDefinition GetWebhooksManagement()
    {
        var webhooks = new ApplicationMenu(
            name: "WebHooks",
            displayName: "WebHooks",
            url: "/webhooks",
            component: "",
            description: "WebHooks",
            icon: "ic:outline-webhook",
            multiTenancySides: MultiTenancySides.Host);
        webhooks.AddItem(
          new ApplicationMenu(
              name: "Subscriptions",
              displayName: "Quản lý đăng ký",
              url: "/webhooks/subscriptions",
              component: "/webhooks/subscriptions/index",
              description: "管理订阅",
              multiTenancySides: MultiTenancySides.Host));
        webhooks.AddItem(
          new ApplicationMenu(
              name: "SendAttempts",
              displayName: "Quản lý hồ sơ",
              url: "/webhooks/send-attempts",
              component: "/webhooks/send-attempts/index",
              description: "管理记录",
              multiTenancySides: MultiTenancySides.Host));
        webhooks.AddItem(
          new ApplicationMenu(
              name: "WebhooksGroupDefinitions",
              displayName: "Nhóm Webhook",
              url: "/webhooks/definitions/groups",
              component: "/webhooks/definitions/groups/index",
              description: "Webhook 分组",
              multiTenancySides: MultiTenancySides.Host));
        webhooks.AddItem(
          new ApplicationMenu(
              name: "WebhooksDefinitions",
              displayName: "Định nghĩa Webhook",
              url: "/webhooks/definitions/webhooks",
              component: "/webhooks/definitions/webhooks/index",
              description: "Webhook 定义",
              multiTenancySides: MultiTenancySides.Host));

        return new NavigationDefinition(webhooks);
    }

    private static NavigationDefinition GetMessages()
    {
        var messages = new ApplicationMenu(
            name: "Messages",
            displayName: "Quản lý tin nhắn",
            url: "/messages",
            component: "",
            description: "消息管理",
            icon: "ant-design:message-outlined");
        messages.AddItem(
          new ApplicationMenu(
              name: "Notifications",
              displayName: "Quản lý thông báo",
              url: "/messages/notifications",
              component: "/messages/notifications/index",
              description: "通知管理"));

        return new NavigationDefinition(messages);
    }

    private static NavigationDefinition GetTextTemplating()
    {
        var textTemplating = new ApplicationMenu(
            name: "Templates",
            displayName: "Quản lý mẫu",
            url: "/text-templating",
            component: "",
            description: "模板管理",
            icon: "eos-icons:templates-outlined",
            multiTenancySides: MultiTenancySides.Host);
        textTemplating.AddItem(
          new ApplicationMenu(
              name: "TextTemplates",
              displayName: "Mẫu văn bản",
              url: "/text-templating/text-templates",
              component: "/text-templating/templates/index",
              description: "文本模板",
              multiTenancySides: MultiTenancySides.Host));

        return new NavigationDefinition(textTemplating);
    }
}
