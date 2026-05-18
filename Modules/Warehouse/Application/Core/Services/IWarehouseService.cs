using Wms.Application.Common;
using Wms.Application.Warehouse.Dtos;

namespace Wms.Application.Warehouse.Services;

public interface IWarehouseService
{
    Task<ApiResponse<IEnumerable<WarehouseDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<WarehouseDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<WarehouseDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<WarehouseDto>> CreateAsync(CreateWarehouseDto createDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<WarehouseDto>> UpdateAsync(long id, UpdateWarehouseDto updateDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> WarehouseSyncAsync(IEnumerable<SyncWarehouseDto> warehouses, CancellationToken cancellationToken = default);
}
