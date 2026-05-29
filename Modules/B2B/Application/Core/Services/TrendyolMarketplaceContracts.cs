namespace Wms.Application.B2B.Services;

public sealed record TrendyolCredentials(string SupplierId, string ApiKey, string ApiSecret);

public sealed record TrendyolClientResult(
    bool Success,
    string? BatchRequestId,
    string RequestJson,
    string ResponseJson,
    int StatusCode,
    string? ErrorMessage);

public sealed record TrendyolBatchStatusResult(
    bool IsFinal,
    bool Success,
    string ResponseJson,
    int StatusCode,
    string? ErrorMessage);
