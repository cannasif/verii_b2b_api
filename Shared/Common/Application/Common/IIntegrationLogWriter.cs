namespace Wms.Application.Common;

public interface IIntegrationLogWriter
{
    Task TryWriteAsync(IntegrationLogWriteRequest request, CancellationToken cancellationToken = default);
}
