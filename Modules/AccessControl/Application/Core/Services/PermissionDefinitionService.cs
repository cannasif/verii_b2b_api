using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.AccessControl.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.AccessControl;

namespace Wms.Application.AccessControl.Services;

public sealed class PermissionDefinitionService : IPermissionDefinitionService
{
    private readonly IRepository<PermissionDefinition> _permissionDefinitions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public PermissionDefinitionService(
        IRepository<PermissionDefinition> permissionDefinitions,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IMapper mapper)
    {
        _permissionDefinitions = permissionDefinitions;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<PagedResponse<PermissionDefinitionDto>>> GetAllAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        request.Filters ??= new List<Filter>();

        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? nameof(PermissionDefinition.Id) : request.SortBy;
        var desc = string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        var query = _permissionDefinitions.Query()
            .ApplySearch(request.Search, nameof(PermissionDefinition.Code), nameof(PermissionDefinition.Name), nameof(PermissionDefinition.Description))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(sortBy, desc);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync(cancellationToken);
        var dtoItems = _mapper.Map<List<PermissionDefinitionDto>>(items);

        return ApiResponse<PagedResponse<PermissionDefinitionDto>>.SuccessResult(
            new PagedResponse<PermissionDefinitionDto>(dtoItems, totalCount, request.PageNumber, request.PageSize),
            _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<PermissionDefinitionDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _permissionDefinitions.Query()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<PermissionDefinitionDto>.ErrorResult(msg, msg, 404);
        }

        return ApiResponse<PermissionDefinitionDto>.SuccessResult(_mapper.Map<PermissionDefinitionDto>(entity), _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<PermissionDefinitionDto>> CreateAsync(CreatePermissionDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        var exists = await _permissionDefinitions.Query().Where(x => x.Code == dto.Code).AnyAsync(cancellationToken);
        if (exists)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<PermissionDefinitionDto>.ErrorResult(msg, msg, 400);
        }

        var entity = new PermissionDefinition
        {
            Code = dto.Code.Trim(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            IsActive = dto.IsActive,
            AvailableOnWeb = dto.AvailableOnWeb,
            AvailableOnMobile = dto.AvailableOnMobile,
            CreatedDate = DateTimeProvider.Now,
            IsDeleted = false
        };

        await _permissionDefinitions.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(entity.Id, cancellationToken);
    }

    public async Task<ApiResponse<PermissionDefinitionDto>> UpdateAsync(long id, UpdatePermissionDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _permissionDefinitions.Query(tracking: true).Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<PermissionDefinitionDto>.ErrorResult(msg, msg, 404);
        }

        if (!string.IsNullOrWhiteSpace(dto.Code) && !dto.Code.Equals(entity.Code, StringComparison.OrdinalIgnoreCase))
        {
            var duplicate = await _permissionDefinitions.Query().Where(x => x.Id != id && x.Code == dto.Code).AnyAsync(cancellationToken);
            if (duplicate)
            {
                var msg = _localizationService.GetLocalizedString("ValidationError");
                return ApiResponse<PermissionDefinitionDto>.ErrorResult(msg, msg, 400);
            }

            entity.Code = dto.Code.Trim();
        }

        if (!string.IsNullOrWhiteSpace(dto.Name))
        {
            entity.Name = dto.Name.Trim();
        }

        if (dto.Description != null)
        {
            entity.Description = dto.Description.Trim();
        }

        if (dto.IsActive.HasValue)
        {
            entity.IsActive = dto.IsActive.Value;
        }

        if (dto.AvailableOnWeb.HasValue)
        {
            entity.AvailableOnWeb = dto.AvailableOnWeb.Value;
        }

        if (dto.AvailableOnMobile.HasValue)
        {
            entity.AvailableOnMobile = dto.AvailableOnMobile.Value;
        }

        entity.UpdatedDate = DateTimeProvider.Now;
        _permissionDefinitions.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var exists = await _permissionDefinitions.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            var msg = _localizationService.GetLocalizedString("ValidationError");
            return ApiResponse<bool>.ErrorResult(msg, msg, 404);
        }

        await _permissionDefinitions.SoftDelete(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public async Task<ApiResponse<PermissionDefinitionSyncResultDto>> SyncAsync(SyncPermissionDefinitionsDto dto, CancellationToken cancellationToken = default)
    {
        dto ??= new SyncPermissionDefinitionsDto();
        dto.Items ??= new List<SyncPermissionDefinitionItemDto>();

        var normalized = dto.Items
            .Where(x => !string.IsNullOrWhiteSpace(x.Code))
            .Select(x => new SyncPermissionDefinitionItemDto
            {
                Code = x.Code.Trim(),
                Name = string.IsNullOrWhiteSpace(x.Name) ? null : x.Name.Trim(),
                Description = x.Description == null ? null : x.Description.Trim(),
                IsActive = x.IsActive
            })
            .ToList();

        var distinctCodes = normalized.Select(x => x.Code).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (distinctCodes.Count == 0)
        {
            return ApiResponse<PermissionDefinitionSyncResultDto>.SuccessResult(new PermissionDefinitionSyncResultDto(), _localizationService.GetLocalizedString("OperationSuccessful"));
        }

        var existingAll = await _permissionDefinitions.Query(ignoreQueryFilters: true)
            .Where(x => distinctCodes.Contains(x.Code))
            .ToListAsync(cancellationToken);

        var existingByCode = existingAll.ToDictionary(x => x.Code, x => x, StringComparer.OrdinalIgnoreCase);
        var result = new PermissionDefinitionSyncResultDto();

        foreach (var item in normalized.GroupBy(x => x.Code, StringComparer.OrdinalIgnoreCase).Select(x => x.First()))
        {
            if (existingByCode.TryGetValue(item.Code, out var existing))
            {
                var changed = false;

                if (dto.ReactivateSoftDeleted && existing.IsDeleted)
                {
                    existing.IsDeleted = false;
                    existing.DeletedBy = null;
                    existing.DeletedDate = null;
                    result.ReactivatedCount++;
                    changed = true;
                }

                if (dto.UpdateExistingNames && !string.IsNullOrWhiteSpace(item.Name) && !string.Equals(existing.Name, item.Name, StringComparison.Ordinal))
                {
                    existing.Name = item.Name;
                    changed = true;
                }

                if (dto.UpdateExistingDescriptions && item.Description != null && !string.Equals(existing.Description, item.Description, StringComparison.Ordinal))
                {
                    existing.Description = item.Description;
                    changed = true;
                }

                if (dto.UpdateExistingIsActive && existing.IsActive != item.IsActive)
                {
                    existing.IsActive = item.IsActive;
                    changed = true;
                }

                if (existing.AvailableOnWeb != item.AvailableOnWeb)
                {
                    existing.AvailableOnWeb = item.AvailableOnWeb;
                    changed = true;
                }

                if (existing.AvailableOnMobile != item.AvailableOnMobile)
                {
                    existing.AvailableOnMobile = item.AvailableOnMobile;
                    changed = true;
                }

                if (changed)
                {
                    existing.UpdatedDate = DateTimeProvider.Now;
                    _permissionDefinitions.Update(existing);
                    result.UpdatedCount++;
                }
            }
            else
            {
                await _permissionDefinitions.AddAsync(new PermissionDefinition
                {
                    Code = item.Code,
                    Name = !string.IsNullOrWhiteSpace(item.Name) ? item.Name : item.Code,
                    Description = item.Description,
                    IsActive = item.IsActive,
                    AvailableOnWeb = item.AvailableOnWeb,
                    AvailableOnMobile = item.AvailableOnMobile,
                    CreatedDate = DateTimeProvider.Now,
                    IsDeleted = false
                }, cancellationToken);
                result.CreatedCount++;
            }
        }

        result.TotalProcessed = normalized.Count;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<PermissionDefinitionSyncResultDto>.SuccessResult(result, _localizationService.GetLocalizedString("OperationSuccessful"));
    }
}
