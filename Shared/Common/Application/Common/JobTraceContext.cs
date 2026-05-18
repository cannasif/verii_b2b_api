using System.Diagnostics;

namespace Wms.Application.Common;

public sealed class JobTraceContext
{
    public string TraceId { get; init; } = string.Empty;
    public string? ParentTraceId { get; init; }
    public string? Source { get; init; }
    public string? JobId { get; init; }
    public string? RecurringJobId { get; init; }
    public Activity? Activity { get; init; }
}
