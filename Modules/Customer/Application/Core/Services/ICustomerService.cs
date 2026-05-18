using Wms.Application.Common;
using Wms.Application.Customer.Dtos;

namespace Wms.Application.Customer.Services;

public interface ICustomerService
{
    Task<ApiResponse<IEnumerable<CustomerDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<PagedResponse<CustomerDto>>> GetPagedAsync(PagedRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> CreateAsync(CreateCustomerDto createDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerDto>> UpdateAsync(long id, UpdateCustomerDto updateDto, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> SoftDeleteAsync(long id, CancellationToken cancellationToken = default);
    Task<ApiResponse<int>> CustomerSyncAsync(IEnumerable<SyncCustomerDto> customers, CancellationToken cancellationToken = default);
}
