using Wms.Application.Customer.Dtos;
using Wms.Application.Customer.Services;
using Wms.Application.Common;
using Hangfire;

namespace Wms.Infrastructure.Services.Customer;

[DisableConcurrentExecution(timeoutInSeconds: 300)]
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 }, LogEvents = true)]
public sealed class CustomerSyncJob : ICustomerSyncJob
{
    private const string RecurringJobId = "erp-customer-sync-job";
    private readonly ICustomerService _customerService;
    private readonly ICustomerErpReadService _customerErpReadService;
    private readonly IJobFailureLogWriter _jobFailureLogWriter;
    private readonly ILogger<CustomerSyncJob> _logger;

    public CustomerSyncJob(ICustomerService customerService, ICustomerErpReadService customerErpReadService, IJobFailureLogWriter jobFailureLogWriter, ILogger<CustomerSyncJob> logger)
    {
        _customerService = customerService;
        _customerErpReadService = customerErpReadService;
        _jobFailureLogWriter = jobFailureLogWriter;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rows = await _customerErpReadService.GetAllAsync(cancellationToken);
            var payload = rows
                .Where(x => !string.IsNullOrWhiteSpace(x.CariKod))
                .Select(x => new SyncCustomerDto
                {
                    CustomerCode = x.CariKod.Trim(),
                    CustomerName = string.IsNullOrWhiteSpace(x.CariIsim) ? x.CariKod.Trim() : x.CariIsim.Trim(),
                    BranchCode = x.SubeKodu?.ToString() ?? "0",
                    BusinessUnitCode = x.IsletmeKodu
                })
                .ToList();
            _logger.LogInformation("Customer sync job started. Count: {Count}", payload.Count);

            var result = await _customerService.CustomerSyncAsync(payload, cancellationToken);
            if (!result.Success)
            {
                var ex = new InvalidOperationException(result.ExceptionMessage ?? result.Message ?? "Customer sync failed.");
                await _jobFailureLogWriter.WriteAsync(
                    $"{RecurringJobId}:SYNC:{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                    $"{typeof(CustomerSyncJob).FullName}.RunAsync",
                    "CustomerSync",
                    ex,
                    cancellationToken: cancellationToken);
                _logger.LogError("Customer sync job failed. StatusCode: {StatusCode}, Message: {Message}, Exception: {ExceptionMessage}", result.StatusCode, result.Message, result.ExceptionMessage);
                throw ex;
            }

            _logger.LogInformation("Customer sync job completed. InsertedCount: {InsertedCount}", result.Data);
            return result.Data;
        }
        catch (Exception ex)
        {
            await _jobFailureLogWriter.WriteAsync(
                $"{RecurringJobId}:ERP_FETCH:{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                $"{typeof(CustomerSyncJob).FullName}.RunAsync",
                "CustomerSyncJobUnhandled",
                ex,
                cancellationToken: cancellationToken);
            throw;
        }
    }
}
