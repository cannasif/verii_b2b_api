using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Stock;

public sealed class Stock : BaseEntity
{
    public string ErpStockCode { get; set; } = string.Empty;
    public string StockName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string? UreticiKodu { get; set; }
    public string? GrupKodu { get; set; }
    public string? GrupAdi { get; set; }
    public string? Kod1 { get; set; }
    public string? Kod1Adi { get; set; }
    public string? Kod2 { get; set; }
    public string? Kod2Adi { get; set; }
    public string? Kod3 { get; set; }
    public string? Kod3Adi { get; set; }
    public string? Kod4 { get; set; }
    public string? Kod4Adi { get; set; }
    public string? Kod5 { get; set; }
    public string? Kod5Adi { get; set; }
    public DateTime? LastSyncDate { get; set; }
}
