using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.Common;
using Wms.Infrastructure.Persistence.Context;

namespace Wms.Infrastructure.Persistence.Repositories;

/// <summary>
/// `_old` generic repository yüzeyini pragmatik DbContext üzerinde gerçekler.
/// İlk aşamada parameter vertical slice için gereken davranışları taşır.
/// </summary>
public sealed class GenericRepository<TEntity> : IGenericRepository<TEntity>
    where TEntity : BaseEntity
{
    private readonly WmsDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(WmsDbContext context)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query(bool tracking = false, bool ignoreQueryFilters = false)
    {
        IQueryable<TEntity> query = _dbSet;

        if (!ignoreQueryFilters)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (!tracking)
        {
            query = query.AsNoTracking();
        }

        return query;
    }

    public async Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await Query()
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(predicate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.CreatedDate = DateTimeProvider.Now;
        entity.IsDeleted = false;
        await _dbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var materialized = entities.ToList();
        foreach (var entity in materialized)
        {
            entity.CreatedDate = DateTimeProvider.Now;
            entity.IsDeleted = false;
        }

        await _dbSet.AddRangeAsync(materialized, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        entity.SetUpdatedInfo();
        _dbSet.Update(entity);
    }

    public async Task SoftDelete(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbSet
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (entity == null)
        {
            return;
        }

        entity.MarkAsDeleted();
        _dbSet.Update(entity);
    }

    public void SoftDeleteRange(IEnumerable<long> ids)
    {
        var materializedIds = ids.ToList();
        var entities = _dbSet
            .Where(x => materializedIds.Contains(x.Id))
            .ToList();

        foreach (var entity in entities)
        {
            entity.MarkAsDeleted();
        }
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await Query()
            .Where(x => x.Id == id)
            .AnyAsync(cancellationToken);
    }

    public async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        return await Query()
            .CountAsync(cancellationToken);
    }
}
