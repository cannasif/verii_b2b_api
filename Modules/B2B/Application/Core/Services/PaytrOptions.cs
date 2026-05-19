namespace Wms.Application.B2B.Services;

public sealed class PaytrOptions
{
    public const string SectionName = "PaymentProviders:PayTR";

    public string MerchantId { get; set; } = string.Empty;
    public string MerchantKey { get; set; } = string.Empty;
    public string MerchantSalt { get; set; } = string.Empty;
    public string TokenUrl { get; set; } = "https://www.paytr.com/odeme/api/get-token";
    public string IframeBaseUrl { get; set; } = "https://www.paytr.com/odeme/guvenli";
    public string OkUrl { get; set; } = string.Empty;
    public string FailUrl { get; set; } = string.Empty;
    public bool TestMode { get; set; } = true;
    public bool DebugOn { get; set; } = true;
    public bool NoInstallment { get; set; } = false;
    public int MaxInstallment { get; set; }
    public int TimeoutLimit { get; set; } = 30;
    public string Lang { get; set; } = "tr";
    public bool IframeV2 { get; set; } = true;
    public bool IframeV2Dark { get; set; }
}
