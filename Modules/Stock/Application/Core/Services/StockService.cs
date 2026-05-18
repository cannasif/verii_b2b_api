using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.Stock.Dtos;
using Wms.Domain.Common;
using StockEntity = Wms.Domain.Entities.Stock.Stock;

namespace Wms.Application.Stock.Services;

public sealed class StockService : IStockService
{
    private readonly IRepository<StockEntity> _stocks;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public StockService(
        IRepository<StockEntity> stocks,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IMapper mapper)
    {
        _stocks = stocks;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<StockDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await BuildQuery().ToListAsync(cancellationToken);
        return ApiResponse<IEnumerable<StockDto>>.SuccessResult(_mapper.Map<List<StockDto>>(items), _localizationService.GetLocalizedString("StockRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<PagedResponse<StockDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = BuildQuery()
            .ApplySearch(request.Search,
                nameof(StockEntity.ErpStockCode),
                nameof(StockEntity.StockName),
                nameof(StockEntity.Unit),
                nameof(StockEntity.UreticiKodu),
                nameof(StockEntity.GrupKodu),
                nameof(StockEntity.GrupAdi),
                nameof(StockEntity.Kod1),
                nameof(StockEntity.Kod1Adi),
                nameof(StockEntity.Kod2),
                nameof(StockEntity.Kod2Adi),
                nameof(StockEntity.Kod3),
                nameof(StockEntity.Kod3Adi),
                nameof(StockEntity.Kod4),
                nameof(StockEntity.Kod4Adi),
                nameof(StockEntity.Kod5),
                nameof(StockEntity.Kod5Adi))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(request.SortBy ?? nameof(StockEntity.Id), string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync(cancellationToken);
        var dtoItems = _mapper.Map<List<StockDto>>(items);
        var page = new PagedResponse<StockDto>(dtoItems, total, request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 20 : request.PageSize);
        return ApiResponse<PagedResponse<StockDto>>.SuccessResult(page, _localizationService.GetLocalizedString("StockRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<StockDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _stocks.Query().Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null || entity.IsDeleted)
        {
            var message = _localizationService.GetLocalizedString("StockNotFound");
            return ApiResponse<StockDto>.ErrorResult(message, message, 404);
        }

        return ApiResponse<StockDto>.SuccessResult(_mapper.Map<StockDto>(entity), _localizationService.GetLocalizedString("StockRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<StockDto>> CreateAsync(CreateStockDto createDto, CancellationToken cancellationToken = default)
    {
        var normalizedCode = NormalizeCode(createDto.ErpStockCode);
        var exists = await _stocks.Query().AnyAsync(x => x.ErpStockCode == normalizedCode && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            var message = _localizationService.GetLocalizedString("StockAlreadyExists");
            return ApiResponse<StockDto>.ErrorResult(message, message, 400);
        }

        var entity = _mapper.Map<StockEntity>(createDto) ?? new StockEntity();
        entity.ErpStockCode = normalizedCode;
        entity.BranchCode = NormalizeBranchCode(createDto.BranchCode);
        entity.CreatedDate = DateTimeProvider.Now;
        entity.LastSyncDate = DateTimeProvider.Now;
        await _stocks.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<StockDto>.SuccessResult(_mapper.Map<StockDto>(entity), _localizationService.GetLocalizedString("StockCreatedSuccessfully"));
    }

    public async Task<ApiResponse<StockDto>> UpdateAsync(long id, UpdateStockDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _stocks.Query(tracking: true).Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null || entity.IsDeleted)
        {
            var message = _localizationService.GetLocalizedString("StockNotFound");
            return ApiResponse<StockDto>.ErrorResult(message, message, 404);
        }

        if (!string.IsNullOrWhiteSpace(updateDto.ErpStockCode))
        {
            var normalizedCode = NormalizeCode(updateDto.ErpStockCode);
            var duplicate = await _stocks.Query().AnyAsync(x => x.Id != id && x.ErpStockCode == normalizedCode && !x.IsDeleted, cancellationToken);
            if (duplicate)
            {
                var message = _localizationService.GetLocalizedString("StockAlreadyExists");
                return ApiResponse<StockDto>.ErrorResult(message, message, 400);
            }
        }

        _mapper.Map(updateDto, entity);
        if (!string.IsNullOrWhiteSpace(updateDto.ErpStockCode))
        {
            entity.ErpStockCode = NormalizeCode(updateDto.ErpStockCode);
        }

        if (!string.IsNullOrWhiteSpace(updateDto.BranchCode))
        {
            entity.BranchCode = NormalizeBranchCode(updateDto.BranchCode);
        }

        entity.UpdatedDate = DateTimeProvider.Now;
        _stocks.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<StockDto>.SuccessResult(_mapper.Map<StockDto>(entity), _localizationService.GetLocalizedString("StockUpdatedSuccessfully"));
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var exists = await _stocks.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            var message = _localizationService.GetLocalizedString("StockNotFound");
            return ApiResponse<bool>.ErrorResult(message, message, 404);
        }

        await _stocks.SoftDelete(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("StockDeletedSuccessfully"));
    }

    public async Task<ApiResponse<int>> StockSyncAsync(IEnumerable<SyncStockDto> stocks, CancellationToken cancellationToken = default)
    {
        var input = stocks?
            .Where(x => !string.IsNullOrWhiteSpace(x.ErpStockCode) && !string.IsNullOrWhiteSpace(x.StockName))
            .ToList() ?? new List<SyncStockDto>();

        if (input.Count == 0)
        {
            return ApiResponse<int>.SuccessResult(0, _localizationService.GetLocalizedString("StockSyncCompletedSuccessfully"));
        }

        var now = DateTimeProvider.Now;
        var codes = input.Select(x => NormalizeCode(x.ErpStockCode)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existing = await _stocks.Query(tracking: true)
            .Where(x => codes.Contains(x.ErpStockCode))
            .ToListAsync(cancellationToken);
        var existingByCode = existing.ToDictionary(x => NormalizeCode(x.ErpStockCode), StringComparer.OrdinalIgnoreCase);

        var insertedCount = 0;
        foreach (var item in input)
        {
            var code = NormalizeCode(item.ErpStockCode);
            if (existingByCode.TryGetValue(code, out var entity))
            {
                MapSync(entity, item, now);
                entity.IsDeleted = false;
                entity.DeletedDate = null;
                entity.DeletedBy = null;
                entity.UpdatedDate = now;
                _stocks.Update(entity);
                continue;
            }

            var newEntity = _mapper.Map<StockEntity>(item) ?? new StockEntity();
            newEntity.ErpStockCode = code;
            newEntity.BranchCode = NormalizeBranchCode(item.BranchCode);
            newEntity.CreatedDate = now;
            newEntity.UpdatedDate = now;
            newEntity.LastSyncDate = now;
            await _stocks.AddAsync(newEntity, cancellationToken);
            insertedCount++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<int>.SuccessResult(insertedCount, _localizationService.GetLocalizedString("StockSyncCompletedSuccessfully"));
    }

    private IQueryable<StockEntity> BuildQuery()
    {
        return _stocks.Query().Where(x => !x.IsDeleted);
    }

    private static void MapSync(StockEntity entity, SyncStockDto item, DateTime now)
    {
        entity.StockName = item.StockName.Trim();
        entity.Unit = item.Unit?.Trim();
        entity.UreticiKodu = item.UreticiKodu?.Trim();
        entity.GrupKodu = item.GrupKodu?.Trim();
        entity.GrupAdi = item.GrupAdi?.Trim();
        entity.Kod1 = item.Kod1?.Trim();
        entity.Kod1Adi = item.Kod1Adi?.Trim();
        entity.Kod2 = item.Kod2?.Trim();
        entity.Kod2Adi = item.Kod2Adi?.Trim();
        entity.Kod3 = item.Kod3?.Trim();
        entity.Kod3Adi = item.Kod3Adi?.Trim();
        entity.Kod4 = item.Kod4?.Trim();
        entity.Kod4Adi = item.Kod4Adi?.Trim();
        entity.Kod5 = item.Kod5?.Trim();
        entity.Kod5Adi = item.Kod5Adi?.Trim();
        entity.BranchCode = NormalizeBranchCode(item.BranchCode);
        entity.LastSyncDate = now;
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
    private static string NormalizeBranchCode(string? branchCode) => string.IsNullOrWhiteSpace(branchCode) ? "0" : branchCode.Trim();
}
