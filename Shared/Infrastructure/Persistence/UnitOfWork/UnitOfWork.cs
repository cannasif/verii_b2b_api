using Wms.Application.Common;
using Wms.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace Wms.Infrastructure.Persistence.UnitOfWork;

/// <summary>
/// İlk pragmatik batch için commit sınırını DbContext üstünden taşır.
/// Repository property şişmesi yerine application katmanını sade tutar.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly WmsDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(WmsDbContext context)
    {
        _context = context;
    }

    public async Task<long> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var affectedRows = await _context.SaveChangesAsync(cancellationToken);
        return affectedRows;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return;
        }

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return;
        }

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }
}
