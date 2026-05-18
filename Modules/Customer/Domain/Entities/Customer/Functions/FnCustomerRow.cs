namespace Wms.Domain.Entities.Customer.Functions;

public sealed class FnCustomerRow
{
    public short? SubeKodu { get; set; }
    public short? IsletmeKodu { get; set; }
    public string CariKod { get; set; } = string.Empty;
    public string? CariIsim { get; set; }
}
