namespace Wms.Application.B2B.Services;

internal static class B2bWorkflowStatuses
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string WaitingPayment = "WaitingPayment";
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string ConvertedToCart = "ConvertedToCart";
    public const string Processing = "Processing";
    public const string Completed = "Completed";
    public const string Failed = "Failed";

    public static readonly HashSet<string> QuoteStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        Submitted,
        Approved,
        Rejected,
        Cancelled,
        ConvertedToCart
    };

    public static readonly HashSet<string> PaymentStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        Pending,
        Processing,
        Completed,
        Failed,
        Cancelled
    };

    public static string NormalizeRequired(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}
