using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.YapKod.Dtos;
using Wms.Domain.Common;
using StockEntity = Wms.Domain.Entities.Stock.Stock;
using YapKodEntity = Wms.Domain.Entities.YapKod.YapKod;

namespace Wms.Application.YapKod.Services;

public sealed class YapKodService : IYapKodService
{
    private readonly IRepository<YapKodEntity> _yapKodRepository;
    private readonly IRepository<StockEntity> _stockRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public YapKodService(
        IRepository<YapKodEntity> yapKodRepository,
        IRepository<StockEntity> stockRepository,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IMapper mapper)
    {
        _yapKodRepository = yapKodRepository;
        _stockRepository = stockRepository;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<IEnumerable<YapKodDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await BuildQuery().ToListAsync(cancellationToken);
        return ApiResponse<IEnumerable<YapKodDto>>.SuccessResult(
            _mapper.Map<List<YapKodDto>>(items),
            _localizationService.GetLocalizedString("YapKodRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<PagedResponse<YapKodDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();

        var query = BuildQuery()
            .ApplySearch(request.Search,
                nameof(YapKodEntity.YapKodCode),
                nameof(YapKodEntity.YapAcik),
                nameof(YapKodEntity.YplndrStokKod))
            .ApplyFilters(request.Filters, request.FilterLogic)
            .ApplySorting(request.SortBy ?? nameof(YapKodEntity.Id), string.Equals(request.SortDirection, "desc", StringComparison.OrdinalIgnoreCase));

        var total = await query.CountAsync(cancellationToken);
        var items = await query.ApplyPagination(request.PageNumber, request.PageSize).ToListAsync(cancellationToken);
        var dtoItems = _mapper.Map<List<YapKodDto>>(items);
        var page = new PagedResponse<YapKodDto>(dtoItems, total, request.PageNumber < 1 ? 1 : request.PageNumber, request.PageSize < 1 ? 20 : request.PageSize);
        return ApiResponse<PagedResponse<YapKodDto>>.SuccessResult(page, _localizationService.GetLocalizedString("YapKodRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<YapKodDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _yapKodRepository.Query().Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null || entity.IsDeleted)
        {
            var message = _localizationService.GetLocalizedString("YapKodNotFound");
            return ApiResponse<YapKodDto>.ErrorResult(message, message, 404);
        }

        return ApiResponse<YapKodDto>.SuccessResult(
            _mapper.Map<YapKodDto>(entity),
            _localizationService.GetLocalizedString("YapKodRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<YapKodDto>> CreateAsync(CreateYapKodDto createDto, CancellationToken cancellationToken = default)
    {
        var stockValidation = await ValidateStockAsync(createDto.StockId, cancellationToken);
        if (stockValidation is not null)
        {
            return stockValidation;
        }

        var normalizedCode = NormalizeCode(createDto.YapKod);
        var exists = await _yapKodRepository.Query().AnyAsync(x => x.YapKodCode == normalizedCode && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            var message = _localizationService.GetLocalizedString("YapKodAlreadyExists");
            return ApiResponse<YapKodDto>.ErrorResult(message, message, 400);
        }

        var entity = _mapper.Map<YapKodEntity>(createDto) ?? new YapKodEntity();
        entity.YapKodCode = normalizedCode;
        entity.YapAcik = createDto.YapAcik.Trim();
        entity.YplndrStokKod = createDto.YplndrStokKod?.Trim();
        entity.StockId = createDto.StockId;
        entity.BranchCode = NormalizeBranchCode(createDto.BranchCode);
        entity.CreatedDate = DateTimeProvider.Now;
        entity.LastSyncDate = DateTimeProvider.Now;

        await _yapKodRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<YapKodDto>.SuccessResult(
            _mapper.Map<YapKodDto>(entity),
            _localizationService.GetLocalizedString("YapKodCreatedSuccessfully"));
    }

    public async Task<ApiResponse<YapKodDto>> UpdateAsync(long id, UpdateYapKodDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _yapKodRepository.Query(tracking: true).Where(x => x.Id == id).FirstOrDefaultAsync(cancellationToken);
        if (entity == null || entity.IsDeleted)
        {
            var message = _localizationService.GetLocalizedString("YapKodNotFound");
            return ApiResponse<YapKodDto>.ErrorResult(message, message, 404);
        }

        var stockValidation = await ValidateStockAsync(updateDto.StockId, cancellationToken);
        if (stockValidation is not null)
        {
            return stockValidation;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.YapKod))
        {
            var normalizedCode = NormalizeCode(updateDto.YapKod);
            var duplicate = await _yapKodRepository.Query().AnyAsync(x => x.Id != id && x.YapKodCode == normalizedCode && !x.IsDeleted, cancellationToken);
            if (duplicate)
            {
                var message = _localizationService.GetLocalizedString("YapKodAlreadyExists");
                return ApiResponse<YapKodDto>.ErrorResult(message, message, 400);
            }
        }

        _mapper.Map(updateDto, entity);

        if (!string.IsNullOrWhiteSpace(updateDto.YapKod))
        {
            entity.YapKodCode = NormalizeCode(updateDto.YapKod);
        }

        if (!string.IsNullOrWhiteSpace(updateDto.YapAcik))
        {
            entity.YapAcik = updateDto.YapAcik.Trim();
        }

        if (updateDto.YplndrStokKod != null)
        {
            entity.YplndrStokKod = string.IsNullOrWhiteSpace(updateDto.YplndrStokKod) ? null : updateDto.YplndrStokKod.Trim();
        }

        if (updateDto.StockId.HasValue || updateDto.StockId == null)
        {
            entity.StockId = updateDto.StockId;
        }

        if (!string.IsNullOrWhiteSpace(updateDto.BranchCode))
        {
            entity.BranchCode = NormalizeBranchCode(updateDto.BranchCode);
        }

        entity.UpdatedDate = DateTimeProvider.Now;
        _yapKodRepository.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<YapKodDto>.SuccessResult(
            _mapper.Map<YapKodDto>(entity),
            _localizationService.GetLocalizedString("YapKodUpdatedSuccessfully"));
    }

    public async Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var exists = await _yapKodRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            var message = _localizationService.GetLocalizedString("YapKodNotFound");
            return ApiResponse<bool>.ErrorResult(message, message, 404);
        }

        await _yapKodRepository.SoftDelete(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("YapKodDeletedSuccessfully"));
    }

    public async Task<ApiResponse<int>> YapKodSyncAsync(IEnumerable<SyncYapKodDto> items, CancellationToken cancellationToken = default)
    {
        var input = items?
            .Where(x => !string.IsNullOrWhiteSpace(x.YapKod) && !string.IsNullOrWhiteSpace(x.YapAcik))
            .ToList() ?? new List<SyncYapKodDto>();

        if (input.Count == 0)
        {
            return ApiResponse<int>.SuccessResult(0, _localizationService.GetLocalizedString("YapKodSyncCompletedSuccessfully"));
        }

        var now = DateTimeProvider.Now;
        var codes = input.Select(x => NormalizeCode(x.YapKod)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existing = await _yapKodRepository.Query(tracking: true)
            .Where(x => codes.Contains(x.YapKodCode))
            .ToListAsync(cancellationToken);
        var existingByCode = existing.ToDictionary(x => NormalizeCode(x.YapKodCode), StringComparer.OrdinalIgnoreCase);

        var insertedCount = 0;
        foreach (var item in input)
        {
            var code = NormalizeCode(item.YapKod);
            if (existingByCode.TryGetValue(code, out var entity))
            {
                entity.YapAcik = item.YapAcik.Trim();
                entity.YplndrStokKod = item.YplndrStokKod?.Trim();
                entity.StockId = item.StockId;
                entity.BranchCode = NormalizeBranchCode(item.BranchCode);
                entity.LastSyncDate = now;
                entity.IsDeleted = false;
                entity.DeletedDate = null;
                entity.DeletedBy = null;
                entity.UpdatedDate = now;
                _yapKodRepository.Update(entity);
                continue;
            }

            var newEntity = _mapper.Map<YapKodEntity>(item) ?? new YapKodEntity();
            newEntity.YapKodCode = code;
            newEntity.YapAcik = item.YapAcik.Trim();
            newEntity.YplndrStokKod = item.YplndrStokKod?.Trim();
            newEntity.StockId = item.StockId;
            newEntity.BranchCode = NormalizeBranchCode(item.BranchCode);
            newEntity.CreatedDate = now;
            newEntity.UpdatedDate = now;
            newEntity.LastSyncDate = now;
            await _yapKodRepository.AddAsync(newEntity, cancellationToken);
            insertedCount++;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<int>.SuccessResult(insertedCount, _localizationService.GetLocalizedString("YapKodSyncCompletedSuccessfully"));
    }

    private IQueryable<YapKodEntity> BuildQuery()
    {
        return _yapKodRepository.Query().Where(x => !x.IsDeleted);
    }

    private async Task<ApiResponse<YapKodDto>?> ValidateStockAsync(long? stockId, CancellationToken cancellationToken)
    {
        if (!stockId.HasValue)
        {
            return null;
        }

        var exists = await _stockRepository.Query()
            .AnyAsync(x => x.Id == stockId.Value && !x.IsDeleted, cancellationToken);

        if (exists)
        {
            return null;
        }

        var message = _localizationService.GetLocalizedString("YapKodStockNotFound");
        return ApiResponse<YapKodDto>.ErrorResult(message, message, 400);
    }

    private static string NormalizeCode(string code) => code.Trim().ToUpperInvariant();
    private static string NormalizeBranchCode(string? branchCode) => string.IsNullOrWhiteSpace(branchCode) ? "0" : branchCode.Trim();
}
