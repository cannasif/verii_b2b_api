using Wms.Application.Customer.Dtos;

namespace Wms.Application.Customer.Services;

public interface ICustomerSyncJob
{
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
