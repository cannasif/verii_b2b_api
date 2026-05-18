using Microsoft.AspNetCore.SignalR;
using Wms.Application.Common;
using Wms.Application.Communications.Services;
using Wms.WebApi.Hubs;

namespace Wms.WebApi.Realtime;

public sealed class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IRequestTraceAccessor _requestTraceAccessor;
    private readonly ILogger<SignalRNotificationPublisher> _logger;

    public SignalRNotificationPublisher(
        IHubContext<NotificationHub> hubContext,
        IRequestTraceAccessor requestTraceAccessor,
        ILogger<SignalRNotificationPublisher> logger)
    {
        _hubContext = hubContext;
        _requestTraceAccessor = requestTraceAccessor;
        _logger = logger;
    }

    public Task PublishToUserAsync(string userId, object payload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SignalR notification publish-to-user UserId={UserId} TraceId={TraceId}", userId, _requestTraceAccessor.TraceId);
        return NotificationHub.SendNotificationToUser(_hubContext, userId, payload, cancellationToken);
    }

    public Task PublishToAllAsync(object payload, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("SignalR notification publish-to-all TraceId={TraceId}", _requestTraceAccessor.TraceId);
        return NotificationHub.SendNotificationToAll(_hubContext, payload, cancellationToken);
    }
}
