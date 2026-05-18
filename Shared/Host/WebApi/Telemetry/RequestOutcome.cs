namespace Wms.WebApi.Telemetry;

public static class RequestOutcome
{
    public const string Succeeded = "Succeeded";
    public const string ValidationFailed = "ValidationFailed";
    public const string Unauthorized = "Unauthorized";
    public const string Forbidden = "Forbidden";
    public const string NotFound = "NotFound";
    public const string Conflict = "Conflict";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string UnhandledException = "UnhandledException";
}
