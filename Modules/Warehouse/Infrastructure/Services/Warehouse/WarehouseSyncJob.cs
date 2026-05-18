using Wms.Application.Warehouse.Dtos;
using Wms.Application.Warehouse.Services;
using Wms.Application.Common;
using Hangfire;

namespace Wms.Infrastructure.Services.Warehouse;

[DisableConcurrentExecution(timeoutInSeconds: 300)]
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 }, LogEvents = true)]
public sealed class WarehouseSyncJob : IWarehouseSyncJob
{
    private const string RecurringJobId = "erp-warehouse-sync-job";
    private readonly IWarehouseService _warehouseService;
    private readonly IWarehouseErpReadService _warehouseErpReadService;
    private readonly IJobFailureLogWriter _jobFailureLogWriter;
    private readonly ILogger<WarehouseSyncJob> _logger;

    public WarehouseSyncJob(
        IWarehouseService warehouseService,
        IWarehouseErpReadService warehouseErpReadService,
        IJobFailureLogWriter jobFailureLogWriter,
        ILogger<WarehouseSyncJob> logger)
    {
        _warehouseService = warehouseService;
        _warehouseErpReadService = warehouseErpReadService;
        _jobFailureLogWriter = jobFailureLogWriter;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await _warehouseErpReadService.GetAllAsync(cancellationToken);
            var payload = rows
                .Where(x => x.DepoKodu.HasValue)
                .Select(x => new SyncWarehouseDto
                {
                    WarehouseCode = x.DepoKodu!.Value,
                    WarehouseName = string.IsNullOrWhiteSpace(x.DepoIsmi) ? x.DepoKodu!.Value.ToString() : x.DepoIsmi.Trim()
                })
                .ToList();

            _logger.LogInformation("Warehouse sync job started. Count: {Count}", payload.Count);

            var result = await _warehouseService.WarehouseSyncAsync(payload, cancellationToken);
            if (!result.Success)
            {
                var ex = new InvalidOperationException(result.ExceptionMessage ?? result.Message ?? "Warehouse sync failed.");
                await _jobFailureLogWriter.WriteAsync(
                    $"{RecurringJobId}:SYNC:{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    $"{typeof(WarehouseSyncJob).FullName}.RunAsync",
                    "WarehouseSync",
                    ex,
                    cancellationToken: cancellationToken);
                _logger.LogError("Warehouse sync job failed. StatusCode: {StatusCode}, Message: {Message}, Exception: {ExceptionMessage}", result.StatusCode, result.Message, result.ExceptionMessage);
                throw ex;
            }

            _logger.LogInformation("Warehouse sync job completed. InsertedCount: {InsertedCount}", result.Data);
            return result.Data;
        }
        catch (Exception ex)
        {
            await _jobFailureLogWriter.WriteAsync(
                $"{RecurringJobId}:ERP_FETCH:{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                $"{typeof(WarehouseSyncJob).FullName}.RunAsync",
                "WarehouseSyncJobUnhandled",
                ex,
                cancellationToken: cancellationToken);
            throw;
        }
    }
}
