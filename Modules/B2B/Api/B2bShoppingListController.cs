using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wms.Application.B2B.Dtos;
using Wms.Application.B2B.Services;
using Wms.Application.Common;

namespace Wms.WebApi.Controllers.B2B;

[ApiController]
[Route("api/b2b/shopping-lists")]
[Authorize]
public sealed class B2bShoppingListController : ControllerBase
{
    private readonly IB2bAccountService _service;

    public B2bShoppingListController(IB2bAccountService service)
    {
        _service = service;
    }

    [HttpPost("paged")]
    public async Task<ActionResult<ApiResponse<PagedResponse<ShoppingListDto>>>> GetPaged([FromBody] PagedRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _service.GetShoppingListsAsync(request, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ShoppingListDto>>> Create([FromBody] CreateShoppingListDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _service.CreateShoppingListAsync(dto, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
