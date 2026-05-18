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
}
