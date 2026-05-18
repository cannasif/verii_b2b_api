using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class B2bCart : BaseEntity
{
    public long CustomerId { get; set; }
    public long? UserId { get; set; }
    public string Status { get; set; } = "Draft";
    public string CurrencyCode { get; set; } = "TRY";

    public List<B2bCartLine> Lines { get; set; } = new();
}
