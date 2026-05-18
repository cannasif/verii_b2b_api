using System.ComponentModel.DataAnnotations;

namespace Wms.Application.B2B.Dtos;

public sealed class ResolveB2bPriceAvailabilityDto
{
    public long CustomerId { get; set; }
    [StringLength(80)] public string? CustomerGroupCode { get; set; }
    [StringLength(120)] public string? CustomerSku { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public int? WarehouseCode { get; set; }
    public decimal Quantity { get; set; } = 1;
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    public DateTime? RequestedDate { get; set; }
}

public sealed class B2bPriceAvailabilityDto
{
    public long CustomerId { get; set; }
    public string? CustomerGroupCode { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public string? ResolvedSku { get; set; }
    public string? ResolvedName { get; set; }
    public decimal RequestedQuantity { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public bool IsPriceResolved { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? DiscountRate { get; set; }
    public decimal? LineTotal { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal ExchangeRate { get; set; } = 1;
    public long? PriceListId { get; set; }
    public string? PriceListCode { get; set; }
    public string? PriceSource { get; set; }
    public DateTime? PriceResolvedAt { get; set; }
    public bool IsAvailable { get; set; }
    public decimal AvailableToSell { get; set; }
    public decimal ReservedQuantity { get; set; }
    public int? PreferredWarehouseCode { get; set; }
    public DateTime? InventorySnapshotDate { get; set; }
    public List<B2bWarehouseAvailabilityDto> Warehouses { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public sealed class B2bWarehouseAvailabilityDto
{
    public int? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public decimal AvailableToSell { get; set; }
    public string? Unit { get; set; }
    public DateTime SnapshotDate { get; set; }
    public DateTime? LastErpSyncDate { get; set; }
}
