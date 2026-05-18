namespace Wms.Application.Common;

public interface IJobFailureLogWriter
{
    Task WriteAsync(
        string jobId,
        string jobName,
        string? reason,
        Exception exception,
        string queue = "default",
        int retryCount = 0,
        CancellationToken cancellationToken = default);
}
