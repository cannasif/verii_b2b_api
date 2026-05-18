using Wms.Application.YapKod.Dtos;
using Wms.Application.YapKod.Services;
using Wms.Application.Common;
using Hangfire;

namespace Wms.Infrastructure.Services.YapKod;

[DisableConcurrentExecution(timeoutInSeconds: 300)]
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 }, LogEvents = true)]
public sealed class YapKodSyncJob : IYapKodSyncJob
{
    private const string RecurringJobId = "erp-yapkod-sync-job";
    private readonly IYapKodService _yapKodService;
    private readonly IYapKodErpReadService _yapKodErpReadService;
    private readonly IJobFailureLogWriter _jobFailureLogWriter;
    private readonly ILogger<YapKodSyncJob> _logger;

    public YapKodSyncJob(IYapKodService yapKodService, IYapKodErpReadService yapKodErpReadService, IJobFailureLogWriter jobFailureLogWriter, ILogger<YapKodSyncJob> logger)
    {
        _yapKodService = yapKodService;
        _yapKodErpReadService = yapKodErpReadService;
        _jobFailureLogWriter = jobFailureLogWriter;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await _yapKodErpReadService.GetAllAsync(cancellationToken);
            var payload = rows
                .Where(x => !string.IsNullOrWhiteSpace(x.YapKod) && !string.IsNullOrWhiteSpace(x.YapAcik))
                .Select(x => new SyncYapKodDto
                {
                    YapKod = x.YapKod.Trim(),
                    YapAcik = x.YapAcik.Trim(),
                    YplndrStokKod = x.YplndrStokKod?.Trim(),
                    StockId = x.StockId,
                    BranchCode = x.SubeKodu?.ToString() ?? "0"
                })
                .ToList();

            _logger.LogInformation("YapKod sync job started. Count: {Count}", payload.Count);

            var result = await _yapKodService.YapKodSyncAsync(payload, cancellationToken);
            if (!result.Success)
            {
                var ex = new InvalidOperationException(result.ExceptionMessage ?? result.Message ?? "YapKod sync failed.");
                await _jobFailureLogWriter.WriteAsync(
                    $"{RecurringJobId}:SYNC:{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    $"{typeof(YapKodSyncJob).FullName}.RunAsync",
                    "YapKodSync",
                    ex,
                    cancellationToken: cancellationToken);
                _logger.LogError("YapKod sync job failed. StatusCode: {StatusCode}, Message: {Message}, Exception: {ExceptionMessage}", result.StatusCode, result.Message, result.ExceptionMessage);
                throw ex;
            }

            _logger.LogInformation("YapKod sync job completed. InsertedCount: {InsertedCount}", result.Data);
            return result.Data;
        }
        catch (Exception ex)
        {
            await _jobFailureLogWriter.WriteAsync(
                $"{RecurringJobId}:ERP_FETCH:{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                $"{typeof(YapKodSyncJob).FullName}.RunAsync",
                "YapKodSyncJobUnhandled",
                ex,
                cancellationToken: cancellationToken);
            throw;
        }
    }
}
