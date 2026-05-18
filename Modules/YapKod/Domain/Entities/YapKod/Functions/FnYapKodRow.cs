namespace Wms.Domain.Entities.YapKod.Functions;

public sealed class FnYapKodRow
{
    public string YapKod { get; set; } = string.Empty;
    public string YapAcik { get; set; } = string.Empty;
    public short? SubeKodu { get; set; }
    public string? YplndrStokKod { get; set; }
    public long? StockId { get; set; }
}
