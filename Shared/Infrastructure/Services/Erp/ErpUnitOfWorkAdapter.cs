using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Wms.Application.Common;
using Wms.Infrastructure.Persistence.Context;

namespace Wms.Infrastructure.Services.Erp;

public sealed class ErpUnitOfWorkAdapter : IErpUnitOfWork
{
    private readonly WmsDbContext _dbContext;

    public ErpUnitOfWorkAdapter(WmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<TEntity> Query<TEntity>(bool tracking = false) where TEntity : class
    {
        var query = _dbContext.Set<TEntity>().AsQueryable();
        return tracking ? query : query.AsNoTracking();
    }

    public IQueryable<TEntity> SqlQuery<TEntity>(string sql, params object[] parameters) where TEntity : class
    {
        return _dbContext.Set<TEntity>().FromSqlRaw(sql, parameters).AsNoTracking();
    }

    public string? GetConnectionString() => _dbContext.Database.GetConnectionString();

    public DbConnection GetDbConnection() => _dbContext.Database.GetDbConnection();
}
