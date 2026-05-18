using Microsoft.Data.SqlClient;

namespace Wms.Modules.NetsisIntegrations.Application.Services;

public sealed record NetsisQueryDefinition(
    string Operation,
    string Sql,
    string Source,
    IReadOnlyCollection<SqlParameter> Parameters);

public interface INetsisQueryExecutor
{
    Task<List<T>> QueryAsync<T>(
        NetsisQueryDefinition definition,
        Func<SqlDataReader, T> map,
        CancellationToken cancellationToken = default);
}
