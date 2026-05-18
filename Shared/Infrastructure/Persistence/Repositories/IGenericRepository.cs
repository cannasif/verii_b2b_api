using System.Linq.Expressions;
using Wms.Application.Common;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Repositories;

public interface IGenericRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
}
