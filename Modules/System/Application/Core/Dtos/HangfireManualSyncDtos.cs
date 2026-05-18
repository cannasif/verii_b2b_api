namespace Wms.Application.System.Dtos;

public sealed class ManualSyncJobStatusDto
{
    public string JobKey { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public DateTime? LastTriggeredAtUtc { get; set; }
    public DateTime? NextAvailableAtUtc { get; set; }
    public bool IsCoolingDown { get; set; }
    public int CooldownSecondsRemaining { get; set; }
}

public sealed class TriggerManualSyncJobRequestDto
{
    public string JobKey { get; set; } = string.Empty;
}

public sealed class TriggerManualSyncJobResponseDto
{
    public string JobKey { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public string JobId { get; set; } = string.Empty;
    public string Queue { get; set; } = "default";
    public DateTime EnqueuedAtUtc { get; set; }
    public DateTime? NextAvailableAtUtc { get; set; }
    public int CooldownSecondsRemaining { get; set; }
}
