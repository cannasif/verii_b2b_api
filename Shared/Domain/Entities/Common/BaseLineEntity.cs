using System.ComponentModel.DataAnnotations.Schema;

namespace Wms.Domain.Entities.Common;

public abstract class BaseLineEntity : BaseEntity
{
    [NotMapped]
    public string StockCode { get; set; } = string.Empty;
    public long? StockId { get; set; }
    [NotMapped]
    public string? YapKod { get; set; }
    public long? YapKodId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? SiparisMiktar { get; set; }
    public string? Unit { get; set; }
    public string? ErpOrderNo { get; set; }
    public string? ErpOrderId { get; set; }
    public string? Description { get; set; }
}
