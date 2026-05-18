using Wms.Application.Common;

namespace Wms.Infrastructure.Services.Security;

public sealed class CurrentUserServiceAdapter : ICurrentUserService
{
    private readonly ICurrentUserAccessor _accessor;

    public CurrentUserServiceAdapter(ICurrentUserAccessor accessor)
    {
        _accessor = accessor;
    }

    public long? UserId => _accessor.UserId;
    public string? UserEmail => _accessor.UserEmail;
    public string? BranchCode => _accessor.BranchCode;
}
