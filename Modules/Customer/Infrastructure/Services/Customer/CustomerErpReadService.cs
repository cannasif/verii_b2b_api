using Microsoft.Data.SqlClient;
using Wms.Application.Common;
using Wms.Application.Customer.Services;
using Wms.Domain.Entities.Customer.Functions;
using Wms.Modules.NetsisIntegrations.Application.Services;

namespace Wms.Infrastructure.Services.Customer;

public sealed class CustomerErpReadService : ICustomerErpReadService
{
    private readonly INetsisQueryExecutor _netsisQueryExecutor;

    public CustomerErpReadService(
        INetsisQueryExecutor netsisQueryExecutor)
    {
        _netsisQueryExecutor = netsisQueryExecutor;
    }

    public async Task<List<FnCustomerRow>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _netsisQueryExecutor.QueryAsync(
            new NetsisQueryDefinition(
                "RII_FN_CARI",
                "SELECT * FROM dbo.RII_FN_CARI(@cariKodu, @branchCode)",
                "CustomerErpReadService.GetAllAsync",
                new[]
                {
                    new SqlParameter("@cariKodu", DBNull.Value),
                    new SqlParameter("@branchCode", DBNull.Value)
                }),
            reader => new FnCustomerRow
                {
                    SubeKodu = GetNullableValue<short>(reader, "SUBE_KODU"),
                    IsletmeKodu = GetNullableValue<short>(reader, "ISLETME_KODU"),
                    CariKod = GetNullableString(reader, "CARI_KOD") ?? string.Empty,
                    CariIsim = GetNullableString(reader, "CARI_ISIM"),
                    GroupCode = GetNullableString(reader, "GRUP_KODU"),
                    CreditLimit = GetNullableValue<decimal>(reader, "RISK_SINIRI"),
                    PriceListNumber = GetNullableValue<short>(reader, "LISTE_FIATI"),
                    PaymentTermDays = GetNullableValue<short>(reader, "VADE_GUNU")
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
