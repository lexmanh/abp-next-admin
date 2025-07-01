using LY.MicroService.LocalizationManagement.EntityFrameworkCore;
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

namespace LY.MicroService.LocalizationManagement;

public class Program
{
    public async static Task<int> Main(string[] args)
    {
        try
        {
            Console.Title = "LocalizationManagement.HttpApi.Host";
            Log.Information("Starting LocalizationManagement.HttpApi.Host.");

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
            await builder.AddApplicationAsync<LocalizationManagementHttpApiHostModule>(options =>
            {
                LocalizationManagementHttpApiHostModule.ApplicationName = Environment.GetEnvironmentVariable("APPLICATION_NAME")
                    ?? LocalizationManagementHttpApiHostModule.ApplicationName;
                options.ApplicationName = LocalizationManagementHttpApiHostModule.ApplicationName;
                // �ӻ�������ȡ�û���������, ��������������
                options.Configuration.UserSecretsId = Environment.GetEnvironmentVariable("APPLICATION_USER_SECRETS_ID");
                // �������û��ָ���û�����, ����Ŀ��ȡ
                options.Configuration.UserSecretsAssembly = typeof(LocalizationManagementHttpApiHostModule).Assembly;
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
            await app.InitializeApplicationAsync();
            
            // Run database migrations
            using (var scope = app.Services.CreateScope())
            {
                var migrationService = scope.ServiceProvider.GetRequiredService<LocalizationManagementDbMigrationService>();
                await migrationService.CheckAndApplyDatabaseMigrationsAsync();
            }
            
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
