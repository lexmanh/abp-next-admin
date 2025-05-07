using System.Security.Principal;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;

namespace LINGYUN.Abp.UI.Navigation.VueVbenAdmin5;

public class AbpUINavigationVueVbenAdmin5NavigationDefinitionProvider : NavigationDefinitionProvider
{
    public override void Define(INavigationDefinitionContext context)
    {
        context.Add(GetDashboard());
        context.Add(GetAccount());
        context.Add(GetManage());
        context.Add(GetSaas());
        context.Add(GetPlatform());
        context.Add(GetOssManagement());
        context.Add(GetTaskManagement());
        context.Add(GetWebhooksManagement());
        context.Add(GetTextTemplating());
        context.Add(GetVbenDemos());
    }

    private static NavigationDefinition[] GetVbenDemos()
    {
        var project = new ApplicationMenu(
            name: "VbenProject",
            displayName: "Dự án",
            url: "/vben-admin",
            component: "",
            description: "项目",
            order: 9998,
            icon: "https://unpkg.com/@vbenjs/static-source@0.1.7/source/logo-v1.webp")
            .SetProperty("badgeType", "dot")
            .SetProperty("title", "demos.vben.title");
        project.AddItem(
            new ApplicationMenu(
                name: "VbenDocument",
                displayName: "Tài liệu",
                url: "/vben-admin/document",
                component: "",
                icon: "lucide:book-open-text",
                description: "文档")
            .SetProperty("link", "https://doc.vben.pro")
            .SetProperty("title", "demos.vben.document")
         );
        project.AddItem(
            new ApplicationMenu(
                name: "VbenGithub",
                displayName: "Tài liệu",
                url: "/vben-admin/github",
                component: "",
                icon: "mdi:github",
                description: "文档")
            .SetProperty("link", "https://github.com/vbenjs/vue-vben-admin")
            .SetProperty("title", "Github")
         );
        project.AddItem(
            new ApplicationMenu(
                name: "VbenNaive",
                displayName: "Phiên bản Naive UI",
                url: "/vben-admin/naive",
                component: "",
                icon: "logos:naiveui",
                description: "Naive UI 版本")
            .SetProperty("badgeType", "dot")
            .SetProperty("link", "https://naive.vben.pro")
            .SetProperty("title", "demos.vben.naive-ui")
         );
        project.AddItem(
            new ApplicationMenu(
                name: "VbenElementPlus",
                displayName: "Phiên bản Element Plus",
                url: "/vben-admin/ele",
                component: "",
                icon: "logos:element",
                description: "Element Plus 版本")
            .SetProperty("badgeType", "dot")
            .SetProperty("link", "https://ele.vben.pro")
            .SetProperty("title", "demos.vben.element-plus")
         );

        var about = new ApplicationMenu(
            name: "VbenAbout",
            displayName: "Về",
            url: "/vben-admin/about",
            component: "/_core/about/index",
            description: "关于",
            order: 9999,
            icon: "lucide:copyright")
            .SetProperty("title", "demos.vben.about");

        return new NavigationDefinition[2]
        {
            new NavigationDefinition(project),
            new NavigationDefinition(about),
        };
    }

    private static NavigationDefinition GetAccount()
    {
        var account = new ApplicationMenu(
            name: "Vben5Account",
            displayName: "Quản lý tài khoản",
            url: "/account",
            component: "",
            description: "账户管理",
            icon: "mdi:account-outline")
            .SetProperty("hideInMenu", "true")
            .SetProperty("title", "abp.account.title");

        account.AddItem(
            new ApplicationMenu(
                name: "Vben5AccountMySettings",
                displayName: "Cài đặt cá nhân",
                url: "/account/my-settings",
                component: "/account/my-settings/index",
                icon: "tdesign:user-setting",
                description: "个人设置")
            .SetProperty("title", "abp.account.settings.title")
         );

        return new NavigationDefinition(account);
    }

