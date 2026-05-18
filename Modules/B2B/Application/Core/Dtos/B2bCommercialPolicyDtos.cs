using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.B2B.Dtos;

public sealed class CustomerPriceListDto : BaseEntityDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long? CustomerId { get; set; }
    public string? CustomerGroupCode { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public List<CustomerPriceListItemDto> Items { get; set; } = new();
}

public sealed class CustomerPriceListItemDto : BaseEntityDto
{
    public long PriceListId { get; set; }
    public long? CustomerId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? DiscountRate { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public sealed class CreateCustomerPriceListDto
{
    [Required, StringLength(80)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(180)] public string Name { get; set; } = string.Empty;
    public long? CustomerId { get; set; }
    [StringLength(80)] public string? CustomerGroupCode { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpsertCustomerPriceListItemDto
{
    public long? Id { get; set; }
    public long? CustomerId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? MinQuantity { get; set; }
    public decimal? DiscountRate { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

public sealed class InventorySnapshotDto : BaseEntityDto
{
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public string? ErpStockCode { get; set; }
    public int? WarehouseCode { get; set; }
    public string? WarehouseName { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    public string? Unit { get; set; }
    public DateTime SnapshotDate { get; set; }
    public DateTime? LastErpSyncDate { get; set; }
}

public sealed class UpsertInventorySnapshotDto
{
    public long? Id { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    [StringLength(80)] public string? ErpStockCode { get; set; }
    public int? WarehouseCode { get; set; }
    [StringLength(180)] public string? WarehouseName { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ReservedQuantity { get; set; }
    [StringLength(30)] public string? Unit { get; set; }
    public DateTime? LastErpSyncDate { get; set; }
}

public sealed class QuoteRequestDto : BaseEntityDto
{
    public string QuoteNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public long? UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public decimal EstimatedTotal { get; set; }
    public string? CustomerNote { get; set; }
    public string? SalesNote { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public List<QuoteRequestLineDto> Lines { get; set; } = new();
}

public sealed class QuoteRequestLineDto : BaseEntityDto
{
    public long QuoteRequestId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public string? RequestedSku { get; set; }
    public string? RequestedName { get; set; }
    public decimal Quantity { get; set; }
    public decimal? TargetUnitPrice { get; set; }
    public decimal? ApprovedUnitPrice { get; set; }
}

public sealed class CreateQuoteRequestDto
{
    public long CustomerId { get; set; }
    public long? UserId { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    [StringLength(1000)] public string? CustomerNote { get; set; }
    public List<CreateQuoteRequestLineDto> Lines { get; set; } = new();
}

public sealed class CreateQuoteRequestLineDto
{
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    [StringLength(120)] public string? RequestedSku { get; set; }
    [StringLength(250)] public string? RequestedName { get; set; }
    public decimal Quantity { get; set; }
    public decimal? TargetUnitPrice { get; set; }
}

public sealed class UpdateQuoteStatusDto
{
    [Required, StringLength(40)] public string Status { get; set; } = string.Empty;
    [StringLength(1000)] public string? SalesNote { get; set; }
}

public sealed class ConvertQuoteToCartDto
{
    public long QuoteId { get; set; }
    public long? UserId { get; set; }
    public bool AllowBackorder { get; set; }
}

public sealed class B2bIntegrationEventDto : BaseEntityDto
{
    public string Direction { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public long? EntityId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ExternalReference { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ProcessedDate { get; set; }
}
