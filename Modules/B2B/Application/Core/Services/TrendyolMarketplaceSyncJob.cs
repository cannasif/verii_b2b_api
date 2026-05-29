using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

[DisableConcurrentExecution(timeoutInSeconds: 300)]
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 120, 300 }, LogEvents = true)]
public sealed class TrendyolMarketplaceSyncJob : ITrendyolMarketplaceSyncJob
{
    private const string RecurringJobId = "marketplace-trendyol-sync-job";
    private readonly IRepository<MarketplaceSyncEvent> _events;
    private readonly ITrendyolMarketplaceClient _client;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJobFailureLogWriter _jobFailureLogWriter;
    private readonly ILogger<TrendyolMarketplaceSyncJob> _logger;
    private readonly TrendyolMarketplaceOptions _options;

    public TrendyolMarketplaceSyncJob(
        IRepository<MarketplaceSyncEvent> events,
        ITrendyolMarketplaceClient client,
        IUnitOfWork unitOfWork,
        IJobFailureLogWriter jobFailureLogWriter,
        ILogger<TrendyolMarketplaceSyncJob> logger,
        IOptions<TrendyolMarketplaceOptions> options)
    {
        _events = events;
        _client = client;
        _unitOfWork = unitOfWork;
        _jobFailureLogWriter = jobFailureLogWriter;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var processed = 0;
            processed += await SendPendingAsync(cancellationToken);
            processed += await PollQueuedAsync(cancellationToken);
            return processed;
        }
        catch (Exception ex)
        {
            await _jobFailureLogWriter.WriteAsync(
                $"{RecurringJobId}:RUN:{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                $"{typeof(TrendyolMarketplaceSyncJob).FullName}.RunAsync",
                "TrendyolMarketplaceSyncJobUnhandled",
                ex,
                cancellationToken: cancellationToken);
            throw;
        }
    }

    private async Task<int> SendPendingAsync(CancellationToken cancellationToken)
    {
        var take = Math.Clamp(_options.BatchSize, 1, 100);
        var pendingEvents = await BuildBaseQuery(tracking: true)
            .Where(x => x.Status == B2bWorkflowStatuses.Pending && x.OperationType != "OrderImport")
            .OrderBy(x => x.Id)
            .Take(take)
            .ToListAsync(cancellationToken);

        foreach (var item in pendingEvents)
        {
            item.Status = B2bWorkflowStatuses.Processing;
            item.ErrorMessage = null;
            item.SetUpdatedInfo();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var item in pendingEvents)
        {
            var result = await _client.SendAsync(item, cancellationToken);
            item.RequestJson = string.IsNullOrWhiteSpace(result.RequestJson) ? item.RequestJson : result.RequestJson;
            item.ResponseJson = result.ResponseJson;
            item.ExternalBatchId = result.BatchRequestId ?? item.ExternalBatchId;
            item.Status = result.Success ? "Queued" : B2bWorkflowStatuses.Failed;
            item.ErrorMessage = result.ErrorMessage;
            item.RetryCount = result.Success ? item.RetryCount : item.RetryCount + 1;
            item.ProcessedDate = result.Success ? null : DateTimeProvider.Now;
            item.SetUpdatedInfo();

            _logger.LogInformation(
                "Trendyol marketplace event sent EventId={EventId} Operation={OperationType} Status={Status} BatchId={BatchId}",
                item.Id,
                item.OperationType,
                item.Status,
                item.ExternalBatchId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return pendingEvents.Count;
    }

    private async Task<int> PollQueuedAsync(CancellationToken cancellationToken)
    {
        var queuedEvents = await BuildBaseQuery(tracking: true)
            .Where(x => x.Status == "Queued" && x.ExternalBatchId != null)
            .OrderBy(x => x.Id)
            .Take(Math.Clamp(_options.BatchSize, 1, 100))
            .ToListAsync(cancellationToken);

        foreach (var item in queuedEvents)
        {
            var result = await _client.GetBatchStatusAsync(item, cancellationToken);
            item.ResponseJson = result.ResponseJson;
            if (!result.IsFinal)
            {
                item.SetUpdatedInfo();
                continue;
            }

            item.Status = result.Success ? B2bWorkflowStatuses.Completed : B2bWorkflowStatuses.Failed;
            item.ErrorMessage = result.ErrorMessage;
            item.RetryCount = result.Success ? item.RetryCount : item.RetryCount + 1;
            item.ProcessedDate = DateTimeProvider.Now;
            ApplyListingSnapshot(item, result.Success);
            item.SetUpdatedInfo();

            _logger.LogInformation(
                "Trendyol marketplace batch polled EventId={EventId} Operation={OperationType} Status={Status} BatchId={BatchId}",
                item.Id,
                item.OperationType,
                item.Status,
                item.ExternalBatchId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return queuedEvents.Count;
    }

    private IQueryable<MarketplaceSyncEvent> BuildBaseQuery(bool tracking) =>
        _events.Query(tracking)
            .Include(x => x.Channel)
            .Include(x => x.Listing)
            .ThenInclude(x => x!.CatalogProduct)
            .Where(x => !x.IsDeleted && x.Channel != null && x.Channel.ProviderKey == "Trendyol");

    private static void ApplyListingSnapshot(MarketplaceSyncEvent item, bool success)
    {
        if (!success || item.Listing == null)
        {
            return;
        }

        var now = DateTimeProvider.Now;
        item.Listing.Status = item.OperationType == "ProductCreate" ? "Published" : item.Listing.Status;
        item.Listing.ErrorMessage = null;
        if (item.OperationType == "ProductCreate")
        {
            item.Listing.LastProductSyncDate = now;
        }
        else if (item.OperationType == "PriceUpdate")
        {
            item.Listing.LastPriceSyncDate = now;
        }
        else if (item.OperationType == "StockUpdate")
        {
            item.Listing.LastStockSyncDate = now;
        }

        item.Listing.SetUpdatedInfo();
    }
}
