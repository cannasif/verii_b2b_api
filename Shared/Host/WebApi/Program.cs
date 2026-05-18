using Microsoft.AspNetCore.Http;
using Wms.Application.Common;
using Wms.WebApi.Extensions;
using Wms.WebApi.Middleware;

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

app.MapControllers();

app.Run();

public partial class Program { }
