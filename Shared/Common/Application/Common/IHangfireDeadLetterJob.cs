namespace Wms.Application.Common;

public interface IHangfireDeadLetterJob
{
    Task ProcessAsync(HangfireDeadLetterPayload payload);
}
