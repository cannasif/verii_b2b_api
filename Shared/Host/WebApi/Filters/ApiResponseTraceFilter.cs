using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Wms.Application.Common;

namespace Wms.WebApi.Filters;

public sealed class ApiResponseTraceFilter : IAsyncResultFilter
{
    private readonly IRequestTraceAccessor _requestTraceAccessor;

    public ApiResponseTraceFilter(IRequestTraceAccessor requestTraceAccessor)
    {
        _requestTraceAccessor = requestTraceAccessor;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: ITraceableApiResponse traceableResponse })
        {
            traceableResponse.TraceId ??= _requestTraceAccessor.TraceId;
        }

        await next();
    }
}
