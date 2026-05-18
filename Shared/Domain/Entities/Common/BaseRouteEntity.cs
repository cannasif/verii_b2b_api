using System.ComponentModel.DataAnnotations.Schema;

namespace Wms.Domain.Entities.Common;

public abstract class BaseRouteEntity : BaseEntity
{
    public string ScannedBarcode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? Description { get; set; }
    public string? SerialNo { get; set; }
    public string? SerialNo2 { get; set; }
    public string? SerialNo3 { get; set; }
    public string? SerialNo4 { get; set; }
    public long? SourceWarehouseId { get; set; }
    public long? TargetWarehouseId { get; set; }
    [NotMapped]
    public int? SourceWarehouse { get; set; }
    [NotMapped]
    public int? TargetWarehouse { get; set; }
    public string? SourceCellCode { get; set; }
    public string? TargetCellCode { get; set; }
}
