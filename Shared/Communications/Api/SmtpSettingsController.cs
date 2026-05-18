using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.Communications.Dtos;
using Wms.Application.Communications.Services;

namespace Wms.WebApi.Controllers.Communications;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class SmtpSettingsController : ControllerBase
{
    private readonly ISmtpSettingsService _smtpSettingsService;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILocalizationService _localizationService;

    public SmtpSettingsController(
        ISmtpSettingsService smtpSettingsService,
        ICurrentUserAccessor currentUserAccessor,
        ILocalizationService localizationService)
    {
        _smtpSettingsService = smtpSettingsService;
        _currentUserAccessor = currentUserAccessor;
        _localizationService = localizationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SmtpSettingsDto>>> Get(CancellationToken cancellationToken = default)
    {
        var result = await _smtpSettingsService.GetAsync(cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<SmtpSettingsDto>>> Update([FromBody] UpdateSmtpSettingsDto dto, CancellationToken cancellationToken = default)
    {
        if (!_currentUserAccessor.UserId.HasValue)
        {
            var unauthorized = _localizationService.GetLocalizedString("Unauthorized");
            var result = ApiResponse<SmtpSettingsDto>.ErrorResult(unauthorized, unauthorized, 401);
            return StatusCode(result.StatusCode, result);
        }

        var response = await _smtpSettingsService.UpdateAsync(dto, _currentUserAccessor.UserId.Value, cancellationToken);
        return StatusCode(response.StatusCode, response);
    }
}
