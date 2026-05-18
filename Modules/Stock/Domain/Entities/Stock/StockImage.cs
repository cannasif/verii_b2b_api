using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Stock;

public sealed class StockImage : BaseEntity
{
    public long StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}
