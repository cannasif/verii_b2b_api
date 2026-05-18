using Wms.Domain.Entities.Customer.Functions;

namespace Wms.Application.Customer.Services;

public interface ICustomerErpReadService
{
    Task<List<FnCustomerRow>> GetAllAsync(CancellationToken cancellationToken = default);
}
