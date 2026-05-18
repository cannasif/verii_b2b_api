using Microsoft.Data.SqlClient;
using System.Diagnostics;
using Wms.Application.Common;
using Wms.Application.YapKod.Services;
using Wms.Domain.Entities.YapKod.Functions;

namespace Wms.Infrastructure.Services.YapKod;

public sealed class YapKodErpReadService : IYapKodErpReadService
{
    private readonly IConfiguration _configuration;
    private readonly IRequestTraceAccessor _requestTraceAccessor;
    private readonly IIntegrationLogWriter _integrationLogWriter;
    private readonly ILogger<YapKodErpReadService> _logger;

    public YapKodErpReadService(
        IConfiguration configuration,
        IRequestTraceAccessor requestTraceAccessor,
        IIntegrationLogWriter integrationLogWriter,
        ILogger<YapKodErpReadService> logger)
    {
        _configuration = configuration;
        _requestTraceAccessor = requestTraceAccessor;
        _integrationLogWriter = integrationLogWriter;
        _logger = logger;
    }

    public async Task<List<FnYapKodRow>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection")
            ?? _configuration.GetConnectionString("ErpConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        var result = new List<FnYapKodRow>();
        var startedAt = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("ERP yapkod fetch started TraceId={TraceId}", _requestTraceAccessor.TraceId);
        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM dbo.RII_FN_ESNYAPMAS()";

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                result.Add(new FnYapKodRow
                {
                    YapKod = GetNullableString(reader, "YAPKOD") ?? string.Empty,
                    YapAcik = GetNullableString(reader, "YAPACIK") ?? string.Empty,
                    SubeKodu = GetNullableValue<short>(reader, "SUBE_KODU"),
                    YplndrStokKod = GetNullableString(reader, "YPLNDRSTOKKOD"),
                    StockId = GetNullableValue<long>(reader, "StockId")
                });
            }

            stopwatch.Stop();
            _logger.LogInformation("ERP yapkod fetch completed TraceId={TraceId} RowCount={RowCount}", _requestTraceAccessor.TraceId, result.Count);
            await _integrationLogWriter.TryWriteAsync(new IntegrationLogWriteRequest
            {
                TraceId = _requestTraceAccessor.TraceId,
                IntegrationType = "ERP",
                TargetSystem = "Netsis",
                Operation = "RII_FN_ESNYAPMAS",
                Status = "Succeeded",
                Source = "YapKodErpReadService.GetAllAsync",
                RequestMetadata = "{\"parameterCount\":0}",
                ResponseMetadata = $"{{\"rowCount\":{result.Count}}}",
                StartedAt = startedAt,
                FinishedAt = DateTime.UtcNow,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
            }, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            await _integrationLogWriter.TryWriteAsync(new IntegrationLogWriteRequest
            {
                TraceId = _requestTraceAccessor.TraceId,
                IntegrationType = "ERP",
                TargetSystem = "Netsis",
                Operation = "RII_FN_ESNYAPMAS",
                Status = "Failed",
                Source = "YapKodErpReadService.GetAllAsync",
                RequestMetadata = "{\"parameterCount\":0}",
                ErrorMessage = ex.Message,
                ErrorType = ex.GetType().FullName,
                StartedAt = startedAt,
                FinishedAt = DateTime.UtcNow,
                DurationMs = (int)stopwatch.ElapsedMilliseconds,
            }, cancellationToken);
            throw;
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
