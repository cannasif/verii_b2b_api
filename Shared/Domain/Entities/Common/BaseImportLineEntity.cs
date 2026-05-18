using System.ComponentModel.DataAnnotations.Schema;

namespace Wms.Domain.Entities.Common;

public abstract class BaseImportLineEntity : BaseEntity
{
    [NotMapped]
    public string StockCode { get; set; } = string.Empty;
    public long? StockId { get; set; }
    [NotMapped]
    public string? YapKod { get; set; }
    public long? YapKodId { get; set; }
    public string? Description1 { get; set; }
    public string? Description2 { get; set; }
    public string? Description { get; set; }
}
