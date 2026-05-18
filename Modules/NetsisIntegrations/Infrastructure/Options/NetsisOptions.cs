namespace Wms.Modules.NetsisIntegrations.Infrastructure.Options;

public sealed class NetsisOptions
{
    public const string SectionName = "Netsis";

    public bool Enabled { get; set; }
    public NetsisRestOptions Rest { get; set; } = new();
}

public sealed class NetsisRestOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string LoginPath { get; set; } = "/api/v2/token";
    public string ItemSlipsPath { get; set; } = "/api/v2/ItemSlips";
    public int TimeoutSeconds { get; set; } = 30;
    public bool AllowInvalidSslCertificate { get; set; }
    public int DefaultTokenLifetimeMinutes { get; set; } = 60;
    public int TokenExpirySkewSeconds { get; set; } = 30;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BranchCode { get; set; } = string.Empty;
    public string DbName { get; set; } = string.Empty;
    public string DbUser { get; set; } = string.Empty;
    public string DbPassword { get; set; } = string.Empty;
    public string DbType { get; set; } = "0";
    public int? WarehouseTransferDocumentType { get; set; }
    public int? WarehouseInboundDocumentType { get; set; }
    public int? WarehouseOutboundDocumentType { get; set; }
    public int? ShipmentDocumentType { get; set; }
    public int? ProductionTransferDocumentType { get; set; }
}
