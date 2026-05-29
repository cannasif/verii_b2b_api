using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Wms.Application.Common;
using Wms.Application.Customer.Services;
using Wms.Application.B2B.Services;
using Wms.Application.Stock.Services;
using Wms.Application.Warehouse.Services;
using Wms.Application.YapKod.Services;
using Wms.WebApi.Extensions;
using Wms.WebApi.Helpers;
using Wms.WebApi.Middleware;
using Wms.Infrastructure.Options;
using Wms.WebApi.Startup;

var builder = WebApplication.CreateBuilder(args);

var sharedConfigDirectory = Path.Combine(
    builder.Environment.ContentRootPath,
    "Shared",
    "Host",
    "WebApi",
    "Config");
var sharedAppsettingsPath = Path.Combine(sharedConfigDirectory, "appsettings.json");
var sharedEnvironmentAppsettingsPath = Path.Combine(sharedConfigDirectory, $"appsettings.{builder.Environment.EnvironmentName}.json");
var rootAppsettingsPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.json");
var rootEnvironmentAppsettingsPath = Path.Combine(builder.Environment.ContentRootPath, $"appsettings.{builder.Environment.EnvironmentName}.json");

builder.Configuration
    .AddJsonFile(File.Exists(sharedAppsettingsPath) ? sharedAppsettingsPath : rootAppsettingsPath, optional: false, reloadOnChange: false)
    .AddJsonFile(
        File.Exists(sharedEnvironmentAppsettingsPath) ? sharedEnvironmentAppsettingsPath : rootEnvironmentAppsettingsPath,
        optional: true,
        reloadOnChange: false)
    .AddEnvironmentVariables();

builder.Services.AddPragmaticWebApi(builder.Configuration);

var app = builder.Build();

await app.EnsureIdentitySeedAsync();

GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute
{
    Attempts = 3,
    DelaysInSeconds = new[] { 60, 300, 900 },
    LogEvents = true,
    OnAttemptsExceeded = AttemptsExceededAction.Fail
});
GlobalJobFilters.Filters.Add(
    new HangfireTraceContextFilter(
        app.Services.GetRequiredService<ILogger<HangfireTraceContextFilter>>(),
        app.Services.GetRequiredService<IHttpContextAccessor>(),
        app.Services.GetRequiredService<IJobTraceContextAccessor>()));
GlobalJobFilters.Filters.Add(
    new HangfireJobStateFilter(
        app.Services.GetRequiredService<ILogger<HangfireJobStateFilter>>(),
        app.Services.GetRequiredService<IBackgroundJobClient>(),
        app.Services.GetRequiredService<IJobTraceContextAccessor>(),
        app.Services.GetRequiredService<IOptions<HangfireMonitoringOptions>>(),
        app.Services.GetRequiredService<IServiceScopeFactory>()));

var swaggerEnabled = builder.Configuration.GetValue("Swagger:Enabled", true);
if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("PragmaticCors");
app.UseStaticFiles();
app.UseRequestLocalization();

app.Use(async (context, next) =>
{
    var branchCode = context.Request.Headers["X-Branch-Code"].FirstOrDefault();
    context.Items["BranchCode"] = BranchCodeDefaults.Normalize(branchCode);
    await next();
});

app.UseMiddleware<RequestTraceMiddleware>();
app.UseMiddleware<ApiExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

var recurringJobManager = app.Services.GetRequiredService<IRecurringJobManager>();
var erpMirrorCron = app.Environment.IsDevelopment()
    ? Cron.Yearly()
    : Cron.Hourly();
recurringJobManager.AddOrUpdate<ICustomerSyncJob>(
    "erp-customer-sync-job",
    job => job.RunAsync(CancellationToken.None),
    erpMirrorCron);
recurringJobManager.AddOrUpdate<IStockSyncJob>(
    "erp-stock-sync-job",
    job => job.RunAsync(CancellationToken.None),
    erpMirrorCron);
recurringJobManager.AddOrUpdate<IWarehouseSyncJob>(
    "erp-warehouse-sync-job",
    job => job.RunAsync(CancellationToken.None),
    erpMirrorCron);
recurringJobManager.AddOrUpdate<IYapKodSyncJob>(
    "erp-yapkod-sync-job",
    job => job.RunAsync(CancellationToken.None),
    erpMirrorCron);
recurringJobManager.AddOrUpdate<IB2bErpTransferJob>(
    "b2b-erp-transfer-job",
    job => job.RunAsync(CancellationToken.None),
    app.Environment.IsDevelopment() ? Cron.Yearly() : Cron.Minutely());

app.Logger.LogInformation(
    "Registered recurring ERP mirror jobs with cron '{Cron}' for environment '{EnvironmentName}'.",
    erpMirrorCron,
    app.Environment.EnvironmentName);

if (app.Environment.IsDevelopment())
{
    var backgroundJobs = app.Services.GetRequiredService<IBackgroundJobClient>();
    backgroundJobs.Enqueue<ICustomerSyncJob>(job => job.RunAsync(CancellationToken.None));
    backgroundJobs.Enqueue<IStockSyncJob>(job => job.RunAsync(CancellationToken.None));
    backgroundJobs.Enqueue<IWarehouseSyncJob>(job => job.RunAsync(CancellationToken.None));
    backgroundJobs.Enqueue<IYapKodSyncJob>(job => job.RunAsync(CancellationToken.None));

    app.Logger.LogInformation("Queued development ERP mirror jobs for manual testing.");
}

app.MapControllers();

app.Run();

public partial class Program { }
