using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.Common;
using Wms.Infrastructure.Persistence.Context;
using Wms.WebApi.Telemetry;

namespace Wms.Infrastructure.Services.Auditing;

public sealed class AuditLogWriter : IAuditLogWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false
    };

    private readonly WmsDbContext _dbContext;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IRequestTraceAccessor _requestTraceAccessor;
    private readonly ILogger<AuditLogWriter> _logger;

    public AuditLogWriter(
        WmsDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        IRequestTraceAccessor requestTraceAccessor,
        ILogger<AuditLogWriter> logger)
    {
        _dbContext = dbContext;
        _currentUserAccessor = currentUserAccessor;
        _requestTraceAccessor = requestTraceAccessor;
        _logger = logger;
    }

    public async Task WriteAsync(AuditLogWriteRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var entry = new WmsAuditLog
            {
                TraceId = request.TraceId ?? _requestTraceAccessor.TraceId,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                ActionType = request.ActionType,
                PerformedByUserId = request.PerformedByUserId ?? _currentUserAccessor.UserId,
                PerformedByUserEmail = request.PerformedByUserEmail ?? _currentUserAccessor.UserEmail,
                BranchCode = BranchCodeDefaults.Normalize(request.BranchCode ?? _currentUserAccessor.BranchCode),
                Result = request.Result,
                Source = request.Source,
                OldValues = Serialize(request.OldValues),
                NewValues = Serialize(request.NewValues),
                ChangedFields = Serialize(request.ChangedFields),
                Reason = request.Reason,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            };

            _dbContext.Set<WmsAuditLog>().Add(entry);
            await _dbContext.SaveChangesAsync(cancellationToken);

            WmsTelemetry.RecordAuditWrite(entry.Result, entry.Source, entry.EntityType);
        }
        catch (Exception ex)
        {
            WmsTelemetry.RecordAuditWriteFailure(request.Source, request.EntityType);
            _logger.LogWarning(
                ex,
                "Audit log write failed TraceId={TraceId} EntityType={EntityType} EntityId={EntityId} ActionType={ActionType}",
                request.TraceId ?? _requestTraceAccessor.TraceId,
                request.EntityType,
                request.EntityId,
                request.ActionType);
        }
    }

    private static string? Serialize(object? value)
    {
        return value == null ? null : JsonSerializer.Serialize(value, SerializerOptions);
    }
}
