using Wms.Application.Common;
using Wms.Domain.Entities.Identity;

namespace Wms.Application.Identity.Services;

public interface IJwtService
{
    ApiResponse<string> GenerateToken(User user, IReadOnlyCollection<string>? permissions = null, bool isSystemAdmin = false);
}
