using Wms.Application.Common;
using Wms.Application.Communications.Dtos;
using Wms.Domain.Entities.Communications;

namespace Wms.Application.Communications.Services;

public interface INotificationService
{
    Task<ApiResponse<NotificationDto>> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<IEnumerable<NotificationDto>>> CreateForUsersAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<NotificationDto>>> GetPagedByRecipientUserIdAsync(long userId, PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAsReadAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAsReadBulkAsync(List<long> ids, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAllAsReadAsync(long userId, CancellationToken cancellationToken = default);
    List<Notification> CreateNotificationsForTerminalLines<TTerminalLine>(
        IEnumerable<TTerminalLine> terminalLines,
        string orderNumber,
        string entityType,
        string terminalActionCode,
        string titleLocalizationKey,
        string messageLocalizationKey) where TTerminalLine : class;
    Task<List<Notification>> CreateAndAddNotificationsForTerminalLinesAsync<TTerminalLine>(
        IEnumerable<TTerminalLine> terminalLines,
        string orderNumber,
        string entityType,
        string terminalActionCode,
        string titleLocalizationKey,
        string messageLocalizationKey,
        CancellationToken cancellationToken = default) where TTerminalLine : class;
    Task PublishSignalRNotificationsAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default);
    Task PublishSignalRNotificationsForCreatedNotificationsAsync(List<Notification> notifications, CancellationToken cancellationToken = default);
}
