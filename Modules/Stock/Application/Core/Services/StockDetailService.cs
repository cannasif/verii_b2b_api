using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.Stock.Dtos;
using Wms.Domain.Common;
using Wms.Domain.Entities.Stock;
using StockEntity = Wms.Domain.Entities.Stock.Stock;

namespace Wms.Application.Stock.Services;

public sealed class StockDetailService : IStockDetailService
{
    private readonly IRepository<StockDetail> _details;
    private readonly IRepository<StockEntity> _stocks;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILocalizationService _localizationService;
    private readonly IMapper _mapper;

    public StockDetailService(
        IRepository<StockDetail> details,
        IRepository<StockEntity> stocks,
        IUnitOfWork unitOfWork,
        ILocalizationService localizationService,
        IMapper mapper)
    {
        _details = details;
        _stocks = stocks;
        _unitOfWork = unitOfWork;
        _localizationService = localizationService;
        _mapper = mapper;
    }

    public async Task<ApiResponse<StockDetailDto?>> GetByStockIdAsync(long stockId, CancellationToken cancellationToken = default)
    {
        var entity = await _details.Query()
            .Where(x => x.StockId == stockId && !x.IsDeleted)
            .Include(x => x.Stock)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return ApiResponse<StockDetailDto?>.SuccessResult(null, _localizationService.GetLocalizedString("StockDetailRetrievedSuccessfully"));
        }

        return ApiResponse<StockDetailDto?>.SuccessResult(_mapper.Map<StockDetailDto>(entity), _localizationService.GetLocalizedString("StockDetailRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<StockDetailDto>> CreateAsync(CreateStockDetailDto createDto, CancellationToken cancellationToken = default)
    {
        var stock = await _stocks.Query().FirstOrDefaultAsync(x => x.Id == createDto.StockId && !x.IsDeleted, cancellationToken);
        if (stock == null)
        {
            return ApiResponse<StockDetailDto>.ErrorResult(
                _localizationService.GetLocalizedString("StockNotFound"),
                _localizationService.GetLocalizedString("StockNotFound"),
                404);
        }

        var exists = await _details.Query().AnyAsync(x => x.StockId == createDto.StockId && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            return ApiResponse<StockDetailDto>.ErrorResult(
                _localizationService.GetLocalizedString("StockDetailAlreadyExists"),
                _localizationService.GetLocalizedString("StockDetailAlreadyExists"),
                400);
        }

        var entity = new StockDetail
        {
            StockId = createDto.StockId,
            HtmlDescription = createDto.HtmlDescription?.Trim() ?? string.Empty,
            TechnicalSpecsJson = string.IsNullOrWhiteSpace(createDto.TechnicalSpecsJson) ? null : createDto.TechnicalSpecsJson.Trim(),
            BranchCode = stock.BranchCode,
        };

        await _details.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var persisted = await _details.Query()
            .Where(x => x.Id == entity.Id && !x.IsDeleted)
            .Include(x => x.Stock)
            .AsNoTracking()
            .FirstAsync(cancellationToken);

        return ApiResponse<StockDetailDto>.SuccessResult(_mapper.Map<StockDetailDto>(persisted), _localizationService.GetLocalizedString("StockDetailCreatedSuccessfully"));
    }

    public async Task<ApiResponse<StockDetailDto>> UpdateAsync(long id, UpdateStockDetailDto updateDto, CancellationToken cancellationToken = default)
    {
        var entity = await _details.Query(tracking: true)
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            return ApiResponse<StockDetailDto>.ErrorResult(
                _localizationService.GetLocalizedString("StockDetailNotFound"),
                _localizationService.GetLocalizedString("StockDetailNotFound"),
                404);
        }

        if (entity.StockId != updateDto.StockId)
        {
            var stockExists = await _stocks.Query().AnyAsync(x => x.Id == updateDto.StockId && !x.IsDeleted, cancellationToken);
            if (!stockExists)
            {
                return ApiResponse<StockDetailDto>.ErrorResult(
                _localizationService.GetLocalizedString("StockNotFound"),
                _localizationService.GetLocalizedString("StockNotFound"),
                404);
            }

            var duplicate = await _details.Query().AnyAsync(
                x => x.Id != id && x.StockId == updateDto.StockId && !x.IsDeleted,
                cancellationToken);

            if (duplicate)
            {
                return ApiResponse<StockDetailDto>.ErrorResult(
                _localizationService.GetLocalizedString("StockDetailAlreadyExists"),
                _localizationService.GetLocalizedString("StockDetailAlreadyExists"),
                400);
            }

            entity.StockId = updateDto.StockId;
        }

        entity.HtmlDescription = updateDto.HtmlDescription?.Trim() ?? string.Empty;
        entity.TechnicalSpecsJson = string.IsNullOrWhiteSpace(updateDto.TechnicalSpecsJson) ? null : updateDto.TechnicalSpecsJson.Trim();
        entity.UpdatedDate = DateTimeProvider.Now;

        _details.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var persisted = await _details.Query()
            .Where(x => x.Id == entity.Id && !x.IsDeleted)
            .Include(x => x.Stock)
            .AsNoTracking()
            .FirstAsync(cancellationToken);

        return ApiResponse<StockDetailDto>.SuccessResult(_mapper.Map<StockDetailDto>(persisted), _localizationService.GetLocalizedString("StockDetailUpdatedSuccessfully"));
    }
}
