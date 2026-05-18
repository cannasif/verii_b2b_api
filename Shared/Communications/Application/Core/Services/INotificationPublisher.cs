namespace Wms.Application.Communications.Services;

public interface INotificationPublisher
{
    Task PublishToUserAsync(string userId, object payload, CancellationToken cancellationToken = default);
    Task PublishToAllAsync(object payload, CancellationToken cancellationToken = default);
}
