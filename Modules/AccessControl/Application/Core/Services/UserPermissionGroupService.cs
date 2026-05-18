using Microsoft.EntityFrameworkCore;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.AccessControl;
using Wms.Domain.Entities.Identity;
using Wms.WebApi.Telemetry;

namespace Wms.Application.AccessControl.Services;

public sealed class UserPermissionGroupService : IUserPermissionGroupService
{
    private readonly IRepository<User> _users;
    private readonly IRepository<UserPermissionGroup> _userPermissionGroups;
    private readonly IRepository<PermissionGroup> _permissionGroups;
    private readonly IRepository<UserAuthority> _userAuthorities;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IAuditLogWriter _auditLogWriter;

    public UserPermissionGroupService(
        IRepository<User> users,
        IRepository<UserPermissionGroup> userPermissionGroups,
        IRepository<PermissionGroup> permissionGroups,
        IRepository<UserAuthority> userAuthorities,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IAuditLogWriter auditLogWriter)
    {
        _users = users;
        _userPermissionGroups = userPermissionGroups;
        _permissionGroups = permissionGroups;
        _userAuthorities = userAuthorities;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<ApiResponse<UserPermissionGroupDto>> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            var nf = _localizationService.GetLocalizedString("AuthUserNotFound");
            return ApiResponse<UserPermissionGroupDto>.ErrorResult(nf, nf, 404);
        }

        var links = await _userPermissionGroups.Query()
            .Where(x => x.UserId == userId)
            .Include(x => x.PermissionGroup)
            .ToListAsync(cancellationToken);

        var dto = new UserPermissionGroupDto
        {
            UserId = userId,
            PermissionGroupIds = links.Select(x => x.PermissionGroupId).Distinct().OrderBy(x => x).ToList(),
            PermissionGroupNames = links.Where(x => x.PermissionGroup != null).Select(x => x.PermissionGroup.Name).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList()
        };

        return ApiResponse<UserPermissionGroupDto>.SuccessResult(dto, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<UserPermissionGroupDto>> SetUserGroupsAsync(long userId, SetUserPermissionGroupsDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            var nf = _localizationService.GetLocalizedString("AuthUserNotFound");
            return ApiResponse<UserPermissionGroupDto>.ErrorResult(nf, nf, 404);
        }

        var distinctGroupIds = dto.PermissionGroupIds.Distinct().ToList();
        if (distinctGroupIds.Count > 0)
        {
            var validCount = await _permissionGroups.Query().Where(x => distinctGroupIds.Contains(x.Id)).CountAsync(cancellationToken);
            if (validCount != distinctGroupIds.Count)
            {
                var msg = _localizationService.GetLocalizedString("ValidationError");
                return ApiResponse<UserPermissionGroupDto>.ErrorResult(msg, msg, 400);
            }

            var hasSystemAdminGroup = await _permissionGroups.Query().Where(x => x.IsSystemAdmin && distinctGroupIds.Contains(x.Id)).AnyAsync(cancellationToken);
            if (hasSystemAdminGroup)
            {
                var roleTitle = await _userAuthorities.Query().Where(x => x.Id == user.RoleId).Select(x => x.Title).FirstOrDefaultAsync(cancellationToken);
                var isAdminRole = !string.IsNullOrWhiteSpace(roleTitle) && roleTitle.Contains("admin", StringComparison.OrdinalIgnoreCase);
                if (!isAdminRole)
                {
                    var msg = _localizationService.GetLocalizedString("ValidationError");
                    return ApiResponse<UserPermissionGroupDto>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlSystemAdminGroupRequiresAdminRole"), 400);
                }
            }
        }

        var currentLinks = await _userPermissionGroups.Query(ignoreQueryFilters: true).Where(x => x.UserId == userId).ToListAsync(cancellationToken);
        var oldGroupIds = currentLinks.Where(x => !x.IsDeleted).Select(x => x.PermissionGroupId).Distinct().OrderBy(x => x).ToList();
        foreach (var link in currentLinks.Where(x => !x.IsDeleted && !distinctGroupIds.Contains(x.PermissionGroupId)))
        {
            await _userPermissionGroups.SoftDelete(link.Id, cancellationToken);
        }

        foreach (var groupId in distinctGroupIds)
        {
            var existing = currentLinks.FirstOrDefault(x => x.PermissionGroupId == groupId);
            if (existing == null)
            {
                await _userPermissionGroups.AddAsync(new UserPermissionGroup
                {
                    UserId = userId,
                    PermissionGroupId = groupId,
                    CreatedDate = DateTimeProvider.Now,
                    IsDeleted = false
                }, cancellationToken);
                continue;
            }

            if (existing.IsDeleted)
            {
                existing.IsDeleted = false;
                existing.DeletedDate = null;
                existing.DeletedBy = null;
                existing.UpdatedDate = DateTimeProvider.Now;
                _userPermissionGroups.Update(existing);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(UserPermissionGroup),
            EntityId = userId.ToString(),
            ActionType = "SetUserGroups",
            PerformedByUserId = userId,
            BranchCode = user.BranchCode,
            Result = RequestOutcome.Succeeded,
            Source = "UserPermissionGroupService.SetUserGroups",
            OldValues = new { PermissionGroupIds = oldGroupIds },
            NewValues = new { PermissionGroupIds = distinctGroupIds.OrderBy(x => x).ToList() },
            ChangedFields = new[] { "PermissionGroupIds" }
        }, cancellationToken);

        return await GetByUserIdAsync(userId, cancellationToken);
    }
}
