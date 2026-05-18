using Microsoft.Data.SqlClient;
using Wms.Application.Common;
using Wms.Modules.NetsisIntegrations.Application.Dtos;

namespace Wms.Modules.NetsisIntegrations.Application.Services;

public sealed class NetsisReadService : INetsisReadService
{
    private readonly INetsisQueryExecutor _netsisQueryExecutor;
    private readonly ILocalizationService _localizationService;

    public NetsisReadService(
        INetsisQueryExecutor netsisQueryExecutor,
        ILocalizationService localizationService)
    {
        _netsisQueryExecutor = netsisQueryExecutor;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<List<BranchDto>>> GetBranchesAsync(int? branchNo = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await _netsisQueryExecutor.QueryAsync(
                new NetsisQueryDefinition(
                    "RII_FN_BRANCHES",
                    "SELECT * FROM dbo.RII_FN_BRANCHES(@branchNo)",
                    "NetsisReadService.GetBranchesAsync",
                    new[]
                    {
                        new SqlParameter("@branchNo", branchNo.HasValue ? branchNo.Value : DBNull.Value)
                    }),
                reader => new BranchDto
                {
                    SubeKodu = reader.GetInt16(reader.GetOrdinal("SUBE_KODU")),
                    Unvan = reader.IsDBNull(reader.GetOrdinal("UNVAN"))
                        ? string.Empty
                        : reader.GetString(reader.GetOrdinal("UNVAN"))
                },
                cancellationToken);

            return ApiResponse<List<BranchDto>>.SuccessResult(rows, _localizationService.GetLocalizedString("OperationSuccessful"));
        }
        catch (Exception ex)
        {
            var message = _localizationService.GetLocalizedString("BranchesRetrievalError", ex.Message);
            return ApiResponse<List<BranchDto>>.ErrorResult(message, ex.Message, 500);
        }
    }

    public async Task<ApiResponse<List<KurDto>>> GetExchangeRatesAsync(DateTime date, int pricingType, CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await _netsisQueryExecutor.QueryAsync(
                new NetsisQueryDefinition(
                    "RII_FN_KUR",
                    "SELECT * FROM dbo.RII_FN_KUR(@tarih, @fiyatTipi)",
                    "NetsisReadService.GetExchangeRatesAsync",
                    new[]
                    {
                        new SqlParameter("@tarih", date.Date),
                        new SqlParameter("@fiyatTipi", pricingType)
                    }),
                reader => new KurDto
                {
                    DovizTipi = GetNullableValue<int>(reader, "DOVIZ_TIPI") ?? 0,
                    DovizIsmi = GetNullableString(reader, "DOVIZ_ISMI"),
                    KurDegeri = GetNullableValue<double>(reader, "KUR_DEGERI")
                },
                cancellationToken);

            return ApiResponse<List<KurDto>>.SuccessResult(rows, _localizationService.GetLocalizedString("OperationSuccessful"));
        }
        catch (Exception ex)
        {
            var message = _localizationService.GetLocalizedString("ExchangeRatesRetrievalError", ex.Message);
            return ApiResponse<List<KurDto>>.ErrorResult(message, ex.Message, 500);
        }
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
