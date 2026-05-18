namespace Wms.Application.Common;

public interface IJobTraceContextAccessor
{
    JobTraceContext? Current { get; set; }
}
