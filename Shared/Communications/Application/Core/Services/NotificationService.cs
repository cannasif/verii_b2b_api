using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.Communications.Dtos;
using Wms.Domain.Common;
using Wms.Domain.Entities.Communications;
using Wms.Domain.Entities.Communications.Enums;

namespace Wms.Application.Communications.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IRepository<Notification> _notifications;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationPublisher _notificationPublisher;
    private readonly IRequestTraceAccessor _requestTraceAccessor;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IRepository<Notification> notifications,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILocalizationService localizationService,
        INotificationPublisher notificationPublisher,
        IRequestTraceAccessor requestTraceAccessor,
        ILogger<NotificationService> logger)
    {
        _notifications = notifications;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _localizationService = localizationService;
        _notificationPublisher = notificationPublisher;
        _requestTraceAccessor = requestTraceAccessor;
        _logger = logger;
    }

    public async Task<ApiResponse<NotificationDto>> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<Notification>(dto);
        entity.TraceId = _requestTraceAccessor.TraceId;
        entity.DeliveredAt = DateTimeProvider.Now;

        await _notifications.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var result = LocalizeNotification(_mapper.Map<NotificationDto>(entity));
        await PublishSignalRNotificationAsync(entity, cancellationToken);

        return ApiResponse<NotificationDto>.SuccessResult(result, _localizationService.GetLocalizedString("NotificationCreatedSuccessfully"));
    }

    public async Task<ApiResponse<IEnumerable<NotificationDto>>> CreateForUsersAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var recipients = dto.RecipientUserIds?.ToList() ?? new List<long>();
        if (dto.RecipientUserId.HasValue)
        {
            recipients.Add(dto.RecipientUserId.Value);
        }

        if (recipients.Count == 0)
        {
            var msg = _localizationService.GetLocalizedString("NotificationRecipientsRequired");
            return ApiResponse<IEnumerable<NotificationDto>>.ErrorResult(msg, msg, 400);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var entities = new List<Notification>();
            foreach (var userId in recipients.Distinct())
            {
                var entity = _mapper.Map<Notification>(dto);
                entity.RecipientUserId = userId;
                entity.TraceId = _requestTraceAccessor.TraceId;
                entity.DeliveredAt = DateTimeProvider.Now;
                entities.Add(entity);
            }

            await _notifications.AddRangeAsync(entities, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            foreach (var entity in entities)
            {
                await PublishSignalRNotificationAsync(entity, cancellationToken);
            }

            var dtos = LocalizeNotifications(_mapper.Map<List<NotificationDto>>(entities));
            return ApiResponse<IEnumerable<NotificationDto>>.SuccessResult(dtos, _localizationService.GetLocalizedString("NotificationBulkCreatedSuccessfully"));
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<PagedResponse<NotificationDto>>> GetPagedByRecipientUserIdAsync(long userId, PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        if (request.PageNumber < 1) request.PageNumber = 1;
        if (request.PageSize < 1) request.PageSize = 20;

        var baseQuery = _notifications.Query()
            .Where(x => x.RecipientUserId == userId && !x.IsDeleted)
            .ApplyFilters(request.Filters, request.FilterLogic);

        var unreadCount = await baseQuery.Where(x => !x.IsRead).CountAsync(cancellationToken);
        var items = await baseQuery
            .OrderBy(x => x.IsRead)
            .ThenByDescending(x => x.Id)
            .ApplyPagination(request.PageNumber, request.PageSize)
            .ToListAsync(cancellationToken);

        var dtos = LocalizeNotifications(_mapper.Map<List<NotificationDto>>(items));
        var result = new PagedResponse<NotificationDto>(dtos, unreadCount, request.PageNumber, request.PageSize);
        return ApiResponse<PagedResponse<NotificationDto>>.SuccessResult(result, _localizationService.GetLocalizedString("NotificationRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<bool>> MarkAsReadAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _notifications.Query(tracking: true).Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null || entity.IsDeleted)
        {
            var msg = _localizationService.GetLocalizedString("NotificationNotFound");
            return ApiResponse<bool>.ErrorResult(msg, msg, 404);
        }

        entity.IsRead = true;
        entity.ReadDate = DateTime.UtcNow;
        entity.UpdatedDate = DateTime.UtcNow;
        _notifications.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("NotificationMarkedReadSuccessfully"));
    }

    public async Task<ApiResponse<bool>> MarkAsReadBulkAsync(List<long> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null || ids.Count == 0)
        {
            return ApiResponse<bool>.ErrorResult(
                _localizationService.GetLocalizedString("InvalidModelState"),
                _localizationService.GetLocalizedString("NotificationIdsRequired"),
                400);
        }

        var entities = await _notifications.Query(tracking: true)
            .Where(x => ids.Contains(x.Id) && !x.IsDeleted)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            return ApiResponse<bool>.ErrorResult(
                _localizationService.GetLocalizedString("NotificationNotFound"),
                _localizationService.GetLocalizedString("NotificationValidRecordsNotFound"),
                404);
        }

        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.IsRead = true;
            entity.ReadDate = now;
            entity.UpdatedDate = now;
            _notifications.Update(entity);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("NotificationMarkedReadSuccessfully"));
    }

    public async Task<ApiResponse<bool>> MarkAllAsReadAsync(long userId, CancellationToken cancellationToken = default)
    {
        var entities = await _notifications.Query(tracking: true)
            .Where(x => x.RecipientUserId == userId && !x.IsDeleted && !x.IsRead)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("NotificationMarkedReadSuccessfully"));
        }

        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.IsRead = true;
            entity.ReadDate = now;
            entity.UpdatedDate = now;
            _notifications.Update(entity);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("NotificationMarkedReadSuccessfully"));
    }

    public List<Notification> CreateNotificationsForTerminalLines<TTerminalLine>(
        IEnumerable<TTerminalLine> terminalLines,
        string orderNumber,
        string entityType,
        string terminalActionCode,
        string titleLocalizationKey,
        string messageLocalizationKey) where TTerminalLine : class
    {
        var notifications = new List<Notification>();
        foreach (var terminalLine in terminalLines)
        {
            var terminalUserIdProp = typeof(TTerminalLine).GetProperty("TerminalUserId");
            if (terminalUserIdProp == null)
            {
                continue;
            }

            var terminalUserIdValue = terminalUserIdProp.GetValue(terminalLine);
            if (terminalUserIdValue is not long terminalUserId)
            {
                continue;
            }

            notifications.Add(new Notification
            {
                TraceId = _requestTraceAccessor.TraceId,
                Title = titleLocalizationKey,
                Message = messageLocalizationKey,
                TitleKey = titleLocalizationKey,
                MessageKey = messageLocalizationKey,
                Channel = NotificationChannel.Terminal,
                Severity = NotificationSeverity.Info,
                RecipientUserId = terminalUserId,
                RelatedEntityType = entityType,
                RelatedEntityId = long.TryParse(orderNumber, out var orderId) ? orderId : null,
                TerminalActionCode = terminalActionCode,
                DeliveredAt = DateTimeProvider.Now,
                IsDeleted = false,
                CreatedDate = DateTimeProvider.Now
            });
        }

        return notifications;
    }

    public async Task<List<Notification>> CreateAndAddNotificationsForTerminalLinesAsync<TTerminalLine>(
        IEnumerable<TTerminalLine> terminalLines,
        string orderNumber,
        string entityType,
        string terminalActionCode,
        string titleLocalizationKey,
        string messageLocalizationKey,
        CancellationToken cancellationToken = default) where TTerminalLine : class
    {
        var notifications = CreateNotificationsForTerminalLines(
            terminalLines,
            orderNumber,
            entityType,
            terminalActionCode,
            titleLocalizationKey,
            messageLocalizationKey);

        if (notifications.Count > 0)
        {
            await _notifications.AddRangeAsync(notifications, cancellationToken);
        }

        return notifications;
    }

    public async Task PublishSignalRNotificationsAsync(IEnumerable<Notification> notifications, CancellationToken cancellationToken = default)
    {
        foreach (var notification in notifications)
        {
            await PublishSignalRNotificationAsync(notification, cancellationToken);
        }
    }

    public async Task PublishSignalRNotificationsForCreatedNotificationsAsync(List<Notification> notifications, CancellationToken cancellationToken = default)
    {
        if (notifications.Count == 0)
        {
            return;
        }

        var recipientUserIds = notifications
            .Where(x => x.RecipientUserId.HasValue)
            .Select(x => x.RecipientUserId!.Value)
            .Distinct()
            .ToList();

        var relatedEntityIds = notifications
            .Where(x => x.RelatedEntityId.HasValue)
            .Select(x => x.RelatedEntityId!.Value)
            .Distinct()
            .ToList();

        if (recipientUserIds.Count == 0)
        {
            return;
        }

        var timeThreshold = DateTime.UtcNow.AddMinutes(-2);
        var savedNotifications = await _notifications.Query()
            .Where(n => !n.IsDeleted
                && n.DeliveredAt.HasValue
                && n.DeliveredAt.Value >= timeThreshold
                && n.RecipientUserId.HasValue
                && recipientUserIds.Contains(n.RecipientUserId.Value)
                && (relatedEntityIds.Count == 0 || (n.RelatedEntityId.HasValue && relatedEntityIds.Contains(n.RelatedEntityId.Value))))
            .ToListAsync(cancellationToken);

        foreach (var notification in savedNotifications)
        {
            await PublishSignalRNotificationAsync(notification, cancellationToken);
        }
    }

    private async Task PublishSignalRNotificationAsync(Notification entity, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = entity.Severity switch
        {
            NotificationSeverity.Info => "info",
            NotificationSeverity.Warning => "warning",
            NotificationSeverity.Error => "error",
            _ => "info"
        };

        var orderNumber = entity.RelatedEntityId?.ToString() ?? string.Empty;
        var localizedTitle = string.IsNullOrWhiteSpace(entity.TitleKey)
            ? entity.Title
            : _localizationService.GetLocalizedString(entity.TitleKey, orderNumber);
        var localizedMessage = string.IsNullOrWhiteSpace(entity.MessageKey)
            ? entity.Message
            : _localizationService.GetLocalizedString(entity.MessageKey, orderNumber);

        var payload = new
        {
            id = entity.Id,
            title = localizedTitle,
            message = localizedMessage,
            type,
            timestamp = DateTime.UtcNow,
            channel = entity.Channel,
            recipientUserId = entity.RecipientUserId,
            relatedEntityType = entity.RelatedEntityType,
            relatedEntityId = entity.RelatedEntityId
        };

        if (entity.RecipientUserId.HasValue)
        {
            _logger.LogInformation(
                "Notification published to user RecipientUserId={RecipientUserId} NotificationId={NotificationId} TraceId={TraceId} RelatedEntityType={RelatedEntityType} RelatedEntityId={RelatedEntityId}",
                entity.RecipientUserId.Value,
                entity.Id,
                entity.TraceId ?? _requestTraceAccessor.TraceId,
                entity.RelatedEntityType,
                entity.RelatedEntityId);
            await _notificationPublisher.PublishToUserAsync(entity.RecipientUserId.Value.ToString(), payload, cancellationToken);
        }
        else
        {
            _logger.LogInformation(
                "Notification published to all NotificationId={NotificationId} TraceId={TraceId} RelatedEntityType={RelatedEntityType} RelatedEntityId={RelatedEntityId}",
                entity.Id,
                entity.TraceId ?? _requestTraceAccessor.TraceId,
                entity.RelatedEntityType,
                entity.RelatedEntityId);
            await _notificationPublisher.PublishToAllAsync(payload, cancellationToken);
        }
    }

    private NotificationDto LocalizeNotification(NotificationDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.TitleKey))
        {
            dto.Title = _localizationService.GetLocalizedString(dto.TitleKey, dto.RelatedEntityId?.ToString() ?? string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(dto.MessageKey))
        {
            dto.Message = _localizationService.GetLocalizedString(dto.MessageKey, dto.RelatedEntityId?.ToString() ?? string.Empty);
        }

        return dto;
    }

    private List<NotificationDto> LocalizeNotifications(List<NotificationDto> dtos)
    {
        foreach (var dto in dtos)
        {
            LocalizeNotification(dto);
        }

        return dtos;
    }
}
