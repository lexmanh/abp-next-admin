using LY.MicroService.Platform.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;
using Volo.Abp.IO;
using Volo.Abp.Modularity.PlugIns;

namespace LY.MicroService.PlatformManagement;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        try
        {
            Console.Title = "PlatformManagement.HttpApi.Host";
            Log.Information("Starting PlatformManagement.HttpApi.Host.");

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.AddAppSettingsSecretsJson()
                .UseAutofac()
                .ConfigureAppConfiguration((context, config) =>
                {
                    var configuration = config.Build();
                    var agileConfigEnabled = configuration["AgileConfig:IsEnabled"];
                    if (agileConfigEnabled.IsNullOrEmpty() || bool.Parse(agileConfigEnabled))
                    {
                        config.AddAgileConfig(new AgileConfig.Client.ConfigClient(configuration));
                    }
                })
                .UseSerilog((context, provider, config) =>
                {
                    config.ReadFrom.Configuration(context.Configuration);
                });
            await builder.AddApplicationAsync<PlatformManagementHttpApiHostModule>(options =>
            {
                PlatformManagementHttpApiHostModule.ApplicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME")
                    ?? PlatformManagementHttpApiHostModule.ApplicationName;
                options.ApplicationName = PlatformManagementHttpApiHostModule.ApplicationName;
                // �ӻ�������ȡ�û���������, ��������������
                options.Configuration.UserSecretsId = Environment.GetEnvironmentVariable("APPLICATION_USER_SECRETS_ID");
                // �������û��ָ���û�����, ����Ŀ��ȡ
                options.Configuration.UserSecretsAssembly = typeof(PlatformManagementHttpApiHostModule).Assembly;
                // ���� Modules Ŀ¼�������ļ���Ϊ���
                // ȡ����ʾ��������������Ŀ��ģ�飬��Ϊͨ���������ʽ����
                var pluginFolder = Path.Combine(
                        Directory.GetCurrentDirectory(), "Modules");
                DirectoryHelper.CreateIfNotExists(pluginFolder);
                options.PlugInSources.AddFolder(
                    pluginFolder,
                    SearchOption.AllDirectories);
            });
            var app = builder.Build();
            using (var scope = app.Services.CreateScope())
            {
                Log.Information("Checking and applying database migrations...");
                var migrationService = scope.ServiceProvider.GetRequiredService<PlatformDbMigrationService>();
                await migrationService.CheckAndApplyDatabaseMigrationsAsync();
            }
            await app.InitializeApplicationAsync();
            await app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Host terminated unexpectedly!");
            Console.WriteLine("Host terminated unexpectedly!");
            Console.WriteLine(ex.ToString());
            return 1;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
