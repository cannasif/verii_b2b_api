using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Wms.Application.Common;

namespace Wms.Application.B2B.Dtos;

public sealed class CatalogProductDto : BaseEntityDto
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ProductType { get; set; }
    public string? ManufacturerCode { get; set; }
    public string? Barcode { get; set; }
    public string? Unit { get; set; }
    public string? CategoryPath { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string? BulletPointsJson { get; set; }
    public string? AttributesJson { get; set; }
    public string? MediaGalleryJson { get; set; }
    public string? DocumentsJson { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? SearchKeywords { get; set; }
    public decimal? MinOrderQuantity { get; set; }
    public decimal? PackageQuantity { get; set; }
    public int SortOrder { get; set; }
    public int CompletenessScore { get; set; }
    public bool IsPublished { get; set; }
    public long? DefaultStockId { get; set; }
    public DateTime? PublishedDate { get; set; }
    public List<CatalogVariantDto> Variants { get; set; } = new();
    public List<CatalogProductCategoryDto> Categories { get; set; } = new();
    public List<CatalogProductAttributeDto> Attributes { get; set; } = new();
    public List<CatalogProductMediaDto> MediaItems { get; set; } = new();
    public List<CatalogProductDocumentDto> Documents { get; set; } = new();
}

public sealed class CatalogVariantDto : BaseEntityDto
{
    public long CatalogProductId { get; set; }
    public long? ErpStockId { get; set; }
    public string VariantSku { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? Unit { get; set; }
    public string? AttributesJson { get; set; }
    public string? MediaGalleryJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateCatalogProductDto
{
    [Required, StringLength(80)] public string Sku { get; set; } = string.Empty;
    [Required, StringLength(250)] public string Name { get; set; } = string.Empty;
    [StringLength(250)] public string? Slug { get; set; }
    [StringLength(120)] public string? Brand { get; set; }
    [StringLength(120)] public string? ProductType { get; set; }
    [StringLength(120)] public string? ManufacturerCode { get; set; }
    [StringLength(80)] public string? Barcode { get; set; }
    [StringLength(30)] public string? Unit { get; set; }
    [StringLength(500)] public string? CategoryPath { get; set; }
    [StringLength(500)] public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    [StringLength(500)] public string? PrimaryImageUrl { get; set; }
    public string? BulletPointsJson { get; set; }
    public string? AttributesJson { get; set; }
    public string? MediaGalleryJson { get; set; }
    public string? DocumentsJson { get; set; }
    [StringLength(250)] public string? MetaTitle { get; set; }
    [StringLength(500)] public string? MetaDescription { get; set; }
    [StringLength(1000)] public string? SearchKeywords { get; set; }
    public decimal? MinOrderQuantity { get; set; }
    public decimal? PackageQuantity { get; set; }
    public int SortOrder { get; set; }
    public bool IsPublished { get; set; }
    public long? DefaultStockId { get; set; }
}

public sealed class UpdateCatalogProductDto
{
    [StringLength(80)] public string? Sku { get; set; }
    [StringLength(250)] public string? Name { get; set; }
    [StringLength(250)] public string? Slug { get; set; }
    [StringLength(120)] public string? Brand { get; set; }
    [StringLength(120)] public string? ProductType { get; set; }
    [StringLength(120)] public string? ManufacturerCode { get; set; }
    [StringLength(80)] public string? Barcode { get; set; }
    [StringLength(30)] public string? Unit { get; set; }
    [StringLength(500)] public string? CategoryPath { get; set; }
    [StringLength(500)] public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    [StringLength(500)] public string? PrimaryImageUrl { get; set; }
    public string? BulletPointsJson { get; set; }
    public string? AttributesJson { get; set; }
    public string? MediaGalleryJson { get; set; }
    public string? DocumentsJson { get; set; }
    [StringLength(250)] public string? MetaTitle { get; set; }
    [StringLength(500)] public string? MetaDescription { get; set; }
    [StringLength(1000)] public string? SearchKeywords { get; set; }
    public decimal? MinOrderQuantity { get; set; }
    public decimal? PackageQuantity { get; set; }
    public int? SortOrder { get; set; }
    public bool? IsPublished { get; set; }
    public long? DefaultStockId { get; set; }
}

public sealed class UpsertCatalogVariantDto
{
    public long? Id { get; set; }
    public long? ErpStockId { get; set; }
    [Required, StringLength(80)] public string VariantSku { get; set; } = string.Empty;
    [Required, StringLength(250)] public string VariantName { get; set; } = string.Empty;
    [StringLength(80)] public string? Barcode { get; set; }
    [StringLength(30)] public string? Unit { get; set; }
    public string? AttributesJson { get; set; }
    public string? MediaGalleryJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CatalogCategoryDto : BaseEntityDto
{
    public long? ParentCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int Level { get; set; }
    public string? FullPath { get; set; }
    public int SortOrder { get; set; }
    public string? ImageUrl { get; set; }
    public string? IconName { get; set; }
    public string? ColorHex { get; set; }
    public bool IsLeaf { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CatalogProductFavoriteDto : BaseEntityDto
{
    public long CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public long? BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public long? UserId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    public string FavoriteKey { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public string? ProductImageUrl { get; set; }
    public string? Brand { get; set; }
    public string? CategoryPath { get; set; }
    public string? VariantName { get; set; }
    public string? Note { get; set; }
}

public sealed class CatalogCategoryFavoriteDto : BaseEntityDto
{
    public long CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public long? BuyerId { get; set; }
    public string? BuyerName { get; set; }
    public long? UserId { get; set; }
    public long CatalogCategoryId { get; set; }
    public string? CategoryCode { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryFullPath { get; set; }
    public string? ImageUrl { get; set; }
    public string? Note { get; set; }
}

public sealed class ToggleCatalogProductFavoriteDto
{
    public long CompanyId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    public long? CatalogProductId { get; set; }
    public long? CatalogVariantId { get; set; }
    public long? ErpStockId { get; set; }
    [StringLength(80)] public string? Sku { get; set; }
    public bool? IsFavorite { get; set; }
    [StringLength(500)] public string? Note { get; set; }
}

public sealed class ToggleCatalogCategoryFavoriteDto
{
    public long CompanyId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    public long CatalogCategoryId { get; set; }
    public bool? IsFavorite { get; set; }
    [StringLength(500)] public string? Note { get; set; }
}

public sealed class CatalogFavoriteToggleResultDto
{
    public bool IsFavorite { get; set; }
    public long? FavoriteId { get; set; }
    public string FavoriteKey { get; set; } = string.Empty;
}

public sealed class CreateCatalogCategoryDto
{
    public long? ParentCategoryId { get; set; }
    [Required, StringLength(80)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(250)] public string Name { get; set; } = string.Empty;
    [StringLength(1000)] public string? Description { get; set; }
    public int SortOrder { get; set; }
    [StringLength(500)] public string? ImageUrl { get; set; }
    [StringLength(80)] public string? IconName { get; set; }
    [StringLength(20)] public string? ColorHex { get; set; }
    public bool IsLeaf { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class UpdateCatalogCategoryDto
{
    public long? ParentCategoryId { get; set; }
    [StringLength(80)] public string? Code { get; set; }
    [StringLength(250)] public string? Name { get; set; }
    [StringLength(1000)] public string? Description { get; set; }
    public int? SortOrder { get; set; }
    [StringLength(500)] public string? ImageUrl { get; set; }
    [StringLength(80)] public string? IconName { get; set; }
    [StringLength(20)] public string? ColorHex { get; set; }
    public bool? IsLeaf { get; set; }
    public bool? IsActive { get; set; }
}

public sealed class CatalogProductCategoryDto : BaseEntityDto
{
    public long CatalogProductId { get; set; }
    public long CatalogCategoryId { get; set; }
    public string? CategoryCode { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryFullPath { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
    public string? AssignmentSource { get; set; }
}

public sealed class AssignCatalogProductCategoryDto
{
    public long CatalogCategoryId { get; set; }
    public bool IsPrimary { get; set; } = true;
    public int SortOrder { get; set; }
    [StringLength(40)] public string? AssignmentSource { get; set; } = "Manual";
}

public sealed class CatalogAttributeDefinitionDto : BaseEntityDto
{
    public long? CatalogCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsComparable { get; set; }
    public string? Unit { get; set; }
    public string? AllowedValuesJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateCatalogAttributeDefinitionDto
{
    public long? CatalogCategoryId { get; set; }
    [Required, StringLength(80)] public string Code { get; set; } = string.Empty;
    [Required, StringLength(160)] public string Name { get; set; } = string.Empty;
    [StringLength(40)] public string DataType { get; set; } = "Text";
    public bool IsRequired { get; set; }
    public bool IsFilterable { get; set; }
    public bool IsComparable { get; set; }
    [StringLength(30)] public string? Unit { get; set; }
    public string? AllowedValuesJson { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CatalogProductAttributeDto : BaseEntityDto
{
    public long CatalogProductId { get; set; }
    public long AttributeDefinitionId { get; set; }
    public string? AttributeCode { get; set; }
    public string? AttributeName { get; set; }
    public string? DataType { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? NormalizedValue { get; set; }
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
}

public sealed class UpsertCatalogProductAttributeDto
{
    public long AttributeDefinitionId { get; set; }
    [Required, StringLength(1000)] public string Value { get; set; } = string.Empty;
    [StringLength(30)] public string? Unit { get; set; }
    public int SortOrder { get; set; }
}

public sealed class CatalogProductMediaDto : BaseEntityDto
{
    public long CatalogProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string MediaType { get; set; } = "Image";
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public sealed class UpsertCatalogProductMediaDto
{
    public long? Id { get; set; }
    [Required, StringLength(500)] public string Url { get; set; } = string.Empty;
    [StringLength(40)] public string MediaType { get; set; } = "Image";
    [StringLength(250)] public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}

public sealed class UploadCatalogProductMediaDto
{
    public List<IFormFile> Files { get; set; } = new();
    public List<string>? AltTexts { get; set; }
    public bool FirstImageAsPrimary { get; set; } = true;
}

public sealed class CatalogProductDocumentDto : BaseEntityDto
{
    public long CatalogProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string DocumentType { get; set; } = "TechnicalSheet";
    public string? LanguageCode { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}

public sealed class UpsertCatalogProductDocumentDto
{
    public long? Id { get; set; }
    [Required, StringLength(160)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(500)] public string Url { get; set; } = string.Empty;
    [StringLength(60)] public string DocumentType { get; set; } = "TechnicalSheet";
    [StringLength(10)] public string? LanguageCode { get; set; }
    public int SortOrder { get; set; }
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
    public long? PaymentOrderId { get; set; }
    public long? PaymentInstallmentId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string? ExternalTransactionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? ProviderPaymentAmount { get; set; }
    public decimal? ProviderCollectedAmount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? PaymentMethod { get; set; }
    public DateTime? DueDate { get; set; }
    public short? PaymentTermDays { get; set; }
    public int InstallmentCount { get; set; } = 1;
    public string? InstallmentPlanJson { get; set; }
    public string? ProviderConversationId { get; set; }
    public string? BinNumber { get; set; }
    public string? CardType { get; set; }
    public string? CardAssociation { get; set; }
    public string? CardFamily { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public bool? IsCommercialCard { get; set; }
    public decimal? ProviderRate { get; set; }
    public decimal? ProviderCommissionAmount { get; set; }
    public DateTime? RequestedDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public sealed class CreatePaymentTransactionDto
{
    public long OrderId { get; set; }
    public long? PaymentOrderId { get; set; }
    public long? PaymentInstallmentId { get; set; }
    [Required, StringLength(80)] public string ProviderKey { get; set; } = string.Empty;
    [StringLength(160)] public string? ExternalTransactionId { get; set; }
    public decimal Amount { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    [StringLength(80)] public string? PaymentMethod { get; set; }
    public DateTime? DueDate { get; set; }
    public short? PaymentTermDays { get; set; }
    public int InstallmentCount { get; set; } = 1;
    public string? InstallmentPlanJson { get; set; }
}

public sealed class UpdatePaymentStatusDto
{
    [Required, StringLength(40)] public string Status { get; set; } = string.Empty;
    [StringLength(160)] public string? ExternalTransactionId { get; set; }
    public string? CallbackPayloadJson { get; set; }
}

public sealed class CreatePaytrIframeTokenDto
{
    public long OrderId { get; set; }
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(100)] public string UserName { get; set; } = string.Empty;
    [Required, StringLength(400)] public string UserAddress { get; set; } = string.Empty;
    [Required, StringLength(20)] public string UserPhone { get; set; } = string.Empty;
    [StringLength(500)] public string? OkUrl { get; set; }
    [StringLength(500)] public string? FailUrl { get; set; }
    [StringLength(39)] public string? UserIp { get; set; }
}

public sealed class PaytrIframeTokenDto
{
    public long PaymentTransactionId { get; set; }
    public long OrderId { get; set; }
    public string MerchantOid { get; set; } = string.Empty;
    public string IframeToken { get; set; } = string.Empty;
    public string IframeUrl { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public bool TestMode { get; set; }
}

public sealed class CreateIyzico3dsPaymentDto
{
    public long OrderId { get; set; }
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = string.Empty;
    [Required, StringLength(100)] public string BuyerName { get; set; } = string.Empty;
    [Required, StringLength(100)] public string BuyerSurname { get; set; } = string.Empty;
    [Required, StringLength(20)] public string BuyerPhone { get; set; } = string.Empty;
    [Required, StringLength(400)] public string BuyerAddress { get; set; } = string.Empty;
    [Required, StringLength(80)] public string City { get; set; } = string.Empty;
    [Required, StringLength(80)] public string Country { get; set; } = "Türkiye";
    [Required, StringLength(30)] public string CardHolderName { get; set; } = string.Empty;
    [Required, StringLength(19)] public string CardNumber { get; set; } = string.Empty;
    [Required, StringLength(2)] public string ExpireMonth { get; set; } = string.Empty;
    [Required, StringLength(4)] public string ExpireYear { get; set; } = string.Empty;
    [Required, StringLength(4)] public string Cvc { get; set; } = string.Empty;
    public int InstallmentCount { get; set; } = 1;
    [StringLength(500)] public string? CallbackUrl { get; set; }
    [StringLength(39)] public string? BuyerIp { get; set; }
}

public sealed class Iyzico3dsInitializeDto
{
    public long PaymentTransactionId { get; set; }
    public long OrderId { get; set; }
    public string ConversationId { get; set; } = string.Empty;
    public string? PaymentId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ThreeDSHtmlContent { get; set; }
    public string? PaymentPageUrl { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
}

public sealed class PaymentBinLookupRequestDto
{
    [Required, StringLength(20)] public string ProviderKey { get; set; } = string.Empty;
    [Required, StringLength(8, MinimumLength = 6)] public string BinNumber { get; set; } = string.Empty;
    public decimal? Amount { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    [StringLength(120)] public string? ConversationId { get; set; }
}

public sealed class PaymentInstallmentOptionsRequestDto
{
    [Required, StringLength(20)] public string ProviderKey { get; set; } = string.Empty;
    [StringLength(8, MinimumLength = 6)] public string? BinNumber { get; set; }
    [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    [StringLength(120)] public string? ConversationId { get; set; }
}

public sealed class PaymentBinLookupDto
{
    public string ProviderKey { get; set; } = string.Empty;
    public string BinNumber { get; set; } = string.Empty;
    public string? CardType { get; set; }
    public string? CardAssociation { get; set; }
    public string? CardFamily { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public bool? IsCommercial { get; set; }
    public string? ProviderStatus { get; set; }
    public string? ConversationId { get; set; }
    public string? RawResponseJson { get; set; }
}

public sealed class PaymentInstallmentOptionsDto
{
    public string ProviderKey { get; set; } = string.Empty;
    public string? BinNumber { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? ProviderStatus { get; set; }
    public string? ConversationId { get; set; }
    public PaymentBinLookupDto? Card { get; set; }
    public List<PaymentInstallmentOptionDto> Options { get; set; } = new();
    public string? RawResponseJson { get; set; }
}

public sealed class PaymentInstallmentOptionDto
{
    public int InstallmentNumber { get; set; }
    public decimal InstallmentPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal? ProviderRate { get; set; }
    public decimal? CommissionAmount { get; set; }
    public bool IsAvailable { get; set; } = true;
}

public sealed class PaymentOrderDto : BaseEntityDto
{
    public string PaymentOrderNumber { get; set; } = string.Empty;
    public long OrderId { get; set; }
    public long CustomerId { get; set; }
    public long? BuyerId { get; set; }
    public long? UserId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public short? PaymentTermDays { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsDueDateOverridden { get; set; }
    public int InstallmentCount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ProviderKey { get; set; }
    public string? ProviderConversationId { get; set; }
    public string? BinNumber { get; set; }
    public string? CardType { get; set; }
    public string? CardAssociation { get; set; }
    public string? CardFamily { get; set; }
    public string? BankName { get; set; }
    public string? BankCode { get; set; }
    public bool? IsCommercialCard { get; set; }
    public int? ProviderInstallmentNumber { get; set; }
    public decimal? ProviderInstallmentPrice { get; set; }
    public decimal? ProviderTotalPrice { get; set; }
    public decimal? ProviderRate { get; set; }
    public decimal? ProviderCommissionAmount { get; set; }
    public string? ProviderInstallmentSnapshotJson { get; set; }
    public string? Notes { get; set; }
    public List<PaymentInstallmentDto> Installments { get; set; } = new();
}

public sealed class SelectPaymentProviderInstallmentDto
{
    [Required, StringLength(20)] public string ProviderKey { get; set; } = string.Empty;
    [StringLength(120)] public string? ProviderConversationId { get; set; }
    [StringLength(8, MinimumLength = 6)] public string? BinNumber { get; set; }
    [StringLength(40)] public string? CardType { get; set; }
    [StringLength(60)] public string? CardAssociation { get; set; }
    [StringLength(120)] public string? CardFamily { get; set; }
    [StringLength(180)] public string? BankName { get; set; }
    [StringLength(40)] public string? BankCode { get; set; }
    public bool? IsCommercialCard { get; set; }
    [Range(1, 36)] public int InstallmentNumber { get; set; } = 1;
    [Range(0.01, double.MaxValue)] public decimal InstallmentPrice { get; set; }
    [Range(0.01, double.MaxValue)] public decimal TotalPrice { get; set; }
    public decimal? ProviderRate { get; set; }
    public decimal? ProviderCommissionAmount { get; set; }
    public string? ProviderInstallmentSnapshotJson { get; set; }
}

public sealed class PaymentInstallmentDto : BaseEntityDto
{
    public long PaymentOrderId { get; set; }
    public int InstallmentNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaidDate { get; set; }
    public string? Notes { get; set; }
}

public sealed class CreatePaymentOrderDto
{
    public long OrderId { get; set; }
    public short? PaymentTermDays { get; set; }
    public DateTime? DueDate { get; set; }
    [Range(1, 36)] public int InstallmentCount { get; set; } = 1;
    [StringLength(80)] public string? PaymentMethod { get; set; }
    [StringLength(80)] public string? ProviderKey { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
}

public sealed class UpdatePaymentOrderPlanDto
{
    public short? PaymentTermDays { get; set; }
    public DateTime? DueDate { get; set; }
    [Range(1, 36)] public int InstallmentCount { get; set; } = 1;
    [StringLength(80)] public string? PaymentMethod { get; set; }
    [StringLength(80)] public string? ProviderKey { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
}

public sealed class PaymentMethodRuleDto : BaseEntityDto
{
    public long? CompanyId { get; set; }
    public long? CustomerId { get; set; }
    public string? CustomerGroupCode { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string RuleType { get; set; } = "Include";
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Notes { get; set; }
}

public sealed class CreatePaymentMethodRuleDto
{
    public long? CompanyId { get; set; }
    public long? CustomerId { get; set; }
    [StringLength(80)] public string? CustomerGroupCode { get; set; }
    [Required, StringLength(20)] public string ProviderKey { get; set; } = string.Empty;
    [Required, StringLength(80)] public string PaymentMethod { get; set; } = string.Empty;
    [Required, StringLength(20)] public string RuleType { get; set; } = "Include";
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public bool RequiresApproval { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    [StringLength(1000)] public string? Notes { get; set; }
}

public sealed class ResolvePaymentMethodsDto
{
    public long CustomerId { get; set; }
    public long? CompanyId { get; set; }
    [StringLength(80)] public string? CustomerGroupCode { get; set; }
    [Range(0, double.MaxValue)] public decimal Amount { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
}

public sealed class PaymentMethodOptionDto
{
    public string ProviderKey { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool RequiresApproval { get; set; }
    public bool IsProviderHosted { get; set; }
    public bool IsDeferredPayment { get; set; }
}

public sealed class PaymentProviderOperationDto : BaseEntityDto
{
    public long PaymentTransactionId { get; set; }
    public long? PaymentOrderId { get; set; }
    public long? PaymentInstallmentId { get; set; }
    public string ProviderKey { get; set; } = string.Empty;
    public string OperationType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "TRY";
    public string? ExternalOperationId { get; set; }
    public string? IdempotencyKey { get; set; }
    public string? Reason { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime RequestedDate { get; set; }
    public DateTime? ProcessedDate { get; set; }
}

public sealed class CreatePaymentProviderOperationDto
{
    public long PaymentTransactionId { get; set; }
    public long? PaymentInstallmentId { get; set; }
    [Required, StringLength(40)] public string OperationType { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    [StringLength(3)] public string CurrencyCode { get; set; } = "TRY";
    [StringLength(120)] public string? IdempotencyKey { get; set; }
    [StringLength(1000)] public string? Reason { get; set; }
}
