using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.Common;
using Wms.Application.Communications.Dtos;
using Wms.Application.Communications.Services;

namespace Wms.WebApi.Controllers.Communications;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class MailController : ControllerBase
{
    private readonly IMailService _mailService;
    private readonly ICurrentUserAccessor _currentUserAccessor;

    public MailController(IMailService mailService, ICurrentUserAccessor currentUserAccessor)
    {
        _mailService = mailService;
        _currentUserAccessor = currentUserAccessor;
    }

    [HttpPost("send-test")]
    public async Task<ActionResult<ApiResponse<bool>>> SendTest([FromBody] SendTestMailDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _mailService.SendTestAsync(dto, _currentUserAccessor.UserId, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
