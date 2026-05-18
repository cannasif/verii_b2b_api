using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Services.Integrations;

public sealed class HangfireDeadLetterJob : IHangfireDeadLetterJob
{
    private readonly IRepository<JobFailureLog> _jobFailureLogs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRequestTraceAccessor _requestTraceAccessor;

    public HangfireDeadLetterJob(
        IRepository<JobFailureLog> jobFailureLogs,
        IUnitOfWork unitOfWork,
        IRequestTraceAccessor requestTraceAccessor)
    {
        _jobFailureLogs = jobFailureLogs;
        _unitOfWork = unitOfWork;
        _requestTraceAccessor = requestTraceAccessor;
    }

    public async Task ProcessAsync(HangfireDeadLetterPayload payload)
    {
        await _jobFailureLogs.AddAsync(new JobFailureLog
        {
            TraceId = payload.TraceId ?? _requestTraceAccessor.TraceId,
            JobId = payload.JobId,
            JobName = payload.JobName,
            FailedAt = payload.OccurredAtUtc,
            Reason = payload.Reason,
            ExceptionType = payload.ExceptionType,
            ExceptionMessage = payload.ExceptionMessage,
            Queue = payload.Queue,
            RetryCount = payload.RetryCount,
            CreatedDate = DateTimeProvider.Now,
            IsDeleted = false
        });

        await _unitOfWork.SaveChangesAsync();
    }
}
