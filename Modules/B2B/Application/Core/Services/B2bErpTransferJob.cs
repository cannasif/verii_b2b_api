using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class B2bErpTransferJob : IB2bErpTransferJob
{
    private const int BatchSize = 25;
    private readonly IRepository<B2bIntegrationEvent> _integrationEvents;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<B2bErpTransferJob> _logger;

    public B2bErpTransferJob(IRepository<B2bIntegrationEvent> integrationEvents, IUnitOfWork unitOfWork, ILogger<B2bErpTransferJob> logger)
    {
        _integrationEvents = integrationEvents;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        var events = await _integrationEvents.Query(tracking: true)
            .Where(x => !x.IsDeleted && x.Direction == "Outbound" && x.Status == B2bWorkflowStatuses.Pending)
            .OrderBy(x => x.Id)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        foreach (var item in events)
        {
            item.Status = B2bWorkflowStatuses.Processing;
            item.ErrorMessage = null;
            item.SetUpdatedInfo();

            _logger.LogInformation(
                "B2B ERP transfer event prepared EventId={EventId} EventType={EventType} Entity={EntityName}/{EntityId}",
                item.Id,
                item.EventType,
                item.EntityName,
                item.EntityId);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return events.Count;
    }
}
