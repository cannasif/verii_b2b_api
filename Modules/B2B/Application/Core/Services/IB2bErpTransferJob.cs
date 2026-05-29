namespace Wms.Application.B2B.Services;

public interface IB2bErpTransferJob
{
    Task<int> RunAsync(CancellationToken cancellationToken = default);
}
