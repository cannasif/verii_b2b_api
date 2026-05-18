namespace Wms.Domain.Entities.Common;

public abstract class BaseLineSerialEntity : BaseEntity
{
    public decimal Quantity { get; set; }
    public string? SerialNo { get; set; }
    public string? SerialNo2 { get; set; }
    public string? SerialNo3 { get; set; }
    public string? SerialNo4 { get; set; }
    public long? SourceWarehouseId { get; set; }
    public long? TargetWarehouseId { get; set; }
    public string? SourceCellCode { get; set; }
    public string? TargetCellCode { get; set; }
}
