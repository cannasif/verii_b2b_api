using Wms.Application.Common;
using Wms.Application.Stock.Dtos;

namespace Wms.Application.Stock.Services;

public interface IStockService
{
    Task<ApiResponse<IEnumerable<StockDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<StockDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockDto>> CreateAsync(CreateStockDto createDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<StockDto>> UpdateAsync(long id, UpdateStockDto updateDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> StockSyncAsync(IEnumerable<SyncStockDto> stocks, CancellationToken cancellationToken = default);
}
