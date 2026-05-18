using Microsoft.Data.SqlClient;
using Wms.Application.Common;
using Wms.Application.Stock.Services;
using Wms.Domain.Entities.Stock.Functions;
using Wms.Modules.NetsisIntegrations.Application.Services;

namespace Wms.Infrastructure.Services.Stock;

public sealed class StockErpReadService : IStockErpReadService
{
    private readonly INetsisQueryExecutor _netsisQueryExecutor;

    public StockErpReadService(
        INetsisQueryExecutor netsisQueryExecutor)
    {
        _netsisQueryExecutor = netsisQueryExecutor;
    }

    public async Task<List<FnStockRow>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _netsisQueryExecutor.QueryAsync(
            new NetsisQueryDefinition(
                "RII_FN_STOK",
                "SELECT * FROM dbo.RII_FN_STOK(@stokKodu, @branchCode)",
                "StockErpReadService.GetAllAsync",
                new[]
                {
                    new SqlParameter("@stokKodu", DBNull.Value),
                    new SqlParameter("@branchCode", DBNull.Value)
                }),
            reader => new FnStockRow
                {
                    SubeKodu = GetNullableValue<short>(reader, "SUBE_KODU"),
                    IsletmeKodu = GetNullableValue<short>(reader, "ISLETME_KODU"),
                    StokKodu = GetNullableString(reader, "STOK_KODU") ?? string.Empty,
                    UreticiKodu = GetNullableString(reader, "URETICI_KODU"),
                    StokAdi = GetNullableString(reader, "STOK_ADI"),
                    GrupKodu = GetNullableString(reader, "GRUP_KODU"),
                    Kod1 = GetNullableString(reader, "KOD_1"),
                    Kod2 = GetNullableString(reader, "KOD_2"),
                    Kod3 = GetNullableString(reader, "KOD_3"),
                    Kod4 = GetNullableString(reader, "KOD_4"),
                    Kod5 = GetNullableString(reader, "KOD_5")
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
