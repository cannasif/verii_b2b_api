using Microsoft.Data.SqlClient;
using Wms.Application.Common;
using Wms.Application.Warehouse.Services;
using Wms.Domain.Entities.Warehouse.Functions;
using Wms.Modules.NetsisIntegrations.Application.Services;

namespace Wms.Infrastructure.Services.Warehouse;

public sealed class WarehouseErpReadService : IWarehouseErpReadService
{
    private readonly INetsisQueryExecutor _netsisQueryExecutor;

    public WarehouseErpReadService(
        INetsisQueryExecutor netsisQueryExecutor)
    {
        _netsisQueryExecutor = netsisQueryExecutor;
    }

    public async Task<List<FnWarehouseRow>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _netsisQueryExecutor.QueryAsync(
            new NetsisQueryDefinition(
                "RII_FN_DEPO",
                "SELECT * FROM dbo.RII_FN_DEPO(@depoKodu, @branchCode)",
                "WarehouseErpReadService.GetAllAsync",
                new[]
                {
                    new SqlParameter("@depoKodu", DBNull.Value),
                    new SqlParameter("@branchCode", DBNull.Value)
                }),
            reader => new FnWarehouseRow
                {
                    DepoKodu = GetNullableValue<short>(reader, "DEPO_KODU"),
                    DepoIsmi = GetNullableString(reader, "DEPO_ISMI")
                },
            cancellationToken);
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static T? GetNullableValue<T>(SqlDataReader reader, string columnName) where T : struct
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetFieldValue<T>(ordinal);
    }
}
