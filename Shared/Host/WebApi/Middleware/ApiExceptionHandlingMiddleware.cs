using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Wms.Application.Common;
using Wms.WebApi.Telemetry;

namespace Wms.WebApi.Middleware;

public sealed class ApiExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiExceptionHandlingMiddleware> _logger;

    public ApiExceptionHandlingMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ILocalizationService localizationService)
    {
        try
        {
            await _next(context);
        }
        catch (OperationCanceledException ex) when (context.RequestAborted.IsCancellationRequested)
        {
            context.SetRequestOutcome(RequestOutcome.Cancelled);
            _logger.LogWarning(ex, "HTTP request cancelled TraceId={TraceId} Path={Path}", context.GetTraceId(), context.Request.Path.Value);

            if (!context.Response.HasStarted)
            {
                context.Response.Clear();
                context.Response.StatusCode = 499;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new ApiResponse<object?>
                {
                    Success = false,
                    Message = localizationService.GetLocalizedString("OperationCancelled"),
                    ExceptionMessage = ex.Message,
                    Errors = new List<string>(),
                    StatusCode = 499,
                    ClassName = "ApiResponse<Object>",
                    TraceId = context.GetTraceId()
                });
            }
        }
        catch (Exception ex)
        {
            context.SetRequestOutcome(RequestOutcome.UnhandledException);
            _logger.LogError(ex, "Unhandled exception TraceId={TraceId} Path={Path}", context.GetTraceId(), context.Request.Path.Value);

            if (!context.Response.HasStarted)
            {
                var message = localizationService.GetLocalizedString("InternalServerError");

                context.Response.Clear();
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(new ApiResponse<object?>
                {
                    Success = false,
                    Message = message,
                    ExceptionMessage = ex.InnerException?.Message ?? ex.Message,
                    Errors = new List<string>(),
                    StatusCode = StatusCodes.Status500InternalServerError,
                    ClassName = "ApiResponse<Object>",
                    TraceId = context.GetTraceId()
                });
            }
        }
    }
}
