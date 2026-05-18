using Microsoft.EntityFrameworkCore;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;
using Wms.Domain.Entities.AccessControl;
using Wms.Domain.Entities.Identity;

namespace Wms.Application.AccessControl.Services;

public sealed class PermissionAccessService : IPermissionAccessService
{
    private readonly IRepository<User> _users;
    private readonly IRepository<UserPermissionGroup> _userPermissionGroups;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly ILocalizationService _localizationService;

    public PermissionAccessService(
        IRepository<User> users,
        IRepository<UserPermissionGroup> userPermissionGroups,
        ICurrentUserAccessor currentUserAccessor,
        ILocalizationService localizationService)
    {
        _users = users;
        _userPermissionGroups = userPermissionGroups;
        _currentUserAccessor = currentUserAccessor;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<MyPermissionsDto>> GetMyPermissionsAsync(string? platform = null, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserAccessor.UserId;
        if (!userId.HasValue)
        {
            var unauthorized = _localizationService.GetLocalizedString("UnauthorizedAccess");
            return ApiResponse<MyPermissionsDto>.ErrorResult(unauthorized, unauthorized, 401);
        }

        var user = await _users.Query()
            .Include(x => x.RoleNavigation)
            .Where(x => x.Id == userId.Value)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            var nf = _localizationService.GetLocalizedString("AuthUserNotFound");
            return ApiResponse<MyPermissionsDto>.ErrorResult(nf, nf, 404);
        }

        var userGroupLinks = await _userPermissionGroups.Query()
            .Where(x => x.UserId == userId.Value)
            .Include(x => x.PermissionGroup)
            .ThenInclude(x => x.GroupPermissions.Where(gp => !gp.IsDeleted))
            .ThenInclude(x => x.PermissionDefinition)
            .ToListAsync(cancellationToken);

        var roleTitle = user.RoleNavigation?.Title ?? "user";
        var isSystemAdmin = userGroupLinks.Any(x => x.PermissionGroup.IsSystemAdmin);
        if (!isSystemAdmin && userGroupLinks.Count == 0 && roleTitle.Equals("admin", StringComparison.OrdinalIgnoreCase))
        {
            isSystemAdmin = true;
        }

        var permissionCodes = isSystemAdmin
            ? new List<string>()
            : userGroupLinks.SelectMany(x => x.PermissionGroup.GroupPermissions)
                .Where(x =>
                    !x.IsDeleted
                    && x.PermissionDefinition != null
                    && !x.PermissionDefinition.IsDeleted
                    && x.PermissionDefinition.IsActive
                    && IsPermissionAvailableOnPlatform(x.PermissionDefinition, platform))
                .Select(x => x.PermissionDefinition.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();

        var dto = new MyPermissionsDto
        {
            UserId = userId.Value,
            RoleTitle = roleTitle,
            IsSystemAdmin = isSystemAdmin,
            Platform = NormalizePlatform(platform),
            PermissionGroups = userGroupLinks.Select(x => x.PermissionGroup.Name).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList(),
            PermissionCodes = permissionCodes
        };

        return ApiResponse<MyPermissionsDto>.SuccessResult(dto, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    private static bool IsPermissionAvailableOnPlatform(PermissionDefinition definition, string? platform)
    {
        var normalizedPlatform = NormalizePlatform(platform);
        return normalizedPlatform switch
        {
            PermissionPlatforms.Web => definition.AvailableOnWeb,
            PermissionPlatforms.Mobile => definition.AvailableOnMobile,
            _ => true
        };
    }

    private static string? NormalizePlatform(string? platform)
    {
        if (string.IsNullOrWhiteSpace(platform))
        {
            return null;
        }

        return platform.Trim().ToLowerInvariant();
    }
}
