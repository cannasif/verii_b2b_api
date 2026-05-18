using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Stock;

public sealed class StockDetail : BaseEntity
{
    public long StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public string HtmlDescription { get; set; } = string.Empty;
    public string? TechnicalSpecsJson { get; set; }
}
