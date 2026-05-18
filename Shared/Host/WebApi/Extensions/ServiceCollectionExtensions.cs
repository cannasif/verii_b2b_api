using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Reflection;
using System.Text;
using Wms.Application.AccessControl.Services;
using Wms.Application.B2B.Services;
using Wms.Application.Common;
using Wms.Application.Communications.Services;
using Wms.Application.Customer.Services;
using Wms.Application.Identity.Services;
using Wms.Application.Stock.Services;
using Wms.Application.Warehouse.Services;
using Wms.Application.YapKod.Services;
using Wms.Infrastructure.Options;
using Wms.Infrastructure.Persistence.Context;
using Wms.Infrastructure.Persistence.Repositories;
using Wms.Infrastructure.Persistence.UnitOfWork;
using Wms.Infrastructure.Services.Auditing;
using Wms.Infrastructure.Services.Communications;
using Wms.Infrastructure.Services.Customer;
using Wms.Infrastructure.Services.Erp;
using Wms.Infrastructure.Services.Files;
using Wms.Infrastructure.Services.Identity;
using Wms.Infrastructure.Services.Integrations;
using Wms.Infrastructure.Services.Localization;
using Wms.Infrastructure.Services.Security;
using Wms.Infrastructure.Services.Stock;
using Wms.Infrastructure.Services.Warehouse;
using Wms.Infrastructure.Services.YapKod;
using Wms.Modules.NetsisIntegrations.Infrastructure;
using Wms.WebApi.Filters;
using Wms.WebApi.Localization;
using Wms.WebApi.Options;
using Wms.WebApi.Telemetry;

namespace Wms.WebApi.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPragmaticWebApi(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<PragmaticCorsOptions>(configuration.GetSection("Cors"));
        services.AddNetsisIntegrationsModule(configuration);

        var corsOptions = configuration.GetSection("Cors").Get<PragmaticCorsOptions>() ?? new PragmaticCorsOptions();
        services.AddCors(options =>
        {
            options.AddPolicy("PragmaticCors", policy =>
            {
                if (corsOptions.AllowedOrigins.Count > 0)
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins.ToArray())
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                }
                else
                {
                    policy.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                }
            });
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration.GetConnectionString("Default")
            ?? "Server=(localdb)\\mssqllocaldb;Database=v3riib2b;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContext<WmsDbContext>(options => options.UseSqlServer(connectionString));

        var jwtSecret = configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var jwtIssuer = configuration["Jwt:Issuer"] ?? "B2B_API";
        var jwtAudience = configuration["Jwt:Audience"] ?? "B2B_Client";

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        services.AddControllers(options => options.Filters.AddService<ApiResponseTraceFilter>());
        services.AddScoped<ApiResponseTraceFilter>();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddAutoMapper(_ => { }, typeof(Wms.Application.Common.AssemblyMarker).Assembly);
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(typeof(Wms.Application.Common.AssemblyMarker).Assembly, ServiceLifetime.Scoped);
        services.AddHttpContextAccessor();
        services.AddHttpClient();
        services.AddMemoryCache();
        services.AddDataProtection();
        services.AddLocalization();
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(LocalizationDefaults.DefaultCulture);
            options.SupportedCultures = LocalizationDefaults.SupportedCultures.ToList();
            options.SupportedUICultures = LocalizationDefaults.SupportedCultures.ToList();
            options.RequestCultureProviders.Insert(0, new CustomHeaderRequestCultureProvider());
        });

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("verii_b2b_api"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(WmsTelemetry.ActivitySourceName)
                    .AddAspNetCoreInstrumentation(options => options.RecordException = true)
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.SetDbStatementForText = false;
                        options.RecordException = true;
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter(WmsTelemetry.MeterName)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            });

        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IErpUnitOfWork, ErpUnitOfWorkAdapter>();

        foreach (var resourceType in typeof(AssemblyMarker).Assembly
                     .GetTypes()
                     .Where(type => typeof(ILocalizationResource).IsAssignableFrom(type) && type is { IsClass: true, IsAbstract: false }))
        {
            services.AddSingleton(typeof(ILocalizationResource), resourceType);
        }

        services.AddSingleton<LocalizationRegistry>();
        services.AddScoped<ILocalizationService, PragmaticLocalizationService>();
        services.AddScoped<ICurrentUserAccessor, HttpCurrentUserAccessor>();
        services.AddScoped<ICurrentUserService, CurrentUserServiceAdapter>();
        services.AddScoped<IRequestTraceAccessor, HttpRequestTraceAccessor>();
        services.AddScoped<IAuditLogWriter, AuditLogWriter>();
        services.AddScoped<IAuditSnapshotHelper, AuditSnapshotHelper>();
        services.AddScoped<IIntegrationLogWriter, IntegrationLogWriter>();
        services.AddScoped<IRequestCancellationAccessor, RequestCancellationAccessor>();
        services.AddScoped<IFileUploadService, FileUploadService>();

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IResetPasswordEmailJob, ResetPasswordEmailJob>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserAuthorityService, UserAuthorityService>();
        services.AddScoped<IUserDetailService, UserDetailService>();

        services.AddScoped<IMailService, MailService>();
        services.AddScoped<ISmtpSettingsService, SmtpSettingsService>();

        services.AddScoped<IPermissionDefinitionService, PermissionDefinitionService>();
        services.AddScoped<IPermissionGroupService, PermissionGroupService>();
        services.AddScoped<IUserPermissionGroupService, UserPermissionGroupService>();
        services.AddScoped<IPermissionAccessService, PermissionAccessService>();
        services.AddScoped<IWmsScopePolicyService, WmsScopePolicyService>();
        services.AddScoped<IWmsOperationScopeEnforcer, WmsOperationScopeEnforcer>();

        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<ICustomerErpReadService, CustomerErpReadService>();
        services.AddScoped<ICustomerSyncJob, CustomerSyncJob>();

        services.AddScoped<IStockService, StockService>();
        services.AddScoped<IStockDetailService, StockDetailService>();
        services.AddScoped<IStockImageService, StockImageService>();
        services.AddScoped<IStockErpReadService, StockErpReadService>();
        services.AddScoped<IStockSyncJob, StockSyncJob>();

        services.AddScoped<IWarehouseService, WarehouseService>();
        services.AddScoped<IWarehouseErpReadService, WarehouseErpReadService>();
        services.AddScoped<IWarehouseSyncJob, WarehouseSyncJob>();

        services.AddScoped<IYapKodService, YapKodService>();
        services.AddScoped<IYapKodErpReadService, YapKodErpReadService>();
        services.AddScoped<IYapKodSyncJob, YapKodSyncJob>();

        services.AddScoped<IB2bCommerceService, B2bCommerceService>();
        services.AddScoped<IB2bCommercialPolicyService, B2bCommercialPolicyService>();
        services.AddScoped<IB2bPricingAvailabilityResolver, B2bPricingAvailabilityResolver>();
        services.AddScoped<IB2bAccountService, B2bAccountService>();
        services.AddScoped<IB2bInsightService, B2bInsightService>();

        return services;
    }
}