    private static NavigationDefinition GetDashboard()
    {
        var dashboard = new ApplicationMenu(
            name: "Vben5Dashboard",
            displayName: "Bảng điều khiển",
            url: "/dashboard",
            component: "",
            description: "仪表盘",
            icon: "lucide:layout-dashboard",
            order: -1)
            .SetProperty("title", "page.dashboard.title");

        dashboard.AddItem(
            new ApplicationMenu(
                name: "Vben5Analysis",
                displayName: "Trang phân tích",
                url: "/analytics",
                component: "/dashboard/analytics/index",
                icon: "lucide:area-chart",
                description: "分析页")
            .SetProperty("affixTab", "true")
            .SetProperty("title", "page.dashboard.analytics")
         );

        dashboard.AddItem(
           new ApplicationMenu(
               name: "Vben5Workbench",
               displayName: "Bàn làm việc",
               url: "/workspace",
               component: "/dashboard/workspace/index",
               icon: "carbon:workspace",
               description: "工作台")
           .SetProperty("title", "page.dashboard.workspace")
        );

        return new NavigationDefinition(dashboard);
    }

    private static NavigationDefinition GetManage()
    {
        var manage = new ApplicationMenu(
            name: "Vben5Manage",
            displayName: "Quản lý",
            url: "/manage",
            component: "",
            description: "管理",
            icon: "arcticons:activity-manager")
            .SetProperty("title", "abp.manage.title");

        var openIddict = manage.AddItem(
                new ApplicationMenu(
                    name: "Vben5OpenIddict",
                    displayName: "Máy chủ xác thực danh tính",
                    url: "/manage/openiddict",
                    component: "",
                    icon: "mdi:openid",
                    description: "身份认证服务器(OpenIddict)",
                    multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.openiddict.title"));
        openIddict.AddItem(
            new ApplicationMenu(
                name: "Vben5OpenIddictApplications",
                displayName: "Quản lý ứng dụng",
                url: "/manage/openiddict/applications",
                component: "/openiddict/applications/index",
                icon: "carbon:application",
                description: "应用管理",
                multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.openiddict.applications"));
        openIddict.AddItem(
            new ApplicationMenu(
                name: "Vben5OpenIddictAuthorizations",
                displayName: "Quản lý ủy quyền",
                url: "/manage/openiddict/authorizations",
                component: "/openiddict/authorizations/index",
                icon: "arcticons:ente-authenticator",
                description: "授权管理",
                multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.openiddict.authorizations"));
        openIddict.AddItem(
            new ApplicationMenu(
                name: "Vben5OpenIddictScopes",
                displayName: "Quản lý phạm vi",
                url: "/manage/openiddict/scopes",
                component: "/openiddict/scopes/index",
                icon: "et:scope",
                description: "范围管理",
                multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.openiddict.scopes"));
        openIddict.AddItem(
            new ApplicationMenu(
                name: "Vben5OpenIddictTokens",
                displayName: "Mã thông báo ủy quyền",
                url: "/manage/openiddict/tokens",
                component: "/openiddict/tokens/index",
                icon: "oui:token-key",
                description: "授权令牌",
                multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.openiddict.tokens"));

        var identity = manage.AddItem(
            new ApplicationMenu(
                name: "Vben5Identity",
                displayName: "Quản lý xác thực danh tính",
                url: "/manage/identity",
                component: "",
                icon: "teenyicons:id-outline",
                description: "身份认证管理")
            .SetProperty("title", "abp.manage.identity.title"));
        identity.AddItem(
          new ApplicationMenu(
              name: "Vben5IdentityUsers",
              displayName: "Quản lý người dùng",
              url: "/manage/identity/users",
              component: "/identity/users/index",
              icon: "mdi:user-outline",
              description: "用户管理")
          .SetProperty("title", "abp.manage.identity.user"));
        identity.AddItem(
          new ApplicationMenu(
              name: "Vben5IdentityRoles",
              displayName: "Quản lý vai trò",
              url: "/manage/identity/roles",
              component: "/identity/roles/index",
              icon: "carbon:user-role",
              description: "角色管理")
          .SetProperty("title", "abp.manage.identity.role"));
        identity.AddItem(
          new ApplicationMenu(
              name: "Vben5IdentityClaimTypes",
              displayName: "Nhận dạng danh tính",
              url: "/manage/identity/claim-types",
              component: "/identity/claim-types/index",
              icon: "la:id-card-solid",
              description: "身份标识",
              multiTenancySides: MultiTenancySides.Host)
          .SetProperty("title", "abp.manage.identity.claimTypes"));
        identity.AddItem(
          new ApplicationMenu(
              name: "Vben5IdentityOrganizationUnits",
              displayName: "Tổ chức cơ cấu",
              url: "/manage/identity/organization-units",
              component: "/identity/organization-units/index",
              icon: "clarity:organization-line",
              description: "组织机构")
          .SetProperty("title", "abp.manage.identity.organizationUnits"));
        identity.AddItem(
          new ApplicationMenu(
              name: "SecurityLogs",
              displayName: "Nhật ký an toàn",
              url: "/manage/identity/security-logs",
              component: "/identity/security-logs/index",
              icon: "carbon:security",
              description: "安全日志")
          .SetProperty("title", "abp.manage.identity.securityLogs")
          .SetProperty("requiredFeatures", "AbpAuditing.Logging.SecurityLog"));
        identity.AddItem(
          new ApplicationMenu(
              name: "Vben5IdentitySessions",
              displayName: "Quản lý hội thoại",
              url: "/manage/identity/sessions",
              component: "/identity/sessions/index",
              icon: "carbon:prompt-session",
              description: "会话管理")
          .SetProperty("title", "abp.manage.identity.sessions"));

        var permissionManagement = manage.AddItem(new ApplicationMenu(
              name: "Vben5Permissions",
              displayName: "Quản lý quyền hạn",
              url: "/manage/permissions",
              component: "",
              description: "权限管理",
              icon: "arcticons:permissionsmanager",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.permissions.title"));
        permissionManagement.AddItem(new ApplicationMenu(
               name: "Vben5PermissionsGroupDefinitions",
               displayName: "Nhóm quyền hạn",
               url: "/manage/permissions/groups",
               component: "/permissions/groups/index",
               icon: "lucide:group",
               description: "权限分组",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.permissions.groups"));
        permissionManagement.AddItem(new ApplicationMenu(
               name: "Vben5PermissionsDefinitions",
               displayName: "Định nghĩa quyền hạn",
               url: "/manage/permissions/definitions",
               component: "/permissions/definitions/index",
               icon: "icon-park-outline:permissions",
               description: "权限定义",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.permissions.definitions"));


        var featureManagement = manage.AddItem(new ApplicationMenu(
               name: "Vben5Features",
               displayName: "Quản lý chức năng",
               url: "/manage/features",
               component: "",
               description: "功能管理",
               icon: "ant-design:gold-outlined",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.features.title"));
        featureManagement.AddItem(new ApplicationMenu(
               name: "Vben5FeaturesGroupDefinitions",
               displayName: "Nhóm chức năng",
               url: "/manage/features/groups",
               component: "/features/groups/index",
               icon: "lucide:group",
               description: "功能分组",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.features.groups"));
        featureManagement.AddItem(new ApplicationMenu(
               name: "Vben5FeaturesDefinitions",
               displayName: "Định nghĩa chức năng",
               url: "/manage/features/definitions",
               component: "/features/definitions/index",
               icon: "pajamas:feature-flag",
               description: "功能定义",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.features.definitions"));

        var settingManagement = manage.AddItem(new ApplicationMenu(
               name: "Vben5Settings",
               displayName: "Cài đặt quản lý",
               url: "/manage/settings",
               component: "",
               description: "设置管理",
               icon: "ic:outline-settings")
            .SetProperty("title", "abp.manage.settings.title")
            // 此路由需要依赖设置管理特性
            .SetProperty("requiredFeatures", "SettingManagement.Enable"));
        settingManagement.AddItem(new ApplicationMenu(
               name: "Vben5SettingsSystem",
               displayName: "Cài đặt hệ thống",
               url: "/manage/settings/system",
               component: "/settings/system/index",
               icon: "tabler:settings-cog",
               description: "系统设置")
            .SetProperty("title", "abp.manage.settings.system")
            // 此路由需要依赖设置管理特性
            .SetProperty("requiredFeatures", "SettingManagement.Enable"));
        settingManagement.AddItem(new ApplicationMenu(
               name: "Vben5SettingsDefinitions",
               displayName: "Thiết lập định nghĩa",
               url: "/manage/settings/definitions",
               component: "/settings/definitions/index",
               icon: "codicon:settings",
               description: "设置定义",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.settings.definitions"));

        var localization = manage.AddItem(new ApplicationMenu(
            name: "Vben5Localizations",
            displayName: "Quản lý địa phương hóa",
            url: "/manage/localization",
            component: "",
            description: "本地化管理",
            icon: "ion:globe-outline",
            multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.localization.title"));
        localization.AddItem(
          new ApplicationMenu(
              name: "Vben5LocalizationsLanguages",
              displayName: "Quản lý ngôn ngữ",
              url: "/manage/localization/languages",
              component: "/localization/languages/index",
              icon: "cil:language",
              description: "语言管理",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.localization.languages")
            );
        localization.AddItem(
          new ApplicationMenu(
              name: "Vben5LocalizationsResources",
              displayName: "Quản lý tài nguyên",
              url: "/manage/localization/resources",
              component: "/localization/resources/index",
              icon: "grommet-icons:resources",
              description: "资源管理",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.localization.resources")
            );
        localization.AddItem(
          new ApplicationMenu(
              name: "Vben5LocalizationsTexts",
              displayName: "Quản lý tài liệu",
              url: "/manage/localization/texts",
              component: "/localization/texts/index",
              icon: "mi:text",
              description: "文档管理",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.localization.texts")
            );

        var dataProtection = manage.AddItem(new ApplicationMenu(
              name: "Vben5DataProtection",
              displayName: "Bảo vệ dữ liệu",
              url: "/manage/data-protection",
              component: "",
              description: "数据保护",
              icon: "icon-park-outline:protect",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.dataProtection.title"));
        dataProtection.AddItem(new ApplicationMenu(
               name: "Vben5DataProtectionEntityTypeInfos",
               displayName: "Quản lý thực thể",
               url: "/manage/data-protection/entity-type-infos",
               component: "/data-protection/entity-type-infos/index",
               icon: "iconamoon:type",
               description: "实体管理",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.dataProtection.entityTypeInfos"));

        manage.AddItem(new ApplicationMenu(
               name: "Vben5AuditingAuditLogs",
               displayName: "Nhật ký kiểm toán",
               url: "/manage/audit-logs",
               component: "/auditing/audit-logs/index",
               icon: "fluent-mdl2:compliance-audit",
               description: "审计日志")
            .SetProperty("title", "abp.manage.auditLogs")
            // 此路由需要依赖审计日志特性
            .SetProperty("requiredFeatures", "AbpAuditing.Logging.AuditLog"));

        manage.AddItem(
            new ApplicationMenu(
                name: "Vben5AuditingLoggings",
                displayName: "Nhật ký hệ thống",
                url: "/manage/sys-logs",
                component: "/auditing/loggings/index",
                icon: "icon-park-outline:log",
                description: "系统日志",
                multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.loggings"));

        var notificationManagement = manage.AddItem(new ApplicationMenu(
              name: "Vben5Notifications",
              displayName: "Quản lý thông báo",
              url: "/manage/notifications",
              component: "",
              description: "通知管理",
              icon: "tabler:notification")
            .SetProperty("title", "abp.manage.notifications.title"));
        notificationManagement.AddItem(new ApplicationMenu(
               name: "Vben5NotificationsMyNotifilers",
               displayName: "Thông báo của tôi",
               url: "/manage/notifications/my-notifilers",
               component: "/notifications/my-notifilers/index",
               icon: "ant-design:notification-outlined",
               description: "我的通知")
            .SetProperty("title", "abp.manage.notifications.myNotifilers"));
        notificationManagement.AddItem(new ApplicationMenu(
               name: "Vben5NotificationsGroupDefinitions",
               displayName: "Thông báo nhóm",
               url: "/manage/notifications/groups",
               component: "/notifications/groups/index",
               icon: "lucide:group",
               description: "通知分组",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.notifications.groups"));
        notificationManagement.AddItem(new ApplicationMenu(
               name: "NotificationsDefinitions",
               displayName: "Định nghĩa thông báo",
               url: "/manage/notifications/definitions",
               component: "/notifications/definitions/index",
               icon: "nimbus:notification",
               description: "通知定义",
               multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.notifications.definitions"));

        manage.AddItem(
            new ApplicationMenu(
                name: "Vben5ApiDocument",
                displayName: "Tài liệu API",
                url: "/manage/openapi",
                component: "IFrame",
                description: "Api 文档",
                multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.manage.openApi")
            // TODO: 注意在部署完毕之后手动修改此菜单iframe地址
            .SetProperty("iframeSrc", "http://127.0.0.1:30000/swagger/index.html"));

        manage.AddItem(
            new ApplicationMenu(
                name: "Vben5Caches",
                displayName: "Quản lý bộ nhớ đệm",
                url: "/manage/cache",
                component: "/caching/caches/index",
                description: "缓存管理")
            .SetProperty("title", "abp.manage.cache"));

        return new NavigationDefinition(manage);
    }

    private static NavigationDefinition GetSaas()
    {
        var saas = new ApplicationMenu(
            name: "Vben5Saas",
            displayName: "Saas",
            url: "/saas",
            component: "",
            description: "Saas",
            icon: "ant-design:cloud-server-outlined",
            multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.saas.title");
        saas.AddItem(
          new ApplicationMenu(
              name: "Vben5SaasTenants",
              displayName: "Quản lý người thuê nhà",
              url: "/saas/tenants",
              component: "/saas/tenants/index",
              icon: "arcticons:tenantcloud-pro",
              description: "租户管理",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.saas.tenants"));
        saas.AddItem(
          new ApplicationMenu(
              name: "Vben5SaasEditions",
              displayName: "Quản lý phiên bản",
              url: "/saas/editions",
              component: "/saas/editions/index",
              icon: "icon-park-outline:multi-rectangle",
              description: "版本管理",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.saas.editions"));

        return new NavigationDefinition(saas);
    }

    private static NavigationDefinition GetPlatform()
    {
        var platform = new ApplicationMenu(
            name: "Vben5Platform",
            displayName: "Quản lý nền tảng",
            url: "/platform",
            component: "",
            description: "平台管理",
            icon: "ep:platform")
            .SetProperty("title", "abp.platform.title");
        platform.AddItem(
          new ApplicationMenu(
              name: "Vben5PlatformDataDictionaries",
              displayName: "Từ điển dữ liệu",
              url: "/platform/data-dictionaries",
              component: "/platform/data-dictionaries/index",
              icon: "material-symbols:dictionary-outline",
              description: "数据字典")
            .SetProperty("title", "abp.platform.dataDictionaries"));
        platform.AddItem(
          new ApplicationMenu(
              name: "Vben5PlatformLayouts",
              displayName: "Quản lý bố cục",
              url: "/platform/layouts",
              component: "/platform/layouts/index",
              icon: "material-symbols-light:responsive-layout",
              description: "布局管理")
            .SetProperty("title", "abp.platform.layouts"));
        platform.AddItem(
          new ApplicationMenu(
              name: "Vben5PlatformMenus",
              displayName: "Quản lý thực đơn",
              url: "/platform/menus",
              component: "/platform/menus/index",
              icon: "material-symbols-light:menu",
              description: "菜单管理")
            .SetProperty("title", "abp.platform.menus"));

        var messages = platform.AddItem(
          new ApplicationMenu(
              name: "Vben5PlatformMessages",
              displayName: "Quản lý tin nhắn",
              url: "/platform/messages",
              component: "",
              icon: "tabler:message-cog",
              description: "消息管理",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.platform.messages.title"));
        messages.AddItem(
          new ApplicationMenu(
              name: "Vben5PlatformEmailMessages",
              displayName: "Thư điện tử",
              url: "/platform/messages/email",
              component: "/platform/messages/email/index",
              icon: "material-symbols:attach-email-outline",
              description: "邮件消息",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.platform.messages.email"));
        messages.AddItem(
          new ApplicationMenu(
              name: "Vben5PlatformSmsMessages",
              displayName: "Tin nhắn văn bản",
              url: "/platform/messages/sms",
              component: "/platform/messages/sms/index",
              icon: "material-symbols:sms-outline",
              description: "短信消息",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.platform.messages.sms"));

        return new NavigationDefinition(platform);
    }

    private static NavigationDefinition GetOssManagement()
    {
        var oss = new ApplicationMenu(
            name: "Vben5Oss",
            displayName: "Lưu trữ đối tượng",
            url: "/oss",
            component: "",
            description: "对象存储",
            icon: "icon-park-outline:cloud-storage")
            .SetProperty("title", "abp.oss.title");
        oss.AddItem(
          new ApplicationMenu(
              name: "Vben5OssContainers",
              displayName: "Quản lý container",
              url: "/oss/containers",
              component: "/oss/containers/index",
              icon: "mdi:bucket-outline",
              description: "容器管理")
            .SetProperty("title", "abp.oss.containers"));
        oss.AddItem(
          new ApplicationMenu(
              name: "Vben5OssObjects",
              displayName: "Quản lý tài liệu",
              url: "/oss/objects",
              component: "/oss/objects/index",
              icon: "mdi-light:file",
              description: "文件管理")
            .SetProperty("title", "abp.oss.objects"));

        return new NavigationDefinition(oss);
    }

    private static NavigationDefinition GetTaskManagement()
    {
        var task = new ApplicationMenu(
            name: "Vben5Tasks",
            displayName: "Quản lý nhiệm vụ",
            url: "/task-management",
            component: "",
            description: "任务管理",
            icon: "eos-icons:background-tasks")
            .SetProperty("title", "abp.tasks.title");
        task.AddItem(
          new ApplicationMenu(
              name: "Vben5TasksJobInfos",
              displayName: "Hàng đợi nhiệm vụ",
              url: "/task-management/background-jobs",
              component: "/tasks/job-infos/index",
              icon: "eos-icons:job",
              description: "任务队列")
            .SetProperty("title", "abp.tasks.jobInfo.title"));

        return new NavigationDefinition(task);
    }

    private static NavigationDefinition GetWebhooksManagement()
    {
        var webhooks = new ApplicationMenu(
            name: "Vben5Webhooks",
            displayName: "WebHooks",
            url: "/webhooks",
            component: "",
            description: "WebHooks",
            icon: "material-symbols:webhook",
            multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.webhooks.title");
        webhooks.AddItem(
          new ApplicationMenu(
              name: "Vben5WebhooksGroupDefinitions",
              displayName: "Nhóm Webhook",
              url: "/webhooks/groups",
              component: "/webhooks/groups/index",
              icon: "lucide:group",
              description: "Webhook分组",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.webhooks.groups"));
        webhooks.AddItem(
          new ApplicationMenu(
              name: "Vben5WebhooksDefinitions",
              displayName: "Định nghĩa Webhook",
              url: "/webhooks/definitions",
              component: "/webhooks/definitions/index",
              icon: "material-symbols:webhook",
              description: "Webhook定义",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.webhooks.definitions"));
        webhooks.AddItem(
          new ApplicationMenu(
              name: "Vben5WebhooksSubscriptions",
              displayName: "Quản lý đăng ký",
              url: "/webhooks/subscriptions",
              component: "/webhooks/subscriptions/index",
              icon: "material-symbols:subscriptions",
              description: "管理订阅",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.webhooks.subscriptions"));
        webhooks.AddItem(
          new ApplicationMenu(
              name: "Vben5WebhooksSendAttempts",
              displayName: "Quản lý hồ sơ",
              url: "/webhooks/send-attempts",
              component: "/webhooks/send-attempts/index",
              icon: "material-symbols:history",
              description: "管理记录",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.webhooks.sendAttempts"));

        return new NavigationDefinition(webhooks);
    }

    private static NavigationDefinition GetTextTemplating()
    {
        var textTemplating = new ApplicationMenu(
            name: "Vben5TextTemplating",
            displayName: "Quản lý mẫu",
            url: "/text-templating",
            component: "",
            description: "模板管理",
            icon: "tdesign:template",
            multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.textTemplating.title");
        textTemplating.AddItem(
          new ApplicationMenu(
              name: "Vben5TextTemplatingDefinitions",
              displayName: "Định nghĩa mẫu",
              url: "/text-templating/definitions",
              component: "/text-templating/definitions/index",
              icon: "qlementine-icons:template-16",
              description: "模板定义",
              multiTenancySides: MultiTenancySides.Host)
            .SetProperty("title", "abp.textTemplating.definitions"));

        return new NavigationDefinition(textTemplating);
    }
}
