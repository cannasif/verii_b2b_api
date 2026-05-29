namespace Wms.Application.B2B.Services;

public sealed class IyzicoOptions
{
    public const string SectionName = "PaymentProviders:Iyzico";

    public string ApiKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://sandbox-api.iyzipay.com";
    public string BinLookupPath { get; set; } = "/payment/bin/check";
    public string InstallmentPath { get; set; } = "/payment/iyzipos/installment";
    public string ThreedsInitializePath { get; set; } = "/payment/3dsecure/initialize";
    public string ThreedsAuthPath { get; set; } = "/payment/3dsecure/auth";
    public string RefundPath { get; set; } = "/payment/refund";
    public string CancelPath { get; set; } = "/payment/cancel";
    public string CallbackUrl { get; set; } = string.Empty;
    public string Locale { get; set; } = "tr";
}
