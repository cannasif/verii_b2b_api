using Wms.Application.Common;
using Wms.Application.Stock.Dtos;

namespace Wms.Application.Stock.Services;

public interface IStockDetailService
{
    Task<ApiResponse<StockDetailDto?>> GetByStockIdAsync(long stockId, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockDetailDto>> CreateAsync(CreateStockDetailDto createDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockDetailDto>> UpdateAsync(long id, UpdateStockDetailDto updateDto, CancellationToken cancellationToken = default);
}
