namespace Wms.Application.Common;

public interface IRequestCancellationAccessor
{
    CancellationToken Get(CancellationToken cancellationToken = default);
}
