using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;

namespace Wms.Application.B2B.Dtos;

public sealed class CatalogProductDto : BaseEntityDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? CategoryPath { get; set; }
    public string? Description { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public long? DefaultStockId { get; set; }
    public DateTime? PublishedDate { get; set; }
    public List<CatalogVariantDto> Variants { get; set; } = new();
}

public sealed class CatalogVariantDto : BaseEntityDto
{
    public long CatalogProductId { get; set; }
    public long? ErpStockId { get; set; }
    public string VariantSku { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string? AttributesJson { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateCatalogProductDto
{
    [Required, StringLength(80)] public string Sku { get; set; } = string.Empty;
    [Required, StringLength(250)] public string Name { get; set; } = string.Empty;
    [StringLength(250)] public string? Slug { get; set; }
    [StringLength(120)] public string? Brand { get; set; }
    [StringLength(500)] public string? CategoryPath { get; set; }
    public string? Description { get; set; }
    [StringLength(500)] public string? PrimaryImageUrl { get; set; }
    public bool IsPublished { get; set; }
    public long? DefaultStockId { get; set; }
}

public sealed class UpdateCatalogProductDto
{
    [StringLength(80)] public string? Sku { get; set; }
    [StringLength(250)] public string? Name { get; set; }
    [StringLength(250)] public string? Slug { get; set; }
    [StringLength(120)] public string? Brand { get; set; }
    [StringLength(500)] public string? CategoryPath { get; set; }
    public string? Description { get; set; }
    [StringLength(500)] public string? PrimaryImageUrl { get; set; }
    public bool? IsPublished { get; set; }
    public long? DefaultStockId { get; set; }
}

public sealed class UpsertCatalogVariantDto
{
    public long? Id { get; set; }
    public long? ErpStockId { get; set; }
    [Required, StringLength(80)] public string VariantSku { get; set; } = string.Empty;
    [Required, StringLength(250)] public string VariantName { get; set; } = string.Empty;
    public string? AttributesJson { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CustomerProductAliasDto : BaseEntityDto
{
    public long CustomerId { get; set; }
    public long? ErpStockId { get; set; }
    public long? CatalogProductId { get; set; }
    public string CustomerSku { get; set; } = string.Empty;
    public string? CustomerProductName { get; set; }
    public string MatchStatus { get; set; } = string.Empty;
    public decimal? ConfidenceScore { get; set; }
    public string? Notes { get; set; }
    public DateTime? MatchedDate { get; set; }
}

public sealed class CreateCustomerProductAliasDto
{
    public long CustomerId { get; set; }
    public long? ErpStockId { get; set; }
    public long? CatalogProductId { get; set; }
    [Required, StringLength(120)] public string CustomerSku { get; set; } = string.Empty;
    [StringLength(250)] public string? CustomerProductName { get; set; }
    [StringLength(40)] public string MatchStatus { get; set; } = "Pending";
    public decimal? ConfidenceScore { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
}

public sealed class UpdateCustomerProductAliasDto
{
    public long? ErpStockId { get; set; }
    public long? CatalogProductId { get; set; }
    [StringLength(250)] public string? CustomerProductName { get; set; }
    [StringLength(40)] public string? MatchStatus { get; set; }
    public decimal? ConfidenceScore { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
}

public sealed class CartDto : BaseEntityDto
{
    public long CustomerId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public List<CartLineDto> Lines { get; set; } = new();
}

public sealed class CartLineDto : BaseEntityDto
{
    public long CartId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public int? WarehouseCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? PriceSource { get; set; }
    public long? PriceListId { get; set; }
    public decimal? DiscountRate { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal ExchangeRate { get; set; } = 1;
    public DateTime? PriceResolvedAt { get; set; }
}

public sealed class AddCartLineDto
{
    public long CustomerId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    [StringLength(80)] public string? CustomerGroupCode { get; set; }
    [StringLength(120)] public string? CustomerSku { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public int? WarehouseCode { get; set; }
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    public bool AllowBackorder { get; set; }
}

public sealed class QuickOrderLineDto
{
    [StringLength(120)] public string? CustomerSku { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public int? WarehouseCode { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class QuickOrderDto
{
    public long CustomerId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    [StringLength(80)] public string? CustomerGroupCode { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    public bool AllowBackorder { get; set; }
    public List<QuickOrderLineDto> Lines { get; set; } = new();
}

public sealed class QuickOrderLineResultDto
{
    public int LineNumber { get; set; }
    public string? CustomerSku { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public decimal Quantity { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class QuickOrderResultDto
{
    public CartDto? Cart { get; set; }
    public int RequestedLineCount { get; set; }
    public int AddedLineCount { get; set; }
    public List<QuickOrderLineResultDto> Lines { get; set; } = new();
}

public sealed class UpdateCartLineDto
{
    public decimal Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
}

public sealed class OrderDto : BaseEntityDto
{
    public string OrderNumber { get; set; } = string.Empty;
    public long CustomerId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "TRY";
    public string? OfferType { get; set; }
    public DateTime? OfferDate { get; set; }
    public string? OfferNo { get; set; }
    public string? RevisionNo { get; set; }
    public long? RevisionId { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public string? DeliveryMethod { get; set; }
    public long? PaymentTypeId { get; set; }
    public long? QuoteRequestId { get; set; }
    public string? ErpProjectCode { get; set; }
    public decimal? GeneralDiscountRate { get; set; }
    public decimal? GeneralDiscountAmount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Description { get; set; }
    public string? ExternalErpOrderNumber { get; set; }
    public DateTime? SubmittedDate { get; set; }
    public List<OrderLineDto> Lines { get; set; } = new();
}

public sealed class OrderLineDto : BaseEntityDto
{
    public long OrderId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public int? WarehouseCode { get; set; }
    public string? ProductSku { get; set; }
    public string? ProductName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountRate1 { get; set; }
    public decimal DiscountAmount1 { get; set; }
    public decimal DiscountRate2 { get; set; }
    public decimal DiscountAmount2 { get; set; }
    public decimal DiscountRate3 { get; set; }
    public decimal DiscountAmount3 { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal LineTotal { get; set; }
    public decimal LineGrandTotal { get; set; }
    public string? PriceSource { get; set; }
    public long? PriceListId { get; set; }
    public decimal ExchangeRate { get; set; } = 1;
    public DateTime? PriceResolvedAt { get; set; }
    public string? Description { get; set; }
    public string? Description1 { get; set; }
    public string? Description2 { get; set; }
    public string? Description3 { get; set; }
    public long? PricingRuleHeaderId { get; set; }
    public string? RelatedProductKey { get; set; }
    public bool IsMainRelatedProduct { get; set; }
    public string? ErpProjectCode { get; set; }
}

public sealed class CreateOrderFromCartDto
{
    public long CartId { get; set; }
    public decimal TaxTotal { get; set; }
    [StringLength(40)] public string? OfferType { get; set; }
    public DateTime? OfferDate { get; set; }
    [StringLength(60)] public string? OfferNo { get; set; }
    [StringLength(30)] public string? RevisionNo { get; set; }
    public long? RevisionId { get; set; }
    public DateTime? ValidUntil { get; set; }
    public DateTime? DeliveryDate { get; set; }
    [StringLength(120)] public string? DeliveryMethod { get; set; }
    public long? PaymentTypeId { get; set; }
    public long? QuoteRequestId { get; set; }
    [StringLength(50)] public string? ErpProjectCode { get; set; }
    public decimal? GeneralDiscountRate { get; set; }
    public decimal? GeneralDiscountAmount { get; set; }
    [StringLength(1000)] public string? Description { get; set; }
}

public sealed class ReorderDto
{
    public long OrderId { get; set; }
    public long? UserId { get; set; }
    public bool AllowBackorder { get; set; }
}

public sealed class CustomerPortalSummaryDto
{
    public long CustomerId { get; set; }
    public int DraftCartCount { get; set; }
    public int OrderCount { get; set; }
    public int OpenOrderCount { get; set; }
    public int QuoteCount { get; set; }
    public int PendingQuoteCount { get; set; }
    public int PendingPaymentCount { get; set; }
    public decimal OpenOrderTotal { get; set; }
    public decimal PendingPaymentTotal { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public CartDto? DraftCart { get; set; }
    public List<OrderDto> RecentOrders { get; set; } = new();
    public List<PaymentTransactionDto> PendingPayments { get; set; } = new();
}

public sealed class PaymentTransactionDto : BaseEntityDto
{
    public long OrderId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string? ExternalTransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? PaymentMethod { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public sealed class CreatePaymentTransactionDto
{
    public long OrderId { get; set; }
    [Required, StringLength(80)] public string ProviderKey { get; set; } = string.Empty;
    [StringLength(160)] public string? ExternalTransactionId { get; set; }
    public decimal Amount { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    [StringLength(80)] public string? PaymentMethod { get; set; }
}

public sealed class UpdatePaymentStatusDto
{
    [Required, StringLength(40)] public string Status { get; set; } = string.Empty;
    [StringLength(160)] public string? ExternalTransactionId { get; set; }
    public string? CallbackPayloadJson { get; set; }
}
