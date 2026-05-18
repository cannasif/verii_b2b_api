using Wms.Application.Common;
using Wms.Domain.Entities.Common;
using Wms.WebApi.Telemetry;

namespace Wms.Infrastructure.Services.Integrations;

public sealed class IntegrationLogWriter : IIntegrationLogWriter
{
    private readonly IRepository<WmsIntegrationLog> _integrationLogs;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<IntegrationLogWriter> _logger;

    public IntegrationLogWriter(
        IRepository<WmsIntegrationLog> integrationLogs,
        IUnitOfWork unitOfWork,
        ILogger<IntegrationLogWriter> logger)
    {
        _integrationLogs = integrationLogs;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task TryWriteAsync(IntegrationLogWriteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new WmsIntegrationLog
            {
                TraceId = request.TraceId,
                IntegrationType = request.IntegrationType,
                TargetSystem = request.TargetSystem,
                Operation = request.Operation,
                Status = request.Status,
                Source = request.Source,
                RequestMetadata = request.RequestMetadata,
                ResponseMetadata = request.ResponseMetadata,
                ErrorMessage = request.ErrorMessage,
                ErrorType = request.ErrorType,
                StartedAt = request.StartedAt,
                FinishedAt = request.FinishedAt,
                DurationMs = request.DurationMs,
                CreatedDate = request.FinishedAt,
                IsDeleted = false,
            };

            await _integrationLogs.AddAsync(entity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            WmsTelemetry.RecordIntegrationExecution(request.TargetSystem, request.Operation, request.Status);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Integration log write failed TraceId={TraceId} TargetSystem={TargetSystem} Operation={Operation}",
                request.TraceId,
                request.TargetSystem,
                request.Operation);

            WmsTelemetry.RecordIntegrationExecution(request.TargetSystem, request.Operation, "LogWriteFailed");
        }
    }
}
