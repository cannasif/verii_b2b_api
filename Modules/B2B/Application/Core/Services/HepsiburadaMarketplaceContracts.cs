namespace Wms.Application.B2B.Services;

public sealed record HepsiburadaCredentials(string MerchantId, string ApiKey, string ApiSecret);

public sealed record HepsiburadaClientResult(
    bool Success,
    string? TransactionId,
    string RequestJson,
    string ResponseJson,
    int StatusCode,
    string? ErrorMessage);

public sealed record HepsiburadaTransactionStatusResult(
    bool IsFinal,
    bool Success,
    string ResponseJson,
    int StatusCode,
    string? ErrorMessage);
