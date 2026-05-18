namespace Wms.Application.Common;

public sealed class IntegrationLogWriteRequest
{
    public string? TraceId { get; set; }
    public string IntegrationType { get; set; } = string.Empty;
    public string TargetSystem { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string? RequestMetadata { get; set; }
    public string? ResponseMetadata { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorType { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
    public int DurationMs { get; set; }
}
