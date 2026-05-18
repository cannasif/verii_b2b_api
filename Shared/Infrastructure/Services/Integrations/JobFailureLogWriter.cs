using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Services.Integrations;

public sealed class JobFailureLogWriter : IJobFailureLogWriter
{
    private readonly IRepository<JobFailureLog> _jobFailureLogs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestTraceAccessor _requestTraceAccessor;
    private readonly ILogger<JobFailureLogWriter> _logger;

    public JobFailureLogWriter(
        IRepository<JobFailureLog> jobFailureLogs,
        IUnitOfWork unitOfWork,
        IRequestTraceAccessor requestTraceAccessor,
        ILogger<JobFailureLogWriter> logger)
    {
        _jobFailureLogs = jobFailureLogs;
        _unitOfWork = unitOfWork;
        _requestTraceAccessor = requestTraceAccessor;
        _logger = logger;
    }

    public async Task WriteAsync(
        string jobId,
        string jobName,
        string? reason,
        Exception exception,
        string queue = "default",
        int retryCount = 0,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new JobFailureLog
            {
                TraceId = _requestTraceAccessor.TraceId,
                JobId = jobId,
                JobName = jobName,
                FailedAt = DateTime.UtcNow,
                Reason = reason,
                ExceptionType = exception.GetType().FullName,
                ExceptionMessage = exception.Message,
                StackTrace = Truncate(exception.StackTrace, 4000),
                Queue = queue,
                RetryCount = retryCount,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            };

            await _jobFailureLogs.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception logEx)
        {
            _logger.LogWarning(logEx, "JobFailureLog write failed. JobId: {JobId} TraceId={TraceId}", jobId, _requestTraceAccessor.TraceId);
        }
    }

    private static string? Truncate(string? value, int maxLength)
        => string.IsNullOrWhiteSpace(value) ? value : value.Length <= maxLength ? value : value[..maxLength];
}
