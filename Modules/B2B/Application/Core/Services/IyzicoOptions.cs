namespace Wms.Application.B2B.Services;

public sealed class IyzicoOptions
{
    public const string SectionName = "PaymentProviders:Iyzico";

    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";
    public string BinLookupPath { get; set; } = "/payment/bin/check";
    public string InstallmentPath { get; set; } = "/payment/iyzipos/installment";
    public string Locale { get; set; } = "tr";
}
