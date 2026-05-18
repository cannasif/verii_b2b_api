using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.Warehouse.Dtos;
using Wms.Domain.Common;
using WarehouseEntity = Wms.Domain.Entities.Warehouse.Warehouse;

namespace Wms.Application.Warehouse.Services;

public sealed class WarehouseService : IWarehouseService
{
    private readonly IRepository<WarehouseEntity> _warehouses;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public WarehouseService(
        IRepository<WarehouseEntity> warehouses,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IMapper mapper)
    {
        _warehouses = warehouses;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<WarehouseDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await BuildQuery().ToListAsync(cancellationToken);
        return ApiResponse<IEnumerable<WarehouseDto>>.SuccessResult(_mapper.Map<List<WarehouseDto>>(items), _localizationService.GetLocalizedString("WarehouseRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<PagedResponse<WarehouseDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = BuildQuery()
            .ApplySearch(request.Search, nameof(WarehouseEntity.WarehouseName))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(request.SortBy ?? nameof(WarehouseEntity.Id), string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync(cancellationToken);
        var dtoItems = _mapper.Map<List<WarehouseDto>>(items);
        var page = new PagedResponse<WarehouseDto>(dtoItems, total, request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 20 : request.PageSize);
        return ApiResponse<PagedResponse<WarehouseDto>>.SuccessResult(page, _localizationService.GetLocalizedString("WarehouseRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<WarehouseDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _warehouses.Query().Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null || entity.IsDeleted)
        {
            var message = _localizationService.GetLocalizedString("WarehouseNotFound");
            return ApiResponse<WarehouseDto>.ErrorResult(message, message, 404);
        }

        return ApiResponse<WarehouseDto>.SuccessResult(_mapper.Map<WarehouseDto>(entity), _localizationService.GetLocalizedString("WarehouseRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<WarehouseDto>> CreateAsync(CreateWarehouseDto createDto, CancellationToken cancellationToken = default)
    {
        var exists = await _warehouses.Query().AnyAsync(x => x.WarehouseCode == createDto.WarehouseCode && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            var message = _localizationService.GetLocalizedString("WarehouseAlreadyExists");
            return ApiResponse<WarehouseDto>.ErrorResult(message, message, 400);
        }

        var entity = _mapper.Map<WarehouseEntity>(createDto) ?? new WarehouseEntity();
        entity.BranchCode = NormalizeBranchCode(createDto.BranchCode);
        entity.CreatedDate = DateTimeProvider.Now;
        await _warehouses.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<WarehouseDto>.SuccessResult(_mapper.Map<WarehouseDto>(entity), _localizationService.GetLocalizedString("WarehouseCreatedSuccessfully"));
    }

    public async Task<ApiResponse<WarehouseDto>> UpdateAsync(long id, UpdateWarehouseDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _warehouses.Query(tracking: true).Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null || entity.IsDeleted)
        {
            var message = _localizationService.GetLocalizedString("WarehouseNotFound");
            return ApiResponse<WarehouseDto>.ErrorResult(message, message, 404);
        }

        if (updateDto.WarehouseCode.HasValue)
        {
            var duplicate = await _warehouses.Query().AnyAsync(x => x.Id != id && x.WarehouseCode == updateDto.WarehouseCode.Value && !x.IsDeleted, cancellationToken);
            if (duplicate)
            {
                var message = _localizationService.GetLocalizedString("WarehouseAlreadyExists");
                return ApiResponse<WarehouseDto>.ErrorResult(message, message, 400);
            }
        }

        _mapper.Map(updateDto, entity);
        if (!string.IsNullOrWhiteSpace(updateDto.BranchCode))
        {
            entity.BranchCode = NormalizeBranchCode(updateDto.BranchCode);
        }

        entity.UpdatedDate = DateTimeProvider.Now;
        _warehouses.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<WarehouseDto>.SuccessResult(_mapper.Map<WarehouseDto>(entity), _localizationService.GetLocalizedString("WarehouseUpdatedSuccessfully"));
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var exists = await _warehouses.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            var message = _localizationService.GetLocalizedString("WarehouseNotFound");
            return ApiResponse<bool>.ErrorResult(message, message, 404);
        }

        await _warehouses.SoftDelete(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("WarehouseDeletedSuccessfully"));
    }

    public async Task<ApiResponse<int>> WarehouseSyncAsync(IEnumerable<SyncWarehouseDto> warehouses, CancellationToken cancellationToken = default)
    {
        var input = warehouses?
            .Where(x => x.WarehouseCode > 0 && !string.IsNullOrWhiteSpace(x.WarehouseName))
            .ToList() ?? new List<SyncWarehouseDto>();

        if (input.Count == 0)
        {
            return ApiResponse<int>.SuccessResult(0, _localizationService.GetLocalizedString("WarehouseSyncCompletedSuccessfully"));
        }

        var now = DateTimeProvider.Now;
        var codes = input.Select(x => x.WarehouseCode).Distinct().ToList();
        var existing = await _warehouses.Query(tracking: true)
            .Where(x => codes.Contains(x.WarehouseCode))
            .ToListAsync(cancellationToken);
        var existingByCode = existing.ToDictionary(x => x.WarehouseCode);

        var insertedCount = 0;
        foreach (var item in input)
        {
            if (existingByCode.TryGetValue(item.WarehouseCode, out var entity))
            {
                entity.WarehouseName = item.WarehouseName.Trim();
                entity.BranchCode = NormalizeBranchCode(item.BranchCode);
                entity.IsDeleted = false;
                entity.DeletedDate = null;
                entity.DeletedBy = null;
                entity.UpdatedDate = now;
                _warehouses.Update(entity);
                continue;
            }

            var newEntity = _mapper.Map<WarehouseEntity>(item) ?? new WarehouseEntity();
            newEntity.BranchCode = NormalizeBranchCode(item.BranchCode);
            newEntity.CreatedDate = now;
            newEntity.UpdatedDate = now;
            await _warehouses.AddAsync(newEntity, cancellationToken);
            insertedCount++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<int>.SuccessResult(insertedCount, _localizationService.GetLocalizedString("WarehouseSyncCompletedSuccessfully"));
    }

    private IQueryable<WarehouseEntity> BuildQuery()
    {
        return _warehouses.Query().Where(x => !x.IsDeleted);
    }

    private static string NormalizeBranchCode(string? branchCode) => string.IsNullOrWhiteSpace(branchCode) ? "0" : branchCode.Trim();
}
