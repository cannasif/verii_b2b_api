using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.YapKod;

public sealed class YapKod : BaseEntity
{
    public string YapKodCode { get; set; } = string.Empty;
    public string YapAcik { get; set; } = string.Empty;
    public string? YplndrStokKod { get; set; }
    public long? StockId { get; set; }
    public DateTime? LastSyncDate { get; set; }
}
