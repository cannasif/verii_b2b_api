using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.B2B;

public sealed class CustomerPriceList : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? CustomerId { get; set; }
    public string? CustomerGroupCode { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;

    public List<CustomerPriceListItem> Items { get; set; } = new();
}
