using Wms.Application.Common;

namespace Wms.Infrastructure.Services.Jobs;

public sealed class JobTraceContextAccessor : IJobTraceContextAccessor
{
    private static readonly AsyncLocal<JobTraceContext?> CurrentContext = new();

    public JobTraceContext? Current
    {
        get => CurrentContext.Value;
        set => CurrentContext.Value = value;
    }
}
