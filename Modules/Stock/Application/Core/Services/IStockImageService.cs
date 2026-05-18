using Microsoft.AspNetCore.Http;
using Wms.Application.Common;
using Wms.Application.Stock.Dtos;

namespace Wms.Application.Stock.Services;

public interface IStockImageService
{
    Task<ApiResponse<List<StockImageDto>>> GetByStockIdAsync(long stockId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<StockImageDto>>> UploadImagesAsync(long stockId, List<IFormFile> files, List<string>? altTexts = null, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockImageDto>> SetPrimaryImageAsync(long imageId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteAsync(long id, CancellationToken cancellationToken = default);
}
