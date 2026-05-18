using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.AccessControl;
using Wms.Domain.Entities.Identity;
using WarehouseEntity = Wms.Domain.Entities.Warehouse.Warehouse;
using Wms.WebApi.Telemetry;

namespace Wms.Application.AccessControl.Services;

public sealed class WmsScopePolicyService : IWmsScopePolicyService
{
    private readonly IRepository<WmsScopePolicy> _scopePolicies;
    private readonly IRepository<UserWmsScopePolicy> _userAssignments;
    private readonly IRepository<User> _users;
    private readonly IRepository<WarehouseEntity> _warehouses;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;
    private readonly IAuditLogWriter _auditLogWriter;

    public WmsScopePolicyService(
        IRepository<WmsScopePolicy> scopePolicies,
        IRepository<UserWmsScopePolicy> userAssignments,
        IRepository<User> users,
        IRepository<WarehouseEntity> warehouses,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IMapper mapper,
        IAuditLogWriter auditLogWriter)
    {
        _scopePolicies = scopePolicies;
        _userAssignments = userAssignments;
        _users = users;
        _warehouses = warehouses;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _mapper = mapper;
        _auditLogWriter = auditLogWriter;
    }

    public async Task<ApiResponse<PagedResponse<WmsScopePolicyDto>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        request.Filters ??= new List<Filter>();
        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(WmsScopePolicy.Id) : request.SortBy;
        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = _scopePolicies.Query()
            .ApplySearch(request.Search, nameof(WmsScopePolicy.Code), nameof(WmsScopePolicy.Name), nameof(WmsScopePolicy.EntityType), nameof(WmsScopePolicy.Description))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(sortBy, desc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync(cancellationToken);
        var dtoItems = _mapper.Map<List<WmsScopePolicyDto>>(items);
        return ApiResponse<PagedResponse<WmsScopePolicyDto>>.SuccessResult(
            new PagedResponse<WmsScopePolicyDto>(dtoItems, totalCount, request.PageNumber, request.PageSize),
            _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<WmsScopePolicyDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _scopePolicies.Query().FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<WmsScopePolicyDto>.ErrorResult(msg, msg, 404);
        }

        return ApiResponse<WmsScopePolicyDto>.SuccessResult(_mapper.Map<WmsScopePolicyDto>(entity), _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<WmsScopePolicyDto>> CreateAsync(CreateWmsScopePolicyDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await ValidatePolicyAsync(dto.Code, dto.ScopeType, dto.EntityType, null, cancellationToken);
        if (validation != null)
        {
            return validation;
        }

        var entity = new WmsScopePolicy
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            EntityType = dto.EntityType.Trim(),
            Description = dto.Description?.Trim(),
            ScopeType = dto.ScopeType.Trim(),
            IncludeSelf = dto.IncludeSelf,
            IsActive = dto.IsActive,
            CreatedDate = DateTimeProvider.Now,
            IsDeleted = false
        };

        await _scopePolicies.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(WmsScopePolicy),
            EntityId = entity.Id.ToString(),
            ActionType = "Create",
            Result = RequestOutcome.Succeeded,
            Source = "WmsScopePolicyService.Create",
            NewValues = new { entity.Code, entity.Name, entity.EntityType, entity.ScopeType, entity.IncludeSelf, entity.IsActive },
            ChangedFields = new[] { "Code", "Name", "EntityType", "ScopeType", "IncludeSelf", "IsActive" }
        }, cancellationToken);

        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ApiResponse<WmsScopePolicyDto>> UpdateAsync(long id, UpdateWmsScopePolicyDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _scopePolicies.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity == null)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<WmsScopePolicyDto>.ErrorResult(msg, msg, 404);
        }

        var nextCode = !string.IsNullOrWhiteSpace(dto.Code) ? dto.Code.Trim() : entity.Code;
        var nextScopeType = !string.IsNullOrWhiteSpace(dto.ScopeType) ? dto.ScopeType.Trim() : entity.ScopeType;
        var nextEntityType = !string.IsNullOrWhiteSpace(dto.EntityType) ? dto.EntityType.Trim() : entity.EntityType;

        var validation = await ValidatePolicyAsync(nextCode, nextScopeType, nextEntityType, id, cancellationToken);
        if (validation != null)
        {
            return validation;
        }

        var oldValues = new { entity.Code, entity.Name, entity.EntityType, entity.Description, entity.ScopeType, entity.IncludeSelf, entity.IsActive };
        var changedFields = new List<string>();

        if (!string.IsNullOrWhiteSpace(dto.Code) && !string.Equals(entity.Code, dto.Code.Trim(), StringComparison.Ordinal))
        {
            entity.Code = dto.Code.Trim();
            changedFields.Add(nameof(WmsScopePolicy.Code));
        }
        if (!string.IsNullOrWhiteSpace(dto.Name) && !string.Equals(entity.Name, dto.Name.Trim(), StringComparison.Ordinal))
        {
            entity.Name = dto.Name.Trim();
            changedFields.Add(nameof(WmsScopePolicy.Name));
        }
        if (dto.EntityType != null && !string.Equals(entity.EntityType, dto.EntityType.Trim(), StringComparison.Ordinal))
        {
            entity.EntityType = dto.EntityType.Trim();
            changedFields.Add(nameof(WmsScopePolicy.EntityType));
        }
        if (dto.Description != null && !string.Equals(entity.Description ?? string.Empty, dto.Description.Trim(), StringComparison.Ordinal))
        {
            entity.Description = dto.Description.Trim();
            changedFields.Add(nameof(WmsScopePolicy.Description));
        }
        if (dto.ScopeType != null && !string.Equals(entity.ScopeType, dto.ScopeType.Trim(), StringComparison.Ordinal))
        {
            entity.ScopeType = dto.ScopeType.Trim();
            changedFields.Add(nameof(WmsScopePolicy.ScopeType));
        }
        if (dto.IncludeSelf.HasValue && entity.IncludeSelf != dto.IncludeSelf.Value)
        {
            entity.IncludeSelf = dto.IncludeSelf.Value;
            changedFields.Add(nameof(WmsScopePolicy.IncludeSelf));
        }
        if (dto.IsActive.HasValue && entity.IsActive != dto.IsActive.Value)
        {
            entity.IsActive = dto.IsActive.Value;
            changedFields.Add(nameof(WmsScopePolicy.IsActive));
        }

        entity.UpdatedDate = DateTimeProvider.Now;
        _scopePolicies.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(WmsScopePolicy),
            EntityId = entity.Id.ToString(),
            ActionType = "Update",
            Result = RequestOutcome.Succeeded,
            Source = "WmsScopePolicyService.Update",
            OldValues = oldValues,
            NewValues = new { entity.Code, entity.Name, entity.EntityType, entity.Description, entity.ScopeType, entity.IncludeSelf, entity.IsActive },
            ChangedFields = changedFields
        }, cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var exists = await _scopePolicies.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<bool>.ErrorResult(msg, msg, 404);
        }

        await _scopePolicies.SoftDelete(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(WmsScopePolicy),
            EntityId = id.ToString(),
            ActionType = "SoftDelete",
            Result = RequestOutcome.Succeeded,
            Source = "WmsScopePolicyService.SoftDelete",
            ChangedFields = new[] { "IsDeleted" }
        }, cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<List<UserWmsScopePolicyAssignmentDto>>> GetAssignmentsByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var items = await _userAssignments.Query()
            .Where(x => x.UserId == userId)
            .Include(x => x.WmsScopePolicy)
            .OrderBy(x => x.WmsScopePolicy.EntityType)
            .ThenBy(x => x.WmsScopePolicy.Code)
            .ToListAsync(cancellationToken);

        return ApiResponse<List<UserWmsScopePolicyAssignmentDto>>.SuccessResult(items.Select(MapAssignment).ToList(), _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<List<UserWmsScopePolicyAssignmentDto>>> SetAssignmentsAsync(long userId, SetUserWmsScopePoliciesDto dto, CancellationToken cancellationToken = default)
    {
        var userExists = await _users.Query().AnyAsync(x => x.Id == userId, cancellationToken);
        if (!userExists)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<List<UserWmsScopePolicyAssignmentDto>>.ErrorResult(msg, msg, 404);
        }

        dto ??= new SetUserWmsScopePoliciesDto();
        dto.Items ??= new List<UserWmsScopePolicyAssignmentInputDto>();

        var policyIds = dto.Items.Select(x => x.WmsScopePolicyId).Distinct().ToList();
        if (policyIds.Count > 0)
        {
            var policies = await _scopePolicies.Query().Where(x => policyIds.Contains(x.Id)).ToListAsync(cancellationToken);
            if (policies.Count != policyIds.Count)
            {
                var msg = _localizationService.GetLocalizedString("ValidationError");
                return ApiResponse<List<UserWmsScopePolicyAssignmentDto>>.ErrorResult(msg, msg, 400);
            }

            foreach (var item in dto.Items)
            {
                var policy = policies.First(x => x.Id == item.WmsScopePolicyId);
                var assignmentValidation = await ValidateAssignmentAsync(policy, item, cancellationToken);
                if (assignmentValidation != null)
                {
                    return assignmentValidation;
                }
            }
        }

        var existing = await _userAssignments.Query(ignoreQueryFilters: true, tracking: true)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var assignment in existing.Where(x => !x.IsDeleted))
        {
            await _userAssignments.SoftDelete(assignment.Id, cancellationToken);
        }

        foreach (var item in dto.Items)
        {
            await _userAssignments.AddAsync(new UserWmsScopePolicy
            {
                UserId = userId,
                WmsScopePolicyId = item.WmsScopePolicyId,
                BranchCode = item.BranchCode?.Trim() ?? string.Empty,
                WarehouseId = item.WarehouseId,
                CreatedDate = DateTimeProvider.Now,
                IsDeleted = false
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditLogWriter.WriteAsync(new AuditLogWriteRequest
        {
            EntityType = nameof(UserWmsScopePolicy),
            EntityId = userId.ToString(),
            ActionType = "SetAssignments",
            Result = RequestOutcome.Succeeded,
            Source = "WmsScopePolicyService.SetAssignments",
            NewValues = dto.Items.Select(x => new { x.WmsScopePolicyId, BranchCode = x.BranchCode?.Trim(), x.WarehouseId }).ToList(),
            ChangedFields = new[] { "Assignments" }
        }, cancellationToken);

        return await GetAssignmentsByUserIdAsync(userId, cancellationToken);
    }

    public async Task<ApiResponse<WmsScopePolicyResolutionDto>> ResolveForUserAsync(long userId, string entityType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entityType))
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<WmsScopePolicyResolutionDto>.ErrorResult(msg, msg, 400);
        }

        var items = await _userAssignments.Query()
            .Where(x => x.UserId == userId
                        && x.WmsScopePolicy.IsActive
                        && x.WmsScopePolicy.EntityType == entityType)
            .Include(x => x.WmsScopePolicy)
            .ToListAsync(cancellationToken);

        if (items.Count == 0)
        {
            return ApiResponse<WmsScopePolicyResolutionDto>.SuccessResult(new WmsScopePolicyResolutionDto
            {
                UserId = userId,
                EntityType = entityType,
                HasExplicitPolicy = false,
                IsUnrestricted = true,
                IncludeSelf = true
            }, _localizationService.GetLocalizedString("OperationSuccessful"));
        }

        var resolution = new WmsScopePolicyResolutionDto
        {
            UserId = userId,
            EntityType = entityType,
            HasExplicitPolicy = true,
            IsUnrestricted = items.Any(x => string.Equals(x.WmsScopePolicy.ScopeType, WmsScopePolicyScopeTypes.Unrestricted, StringComparison.OrdinalIgnoreCase)),
            RequiresAssignedRecords = items.Any(x => string.Equals(x.WmsScopePolicy.ScopeType, WmsScopePolicyScopeTypes.AssignedOnly, StringComparison.OrdinalIgnoreCase)),
            IncludeSelf = items.Any(x => x.WmsScopePolicy.IncludeSelf),
            BranchCodes = items.Select(x => x.BranchCode).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList(),
            WarehouseIds = items.Where(x => x.WarehouseId.HasValue).Select(x => x.WarehouseId!.Value).Distinct().OrderBy(x => x).ToList(),
            ScopeTypes = items.Select(x => x.WmsScopePolicy.ScopeType).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(x => x).ToList()
        };

        return ApiResponse<WmsScopePolicyResolutionDto>.SuccessResult(resolution, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    private async Task<ApiResponse<WmsScopePolicyDto>?> ValidatePolicyAsync(string code, string scopeType, string entityType, long? currentId, CancellationToken cancellationToken)
    {
        if (!WmsScopePolicyScopeTypes.All.Contains(scopeType))
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<WmsScopePolicyDto>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlInvalidScopeType"), 400);
        }

        var duplicate = await _scopePolicies.Query()
            .Where(x => x.Code == code && (!currentId.HasValue || x.Id != currentId.Value))
            .AnyAsync(cancellationToken);

        if (duplicate)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<WmsScopePolicyDto>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlScopePolicyCodeAlreadyExists"), 400);
        }

        if (string.IsNullOrWhiteSpace(entityType))
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<WmsScopePolicyDto>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlEntityTypeRequired"), 400);
        }

        return null;
    }

    private async Task<ApiResponse<List<UserWmsScopePolicyAssignmentDto>>?> ValidateAssignmentAsync(
        WmsScopePolicy policy,
        UserWmsScopePolicyAssignmentInputDto input,
        CancellationToken cancellationToken)
    {
        var normalizedBranchCode = input.BranchCode?.Trim();

        if (string.Equals(policy.ScopeType, WmsScopePolicyScopeTypes.Branch, StringComparison.OrdinalIgnoreCase)
            || string.Equals(policy.ScopeType, WmsScopePolicyScopeTypes.BranchAndWarehouse, StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(normalizedBranchCode))
            {
                var msg = _localizationService.GetLocalizedString("ValidationError");
                return ApiResponse<List<UserWmsScopePolicyAssignmentDto>>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlScopePolicyBranchCodeRequired"), 400);
            }
        }

        if (string.Equals(policy.ScopeType, WmsScopePolicyScopeTypes.Warehouse, StringComparison.OrdinalIgnoreCase)
            || string.Equals(policy.ScopeType, WmsScopePolicyScopeTypes.BranchAndWarehouse, StringComparison.OrdinalIgnoreCase))
        {
            if (!input.WarehouseId.HasValue)
            {
                var msg = _localizationService.GetLocalizedString("ValidationError");
                return ApiResponse<List<UserWmsScopePolicyAssignmentDto>>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlScopePolicyWarehouseRequired"), 400);
            }

            var warehouseExists = await _warehouses.Query().AnyAsync(x => x.Id == input.WarehouseId.Value, cancellationToken);
            if (!warehouseExists)
            {
                var msg = _localizationService.GetLocalizedString("ValidationError");
                return ApiResponse<List<UserWmsScopePolicyAssignmentDto>>.ErrorResult(msg, _localizationService.GetLocalizedString("AccessControlScopePolicyWarehouseNotFound"), 400);
            }
        }

        return null;
    }

    private static UserWmsScopePolicyAssignmentDto MapAssignment(UserWmsScopePolicy entity)
        => new()
        {
            Id = entity.Id,
            CreatedDate = entity.CreatedDate,
            UpdatedDate = entity.UpdatedDate,
            DeletedDate = entity.DeletedDate,
            IsDeleted = entity.IsDeleted,
            UserId = entity.UserId,
            WmsScopePolicyId = entity.WmsScopePolicyId,
            PolicyCode = entity.WmsScopePolicy.Code,
            PolicyName = entity.WmsScopePolicy.Name,
            EntityType = entity.WmsScopePolicy.EntityType,
            ScopeType = entity.WmsScopePolicy.ScopeType,
            BranchCode = entity.BranchCode,
            WarehouseId = entity.WarehouseId
        };
}
