using System.Linq.Expressions;
using Wms.Application.Common;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Repositories;

/// <summary>
/// `_old/reference/verii_wms_api.Infrastructure/Persistence/Repositories/IGenericRepository.cs` kontratını yeni yapıda korur.
/// </summary>
public interface IGenericRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
}
