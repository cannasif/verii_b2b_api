using Wms.Application.Stock.Dtos;
using Wms.Application.Stock.Services;
using Wms.Application.Common;
using Hangfire;

namespace Wms.Infrastructure.Services.Stock;

[DisableConcurrentExecution(timeoutInSeconds: 300)]
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 }, LogEvents = true)]
public sealed class StockSyncJob : IStockSyncJob
{
    private const string RecurringJobId = "erp-stock-sync-job";
    private readonly IStockService _stockService;
    private readonly IStockErpReadService _stockErpReadService;
    private readonly IJobFailureLogWriter _jobFailureLogWriter;
    private readonly ILogger<StockSyncJob> _logger;

    public StockSyncJob(IStockService stockService, IStockErpReadService stockErpReadService, IJobFailureLogWriter jobFailureLogWriter, ILogger<StockSyncJob> logger)
    {
        _stockService = stockService;
        _stockErpReadService = stockErpReadService;
        _jobFailureLogWriter = jobFailureLogWriter;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await _stockErpReadService.GetAllAsync(cancellationToken);
            var payload = rows
                .Where(x => !string.IsNullOrWhiteSpace(x.StokKodu))
                .Select(x => new SyncStockDto
                {
                    ErpStockCode = x.StokKodu.Trim(),
                    StockName = string.IsNullOrWhiteSpace(x.StokAdi) ? x.StokKodu.Trim() : x.StokAdi.Trim(),
                    UreticiKodu = x.UreticiKodu?.Trim(),
                    GrupKodu = x.GrupKodu?.Trim(),
                    Kod1 = x.Kod1?.Trim(),
                    Kod2 = x.Kod2?.Trim(),
                    Kod3 = x.Kod3?.Trim(),
                    Kod4 = x.Kod4?.Trim(),
                    Kod5 = x.Kod5?.Trim(),
                    BranchCode = x.SubeKodu?.ToString() ?? "0"
                })
                .ToList();
            _logger.LogInformation("Stock sync job started. Count: {Count}", payload.Count);

            var result = await _stockService.StockSyncAsync(payload, cancellationToken);
            if (!result.Success)
            {
                var ex = new InvalidOperationException(result.ExceptionMessage ?? result.Message ?? "Stock sync failed.");
                await _jobFailureLogWriter.WriteAsync(
                    $"{RecurringJobId}:SYNC:{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    $"{typeof(StockSyncJob).FullName}.RunAsync",
                    "StockSync",
                    ex,
                    cancellationToken: cancellationToken);
                _logger.LogError("Stock sync job failed. StatusCode: {StatusCode}, Message: {Message}, Exception: {ExceptionMessage}", result.StatusCode, result.Message, result.ExceptionMessage);
                throw ex;
            }

            _logger.LogInformation("Stock sync job completed. InsertedCount: {InsertedCount}", result.Data);
            return result.Data;
        }
        catch (Exception ex)
        {
            await _jobFailureLogWriter.WriteAsync(
                $"{RecurringJobId}:ERP_FETCH:{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                $"{typeof(StockSyncJob).FullName}.RunAsync",
                "StockSyncJobUnhandled",
                ex,
                cancellationToken: cancellationToken);
            throw;
        }
    }
}
