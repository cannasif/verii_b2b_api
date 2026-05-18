using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.Communications.Dtos;
using Wms.Application.Communications.Services;

namespace Wms.WebApi.Controllers.Communications;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public NotificationController(
        INotificationService notificationService,
        ILocalizationService localizationService,
        ICurrentUserAccessor currentUserAccessor)
    {
        _notificationService = notificationService;
        _localizationService = localizationService;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpPost("user/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<NotificationDto>>>> GetPagedByCurrentUserId([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.UserId.HasValue)
        {
            return StatusCode(401, ApiResponse<PagedResponse<NotificationDto>>.ErrorResult(
                _localizationService.GetLocalizedString("Unauthorized"),
                _localizationService.GetLocalizedString("CurrentUserIdNotFoundInToken"),
                401));
        }

        var result = await _notificationService.GetPagedByRecipientUserIdAsync(_currentUserAccessor.UserId.Value, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("user/{userId:long}/paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<NotificationDto>>>> GetPagedByUserId(long userId, [FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.GetPagedByRecipientUserIdAsync(userId, request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("read/all")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead(CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.UserId.HasValue)
        {
            return StatusCode(401, ApiResponse<bool>.ErrorResult(
                _localizationService.GetLocalizedString("Unauthorized"),
                _localizationService.GetLocalizedString("CurrentUserIdNotFoundInToken"),
                401));
        }

        var result = await _notificationService.MarkAllAsReadAsync(_currentUserAccessor.UserId.Value, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("{id:long}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(long id, CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut("read/bulk")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsReadBulk([FromBody] List<long> ids, CancellationToken cancellationToken = default)
    {
        if (ids == null || ids.Count == 0)
        {
            return StatusCode(400, ApiResponse<bool>.ErrorResult(
                _localizationService.GetLocalizedString("InvalidModelState"),
                _localizationService.GetLocalizedString("NotificationIdsRequired"),
                400));
        }

        var result = await _notificationService.MarkAsReadBulkAsync(ids, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> Create([FromBody] CreateNotificationDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _notificationService.CreateAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
