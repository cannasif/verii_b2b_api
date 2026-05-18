using Microsoft.EntityFrameworkCore;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.AccessControl;
using Wms.WebApi.Telemetry;

namespace Wms.Application.AccessControl.Services;

public sealed class PermissionGroupService : IPermissionGroupService
{
    private readonly IRepository<PermissionGroup> _permissionGroups;
    private readonly IRepository<PermissionDefinition> _permissionDefinitions;
    private readonly IRepository<PermissionGroupPermission> _groupPermissions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IAuditLogWriter _auditLogWriter;

    public PermissionGroupService(
        IRepository<PermissionGroup> permissionGroups,
        IRepository<PermissionDefinition> permissionDefinitions,
        IRepository<PermissionGroupPermission> groupPermissions,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IAuditLogWriter auditLogWriter)
    {
        _permissionGroups = permissionGroups;
        _permissionDefinitions = permissionDefinitions;
        _groupPermissions = groupPermissions;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<ApiResponse<PagedResponse<PermissionGroupDto>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        request.Filters ??= new List<Filter>();
        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(PermissionGroup.Id) : request.SortBy;
        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = _permissionGroups.Query()
            .Include(x => x.GroupPermissions.Where(gp => !gp.IsDeleted))
            .ThenInclude(x => x.PermissionDefinition)
            .ApplySearch(request.Search, nameof(PermissionGroup.Name), nameof(PermissionGroup.Description))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(sortBy, desc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync(cancellationToken);
        var dtos = items.Select(MapToDto).ToList();
        return ApiResponse<PagedResponse<PermissionGroupDto>>.SuccessResult(new PagedResponse<PermissionGroupDto>(dtos, totalCount, request.PageNumber, request.PageSize), _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<PermissionGroupDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _permissionGroups.Query()
            .Include(x => x.GroupPermissions.Where(gp => !gp.IsDeleted))
            .ThenInclude(x => x.PermissionDefinition)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<PermissionGroupDto>.ErrorResult(msg, msg, 404);
        }

        return ApiResponse<PermissionGroupDto>.SuccessResult(MapToDto(entity), _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<PermissionGroupDto>> CreateAsync(CreatePermissionGroupDto dto, CancellationToken cancellationToken = default)
    {
        var duplicate = await _permissionGroups.Query().Where(x => x.Name == dto.Name).AnyAsync(cancellationToken);
        if (duplicate)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<PermissionGroupDto>.ErrorResult(msg, msg, 400);
        }

        var entity = new PermissionGroup
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsSystemAdmin = dto.IsSystemAdmin,
            IsActive = dto.IsActive,
            CreatedDate = DateTimeProvider.Now,
            IsDeleted = false
        };

        await _permissionGroups.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (dto.PermissionDefinitionIds.Count > 0)
        {
            var result = await SetPermissionsInternalAsync(entity.Id, dto.PermissionDefinitionIds, cancellationToken);
            if (!result.Success)
            {
                return ApiResponse<PermissionGroupDto>.ErrorResult(result.Message, result.ExceptionMessage, result.StatusCode);
            }
        }

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(PermissionGroup),
            EntityId = entity.Id.ToString(),
            ActionType = "Create",
            Result = RequestOutcome.Succeeded,
            Source = "PermissionGroupService.Create",
            NewValues = new
            {
                entity.Id,
                entity.Name,
                entity.Description,
                entity.IsSystemAdmin,
                entity.IsActive,
                PermissionDefinitionIds = dto.PermissionDefinitionIds.Distinct().OrderBy(x => x).ToList()
            },
            ChangedFields = new[] { "Name", "Description", "IsSystemAdmin", "IsActive", "PermissionDefinitionIds" }
        }, cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ApiResponse<PermissionGroupDto>> UpdateAsync(long id, UpdatePermissionGroupDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _permissionGroups.Query(tracking: true).Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<PermissionGroupDto>.ErrorResult(msg, msg, 404);
        }

        var oldValues = new
        {
            entity.Id,
            entity.Name,
            entity.Description,
            entity.IsSystemAdmin,
            entity.IsActive
        };
        var changedFields = new List<string>();

        if (entity.IsSystemAdmin)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<PermissionGroupDto>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlSystemAdminGroupCannotBeModified"), 403);
        }

        if (!string.IsNullOrWhiteSpace(dto.Name) && !dto.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await _permissionGroups.Query().Where(x => x.Id != id && x.Name == dto.Name).AnyAsync(cancellationToken);
            if (duplicate)
            {
                var msg = _localizationService.GetLocalizedString("ValidationError");
                return ApiResponse<PermissionGroupDto>.ErrorResult(msg, msg, 400);
            }
            entity.Name = dto.Name.Trim();
            changedFields.Add(nameof(PermissionGroup.Name));
        }

        if (dto.Description != null)
        {
            entity.Description = dto.Description.Trim();
            changedFields.Add(nameof(PermissionGroup.Description));
        }

