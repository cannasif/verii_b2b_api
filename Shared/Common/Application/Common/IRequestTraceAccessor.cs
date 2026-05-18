namespace Wms.Application.Common;

public interface IRequestTraceAccessor
{
    string? TraceId { get; }
}
