using System.Data.Common;

namespace Wms.Application.Common;

public interface IErpUnitOfWork
{
    IQueryable<TEntity> Query<TEntity>(bool tracking = false) where TEntity : class;
    IQueryable<TEntity> SqlQuery<TEntity>(string sql, params object[] parameters) where TEntity : class;
    string? GetConnectionString();
    DbConnection GetDbConnection();
}
