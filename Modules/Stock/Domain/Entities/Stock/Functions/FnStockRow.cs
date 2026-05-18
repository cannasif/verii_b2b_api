namespace Wms.Domain.Entities.Stock.Functions;

public sealed class FnStockRow
{
    public short? SubeKodu { get; set; }
    public short? IsletmeKodu { get; set; }
    public string StokKodu { get; set; } = string.Empty;
    public string? UreticiKodu { get; set; }
    public string? StokAdi { get; set; }
    public string? GrupKodu { get; set; }
    public string? Kod1 { get; set; }
    public string? Kod2 { get; set; }
    public string? Kod3 { get; set; }
    public string? Kod4 { get; set; }
    public string? Kod5 { get; set; }
}
