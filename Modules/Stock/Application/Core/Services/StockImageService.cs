using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Application.Stock.Dtos;
using Wms.Domain.Common;
using Wms.Domain.Entities.Stock;
using StockEntity = Wms.Domain.Entities.Stock.Stock;

namespace Wms.Application.Stock.Services;

public sealed class StockImageService : IStockImageService
{
    private readonly IRepository<StockImage> _images;
    private readonly IRepository<StockEntity> _stocks;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileUploadService _fileUploadService;
    private readonly IMapper _mapper;
    private readonly ILocalizationService _localization;

    public StockImageService(
        IRepository<StockImage> images,
        IRepository<StockEntity> stocks,
        IUnitOfWork unitOfWork,
        IFileUploadService fileUploadService,
        IMapper mapper,
        ILocalizationService localization)
    {
        _images = images;
        _stocks = stocks;
        _unitOfWork = unitOfWork;
        _fileUploadService = fileUploadService;
        _mapper = mapper;
        _localization = localization;
    }

    public async Task<ApiResponse<List<StockImageDto>>> GetByStockIdAsync(long stockId, CancellationToken cancellationToken = default)
    {
        var items = await _images.Query()
            .Where(x => x.StockId == stockId && !x.IsDeleted)
            .Include(x => x.Stock)
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .ToListAsync(cancellationToken);

        return ApiResponse<List<StockImageDto>>.SuccessResult(_mapper.Map<List<StockImageDto>>(items), _localization.GetLocalizedString("StockImagesRetrievedSuccessfully"));
    }

    public async Task<ApiResponse<List<StockImageDto>>> UploadImagesAsync(long stockId, List<IFormFile> files, List<string>? altTexts = null, CancellationToken cancellationToken = default)
    {
        if (files == null || files.Count == 0)
        {
            var needFiles = _localization.GetLocalizedString("StockImageFilesRequired");
            return ApiResponse<List<StockImageDto>>.ErrorResult(needFiles, needFiles, 400);
        }

        var stock = await _stocks.Query().FirstOrDefaultAsync(x => x.Id == stockId && !x.IsDeleted, cancellationToken);
        if (stock == null)
        {
            var nf = _localization.GetLocalizedString("StockNotFound");
            return ApiResponse<List<StockImageDto>>.ErrorResult(nf, nf, 404);
        }

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var nextSortOrder = await _images.Query()
                .Where(x => x.StockId == stockId && !x.IsDeleted)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync(cancellationToken) ?? 0;

            var created = new List<StockImage>();
            for (var index = 0; index < files.Count; index++)
            {
                var upload = await _fileUploadService.UploadStockImageAsync(files[index], stockId, cancellationToken);
                if (!upload.Success || string.IsNullOrWhiteSpace(upload.Data))
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<List<StockImageDto>>.ErrorResult(upload.Message, upload.ExceptionMessage, upload.StatusCode);
                }

                nextSortOrder++;
                var image = new StockImage
                {
                    StockId = stockId,
                    Stock = stock,
                    BranchCode = stock.BranchCode,
                    FilePath = upload.Data!,
                    AltText = altTexts != null && index < altTexts.Count && !string.IsNullOrWhiteSpace(altTexts[index]) ? altTexts[index].Trim() : null,
                    SortOrder = nextSortOrder,
                    IsPrimary = false,
                };

                await _images.AddAsync(image, cancellationToken);
                created.Add(image);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return ApiResponse<List<StockImageDto>>.SuccessResult(_mapper.Map<List<StockImageDto>>(created), _localization.GetLocalizedString("StockImagesUploadedSuccessfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApiResponse<List<StockImageDto>>.ErrorResult(_localization.GetLocalizedString("StockImageUploadFailed"), ex.Message, 500);
        }
    }

    public async Task<ApiResponse<StockImageDto>> SetPrimaryImageAsync(long imageId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var image = await _images.Query(tracking: true)
                .FirstOrDefaultAsync(x => x.Id == imageId && !x.IsDeleted, cancellationToken);

            if (image == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                var nin = _localization.GetLocalizedString("StockImageNotFound");
                return ApiResponse<StockImageDto>.ErrorResult(nin, nin, 404);
            }

            var otherPrimaries = await _images.Query(tracking: true)
                .Where(x => x.StockId == image.StockId && x.Id != imageId && x.IsPrimary && !x.IsDeleted)
                .ToListAsync(cancellationToken);

            foreach (var other in otherPrimaries)
            {
                other.IsPrimary = false;
                other.UpdatedDate = DateTimeProvider.Now;
                _images.Update(other);
            }

            image.IsPrimary = true;
            image.UpdatedDate = DateTimeProvider.Now;
            _images.Update(image);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var persisted = await _images.Query()
                .Where(x => x.Id == imageId && !x.IsDeleted)
                .Include(x => x.Stock)
                .AsNoTracking()
                .FirstAsync(cancellationToken);

            return ApiResponse<StockImageDto>.SuccessResult(_mapper.Map<StockImageDto>(persisted), _localization.GetLocalizedString("StockImagePrimaryUpdatedSuccessfully"));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApiResponse<StockImageDto>.ErrorResult(_localization.GetLocalizedString("StockImagePrimaryUpdateFailed"), ex.Message, 500);
        }
    }

    public async Task<ApiResponse<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var image = await _images.Query()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (image == null)
        {
            var imgNf = _localization.GetLocalizedString("StockImageNotFound");
            return ApiResponse<bool>.ErrorResult(imgNf, imgNf, 404);
        }

        await _images.SoftDelete(id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _fileUploadService.DeleteFileAsync(image.FilePath, cancellationToken);

        return ApiResponse<bool>.SuccessResult(true, _localization.GetLocalizedString("StockImageDeletedSuccessfully"));
    }
}
