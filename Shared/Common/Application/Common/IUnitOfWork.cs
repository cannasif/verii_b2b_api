namespace Wms.Application.Common;

/// <summary>
/// Application katmanının transaction/commit bağımlılığı; `_old` SaveChanges davranışı parity için taşınır.
/// </summary>
public interface IUnitOfWork
{
    Task<long> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