        if (dto.IsSystemAdmin.HasValue)
        {
            entity.IsSystemAdmin = dto.IsSystemAdmin.Value;
            changedFields.Add(nameof(PermissionGroup.IsSystemAdmin));
        }

        if (dto.IsActive.HasValue)
        {
            entity.IsActive = dto.IsActive.Value;
            changedFields.Add(nameof(PermissionGroup.IsActive));
        }

        entity.UpdatedDate = DateTimeProvider.Now;
        _permissionGroups.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(PermissionGroup),
            EntityId = entity.Id.ToString(),
            ActionType = "Update",
            Result = RequestOutcome.Succeeded,
            Source = "PermissionGroupService.Update",
            OldValues = oldValues,
            NewValues = new
            {
                entity.Id,
                entity.Name,
                entity.Description,
                entity.IsSystemAdmin,
                entity.IsActive
            },
            ChangedFields = changedFields
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<PermissionGroupDto>> SetPermissionsAsync(long id, SetPermissionGroupPermissionsDto dto, CancellationToken cancellationToken = default)
    {
        var group = await _permissionGroups.Query().Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (group != null && group.IsSystemAdmin)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<PermissionGroupDto>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlSystemAdminGroupCannotBeModified"), 403);
        }

        var result = await SetPermissionsInternalAsync(id, dto.PermissionDefinitionIds, cancellationToken);
        if (!result.Success)
        {
            return ApiResponse<PermissionGroupDto>.ErrorResult(result.Message, result.ExceptionMessage, result.StatusCode);
        }

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(PermissionGroup),
            EntityId = id.ToString(),
            ActionType = "SetPermissions",
            Result = RequestOutcome.Succeeded,
            Source = "PermissionGroupService.SetPermissions",
            NewValues = new
            {
                PermissionDefinitionIds = dto.PermissionDefinitionIds.Distinct().OrderBy(x => x).ToList()
            },
            ChangedFields = new[] { "PermissionDefinitionIds" }
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _permissionGroups.Query().Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<bool>.ErrorResult(msg, msg, 404);
        }

        if (entity.IsSystemAdmin)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<bool>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlSystemAdminGroupCannotBeDeleted"), 403);
        }

        await _permissionGroups.SoftDelete(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(PermissionGroup),
            EntityId = id.ToString(),
            ActionType = "SoftDelete",
            Result = RequestOutcome.Succeeded,
            Source = "PermissionGroupService.SoftDelete",
            ChangedFields = new[] { "IsDeleted" }
        }, cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    private async Task<ApiResponse<bool>> SetPermissionsInternalAsync(long groupId, List<long> permissionIds, CancellationToken cancellationToken)
    {
        var groupExists = await _permissionGroups.ExistsAsync(groupId, cancellationToken);
        if (!groupExists)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<bool>.ErrorResult(msg, msg, 404);
        }

        var distinctIds = permissionIds.Distinct().ToList();
        if (distinctIds.Count > 0)
        {
            var validCount = await _permissionDefinitions.Query().Where(x => distinctIds.Contains(x.Id)).CountAsync(cancellationToken);
            if (validCount != distinctIds.Count)
            {
                var msg = _localizationService.GetLocalizedString("ValidationError");
                return ApiResponse<bool>.ErrorResult(msg, msg, 400);
            }
        }

        var currentLinks = await _groupPermissions.Query(ignoreQueryFilters: true).Where(x => x.PermissionGroupId == groupId).ToListAsync(cancellationToken);
        foreach (var link in currentLinks.Where(x => !x.IsDeleted && !distinctIds.Contains(x.PermissionDefinitionId)))
        {
            await _groupPermissions.SoftDelete(link.Id, cancellationToken);
        }

        foreach (var permissionId in distinctIds)
        {
            var existing = currentLinks.FirstOrDefault(x => x.PermissionDefinitionId == permissionId);
            if (existing == null)
            {
                await _groupPermissions.AddAsync(new PermissionGroupPermission
                {
                    PermissionGroupId = groupId,
                    PermissionDefinitionId = permissionId,
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
                _groupPermissions.Update(existing);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    private static PermissionGroupDto MapToDto(PermissionGroup entity)
    {
        var links = entity.GroupPermissions.Where(x => !x.IsDeleted && x.PermissionDefinition != null && !x.PermissionDefinition.IsDeleted && x.PermissionDefinition.IsActive).ToList();
        return new PermissionGroupDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsSystemAdmin = entity.IsSystemAdmin,
            IsActive = entity.IsActive,
            PermissionDefinitionIds = links.Select(x => x.PermissionDefinitionId).Distinct().OrderBy(x => x).ToList(),
            PermissionCodes = links.Select(x => x.PermissionDefinition.Code).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList(),
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate,
            DeletedDate = entity.DeletedDate,
            IsDeleted = entity.IsDeleted,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            DeletedBy = entity.DeletedBy
        };
    }
}
