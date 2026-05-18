namespace Wms.Domain.Entities.Common;

public sealed class JobFailureLog : BaseEntity
{
    public string? TraceId { get; set; }
    public string JobId { get; set; } = string.Empty;
    public string JobName { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
    public string? Reason { get; set; }
    public string? ExceptionType { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? StackTrace { get; set; }
    public string? Queue { get; set; }
    public int RetryCount { get; set; }
}
