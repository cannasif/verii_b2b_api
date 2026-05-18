using Microsoft.Extensions.Options;
using Wms.Modules.NetsisIntegrations.Application.Services;
using Wms.Modules.NetsisIntegrations.Infrastructure.Auth;
using Wms.Modules.NetsisIntegrations.Infrastructure.Clients;
using Wms.Modules.NetsisIntegrations.Infrastructure.Options;
using Wms.Modules.NetsisIntegrations.Infrastructure.Persistence;

namespace Wms.Modules.NetsisIntegrations.Infrastructure;

public static class NetsisIntegrationsModule
{
    public static IServiceCollection AddNetsisIntegrationsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<NetsisOptions>(configuration.GetSection(NetsisOptions.SectionName));
        services.PostConfigure<NetsisOptions>(options => ApplyLegacyNetsisRestOptions(options, configuration));

        services.AddHttpClient<INetsisRestClient, NetsisRestClient>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<NetsisOptions>>().Value;
            ConfigureNetsisHttpClient(client, options);
        })
        .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<NetsisOptions>>().Value;
            return BuildNetsisHttpHandler(options);
        });

        services.AddHttpClient<INetsisAuthTokenService, NetsisAuthTokenService>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<NetsisOptions>>().Value;
            ConfigureNetsisHttpClient(client, options);
        })
        .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<NetsisOptions>>().Value;
            return BuildNetsisHttpHandler(options);
        });

        services.AddScoped<INetsisQueryExecutor, NetsisQueryExecutor>();
        services.AddScoped<INetsisItemSlipService, NetsisItemSlipService>();
        services.AddScoped<INetsisReadService, NetsisReadService>();

        return services;
    }

    private static void ConfigureNetsisHttpClient(HttpClient client, NetsisOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Rest.BaseUrl))
        {
            client.BaseAddress = new Uri(options.Rest.BaseUrl, UriKind.Absolute);
        }

        client.Timeout = TimeSpan.FromSeconds(options.Rest.TimeoutSeconds > 0 ? options.Rest.TimeoutSeconds : 30);
    }

    private static HttpClientHandler BuildNetsisHttpHandler(NetsisOptions options)
    {
        return new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = options.Rest.AllowInvalidSslCertificate
                ? HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                : null
        };
    }

    private static void ApplyLegacyNetsisRestOptions(NetsisOptions options, IConfiguration configuration)
    {
        var legacySection = configuration.GetSection("NetsisRest");
        if (!legacySection.Exists())
        {
            return;
        }

        options.Enabled = options.Enabled || legacySection.GetValue<bool>("Enabled");
        options.Rest.BaseUrl = string.IsNullOrWhiteSpace(options.Rest.BaseUrl)
            ? legacySection.GetValue<string>("BaseUrl") ?? string.Empty
            : options.Rest.BaseUrl;
        options.Rest.LoginPath = string.IsNullOrWhiteSpace(options.Rest.LoginPath)
            ? legacySection.GetValue<string>("LoginPath") ?? "/api/v2/token"
            : options.Rest.LoginPath;
        options.Rest.ItemSlipsPath = string.IsNullOrWhiteSpace(options.Rest.ItemSlipsPath)
            ? legacySection.GetValue<string>("ItemSlipsPath")
                ?? legacySection.GetValue<string>("SalesInvoicePath")
                ?? "/api/v2/ItemSlips"
            : options.Rest.ItemSlipsPath;
        options.Rest.TimeoutSeconds = legacySection.GetValue<int?>("TimeoutSeconds") ?? options.Rest.TimeoutSeconds;
        options.Rest.AllowInvalidSslCertificate = options.Rest.AllowInvalidSslCertificate
            || legacySection.GetValue<bool>("AllowInvalidSslCertificate");
        options.Rest.Username = string.IsNullOrWhiteSpace(options.Rest.Username)
            ? legacySection.GetValue<string>("Username") ?? string.Empty
            : options.Rest.Username;
        options.Rest.Password = string.IsNullOrWhiteSpace(options.Rest.Password)
            ? legacySection.GetValue<string>("Password") ?? string.Empty
            : options.Rest.Password;
        options.Rest.BranchCode = string.IsNullOrWhiteSpace(options.Rest.BranchCode)
            ? legacySection.GetValue<string>("BranchCode") ?? string.Empty
            : options.Rest.BranchCode;
        options.Rest.DbName = string.IsNullOrWhiteSpace(options.Rest.DbName)
            ? legacySection.GetValue<string>("DbName") ?? legacySection.GetValue<string>("Database") ?? string.Empty
            : options.Rest.DbName;
        options.Rest.DbUser = string.IsNullOrWhiteSpace(options.Rest.DbUser)
            ? legacySection.GetValue<string>("DbUser") ?? string.Empty
            : options.Rest.DbUser;
        options.Rest.DbPassword = string.IsNullOrWhiteSpace(options.Rest.DbPassword)
            ? legacySection.GetValue<string>("DbPassword") ?? string.Empty
            : options.Rest.DbPassword;
        options.Rest.DbType = string.IsNullOrWhiteSpace(options.Rest.DbType)
            ? legacySection.GetValue<string>("DbType") ?? options.Rest.DbType
            : options.Rest.DbType;
    }
}
