using System.Linq.Expressions;
using Wms.Domain.Entities.Common;

namespace Wms.Application.Common;

/// <summary>
/// Application katmanının veri erişim bağımlılığı; `_old` generic repository davranışını application sınırına taşır.
/// </summary>
public interface IRepository<TEntity> where TEntity : BaseEntity
{
    IQueryable<TEntity> Query(bool tracking = false, bool ignoreQueryFilters = false);
    Task<TEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> GetFirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    Task SoftDelete(long id, CancellationToken cancellationToken = default);
    void SoftDeleteRange(IEnumerable<long> ids);
    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
}
