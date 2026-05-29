using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;
using CustomerEntity = Wms.Domain.Entities.Customer.Customer;

namespace Wms.Application.B2B.Services;

public sealed class B2bCommerceService : IB2bCommerceService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly IRepository<CatalogProduct> _catalogProducts;
    private readonly IRepository<CatalogVariant> _catalogVariants;
    private readonly IRepository<CatalogCategory> _catalogCategories;
    private readonly IRepository<CatalogProductCategory> _catalogProductCategories;
    private readonly IRepository<CatalogAttributeDefinition> _catalogAttributeDefinitions;
    private readonly IRepository<CatalogProductAttribute> _catalogProductAttributes;
    private readonly IRepository<CatalogProductMedia> _catalogProductMedia;
    private readonly IRepository<CatalogProductDocument> _catalogProductDocuments;
    private readonly IRepository<CatalogProductFavorite> _catalogProductFavorites;
    private readonly IRepository<CatalogCategoryFavorite> _catalogCategoryFavorites;
    private readonly IRepository<CustomerProductAlias> _aliases;
    private readonly IRepository<B2bCompany> _companies;
    private readonly IRepository<B2bBuyer> _buyers;
    private readonly IRepository<B2bCart> _carts;
    private readonly IRepository<B2bCartLine> _cartLines;
    private readonly IRepository<B2bOrder> _orders;
    private readonly IRepository<B2bOrderLine> _orderLines;
    private readonly IRepository<CustomerEntity> _customers;
    private readonly IRepository<PaymentOrder> _paymentOrders;
    private readonly IRepository<PaymentInstallment> _paymentInstallments;
    private readonly IRepository<PaymentTransaction> _payments;
    private readonly IRepository<QuoteRequest> _quotes;
    private readonly IRepository<InventorySnapshot> _inventory;
    private readonly IB2bPricingAvailabilityResolver _pricingAvailabilityResolver;
    private readonly IFileUploadService _fileUploadService;
    private readonly IUnitOfWork _unitOfWork;

    public B2bCommerceService(
        IRepository<CatalogProduct> catalogProducts,
        IRepository<CatalogVariant> catalogVariants,
        IRepository<CatalogCategory> catalogCategories,
        IRepository<CatalogProductCategory> catalogProductCategories,
        IRepository<CatalogAttributeDefinition> catalogAttributeDefinitions,
        IRepository<CatalogProductAttribute> catalogProductAttributes,
        IRepository<CatalogProductMedia> catalogProductMedia,
        IRepository<CatalogProductDocument> catalogProductDocuments,
        IRepository<CatalogProductFavorite> catalogProductFavorites,
        IRepository<CatalogCategoryFavorite> catalogCategoryFavorites,
        IRepository<CustomerProductAlias> aliases,
        IRepository<B2bCompany> companies,
        IRepository<B2bBuyer> buyers,
        IRepository<B2bCart> carts,
        IRepository<B2bCartLine> cartLines,
        IRepository<B2bOrder> orders,
        IRepository<B2bOrderLine> orderLines,
        IRepository<CustomerEntity> customers,
        IRepository<PaymentOrder> paymentOrders,
        IRepository<PaymentInstallment> paymentInstallments,
        IRepository<PaymentTransaction> payments,
        IRepository<QuoteRequest> quotes,
        IRepository<InventorySnapshot> inventory,
        IB2bPricingAvailabilityResolver pricingAvailabilityResolver,
        IFileUploadService fileUploadService,
        IUnitOfWork unitOfWork)
    {
        _catalogProducts = catalogProducts;
        _catalogVariants = catalogVariants;
        _catalogCategories = catalogCategories;
        _catalogProductCategories = catalogProductCategories;
        _catalogAttributeDefinitions = catalogAttributeDefinitions;
        _catalogProductAttributes = catalogProductAttributes;
        _catalogProductMedia = catalogProductMedia;
        _catalogProductDocuments = catalogProductDocuments;
        _catalogProductFavorites = catalogProductFavorites;
        _catalogCategoryFavorites = catalogCategoryFavorites;
        _aliases = aliases;
        _companies = companies;
        _buyers = buyers;
        _carts = carts;
        _cartLines = cartLines;
        _orders = orders;
        _orderLines = orderLines;
        _customers = customers;
        _paymentOrders = paymentOrders;
        _paymentInstallments = paymentInstallments;
        _payments = payments;
        _quotes = quotes;
        _inventory = inventory;
        _pricingAvailabilityResolver = pricingAvailabilityResolver;
        _fileUploadService = fileUploadService;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResponse<CatalogProductDto>>> GetCatalogProductsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        return await GetCatalogProductsAsync(request, publishedOnly: false, cancellationToken);
    }

    public async Task<ApiResponse<PagedResponse<CatalogProductDto>>> GetPublicCatalogProductsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        return await GetCatalogProductsAsync(request, publishedOnly: true, cancellationToken);
    }

    private async Task<ApiResponse<PagedResponse<CatalogProductDto>>> GetCatalogProductsAsync(PagedRequest request, bool publishedOnly, CancellationToken cancellationToken)
    {
        request ??= new PagedRequest();
        var query = _catalogProducts.Query()
            .Include(x => x.Variants.Where(v => !v.IsDeleted))
            .Include(x => x.ProductCategories.Where(v => !v.IsDeleted)).ThenInclude(x => x.CatalogCategory)
            .Include(x => x.ProductAttributes.Where(v => !v.IsDeleted)).ThenInclude(x => x.AttributeDefinition)
            .Include(x => x.MediaItems.Where(v => !v.IsDeleted))
            .Include(x => x.Documents.Where(v => !v.IsDeleted))
            .Where(x => !x.IsDeleted);

        if (publishedOnly)
        {
            query = query.Where(x => x.IsPublished);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.Sku.Contains(search) ||
                x.Name.Contains(search) ||
                (x.Brand != null && x.Brand.Contains(search)) ||
                (x.ManufacturerCode != null && x.ManufacturerCode.Contains(search)) ||
                (x.Barcode != null && x.Barcode.Contains(search)) ||
                (x.CategoryPath != null && x.CategoryPath.Contains(search)) ||
                (x.SearchKeywords != null && x.SearchKeywords.Contains(search)) ||
                (x.SearchText != null && x.SearchText.Contains(search)));
        }

        query = string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase)
            ? query.OrderBy(x => x.Name)
            : query.OrderByDescending(x => x.Id);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<CatalogProductDto>>.SuccessResult(
            new PagedResponse<CatalogProductDto>(items.Select(MapCatalogProduct).ToList(), total, pageNumber, pageSize),
            "Catalog products retrieved successfully");
    }

    public async Task<ApiResponse<CatalogProductDto>> GetCatalogProductAsync(long id, CancellationToken cancellationToken = default)
    {
        var item = await _catalogProducts.Query()
            .Include(x => x.Variants.Where(v => !v.IsDeleted))
            .Include(x => x.ProductCategories.Where(v => !v.IsDeleted)).ThenInclude(x => x.CatalogCategory)
            .Include(x => x.ProductAttributes.Where(v => !v.IsDeleted)).ThenInclude(x => x.AttributeDefinition)
            .Include(x => x.MediaItems.Where(v => !v.IsDeleted))
            .Include(x => x.Documents.Where(v => !v.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        return item == null
            ? ApiResponse<CatalogProductDto>.ErrorResult("Catalog product not found", statusCode: 404)
            : ApiResponse<CatalogProductDto>.SuccessResult(MapCatalogProduct(item), "Catalog product retrieved successfully");
    }

    public async Task<ApiResponse<CatalogProductDto>> CreateCatalogProductAsync(CreateCatalogProductDto dto, CancellationToken cancellationToken = default)
    {
        var sku = Normalize(dto.Sku);
        var exists = await _catalogProducts.Query().AnyAsync(x => x.Sku == sku && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            return ApiResponse<CatalogProductDto>.ErrorResult("Catalog SKU already exists", statusCode: 400);
        }

        var now = DateTimeProvider.Now;
        var entity = new CatalogProduct
        {
            Sku = sku,
            Name = dto.Name.Trim(),
            Slug = NormalizeSlug(dto.Slug, dto.Name),
            Brand = Trim(dto.Brand),
            ProductType = Trim(dto.ProductType),
            ManufacturerCode = Trim(dto.ManufacturerCode),
            Barcode = Trim(dto.Barcode),
            Unit = Trim(dto.Unit),
            CategoryPath = Trim(dto.CategoryPath),
            ShortDescription = Trim(dto.ShortDescription),
            Description = dto.Description,
            PrimaryImageUrl = Trim(dto.PrimaryImageUrl),
            BulletPointsJson = Trim(dto.BulletPointsJson),
            AttributesJson = Trim(dto.AttributesJson),
            MediaGalleryJson = Trim(dto.MediaGalleryJson),
            DocumentsJson = Trim(dto.DocumentsJson),
            MetaTitle = Trim(dto.MetaTitle),
            MetaDescription = Trim(dto.MetaDescription),
            SearchKeywords = Trim(dto.SearchKeywords),
            MinOrderQuantity = dto.MinOrderQuantity,
            PackageQuantity = dto.PackageQuantity,
            SortOrder = dto.SortOrder,
            IsPublished = dto.IsPublished,
            DefaultStockId = dto.DefaultStockId,
            PublishedDate = dto.IsPublished ? now : null,
            CreatedDate = now
        };
        entity.CompletenessScore = CalculateCatalogCompleteness(entity);
        entity.SearchText = BuildSearchText(entity);

        await _catalogProducts.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogProductDto>.SuccessResult(MapCatalogProduct(entity), "Catalog product created successfully");
    }

    public async Task<ApiResponse<CatalogProductDto>> UpdateCatalogProductAsync(long id, UpdateCatalogProductDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _catalogProducts.Query(tracking: true)
            .Include(x => x.Variants.Where(v => !v.IsDeleted))
            .Include(x => x.ProductCategories.Where(v => !v.IsDeleted)).ThenInclude(x => x.CatalogCategory)
            .Include(x => x.ProductAttributes.Where(v => !v.IsDeleted)).ThenInclude(x => x.AttributeDefinition)
            .Include(x => x.MediaItems.Where(v => !v.IsDeleted))
            .Include(x => x.Documents.Where(v => !v.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            return ApiResponse<CatalogProductDto>.ErrorResult("Catalog product not found", statusCode: 404);
        }

        if (!string.IsNullOrWhiteSpace(dto.Sku))
        {
            var sku = Normalize(dto.Sku);
            var duplicate = await _catalogProducts.Query().AnyAsync(x => x.Id != id && x.Sku == sku && !x.IsDeleted, cancellationToken);
            if (duplicate)
            {
                return ApiResponse<CatalogProductDto>.ErrorResult("Catalog SKU already exists", statusCode: 400);
            }
            entity.Sku = sku;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Slug)) entity.Slug = NormalizeSlug(dto.Slug, entity.Name);
        entity.Brand = dto.Brand ?? entity.Brand;
        entity.ProductType = dto.ProductType ?? entity.ProductType;
        entity.ManufacturerCode = dto.ManufacturerCode ?? entity.ManufacturerCode;
        entity.Barcode = dto.Barcode ?? entity.Barcode;
        entity.Unit = dto.Unit ?? entity.Unit;
        entity.CategoryPath = dto.CategoryPath ?? entity.CategoryPath;
        entity.ShortDescription = dto.ShortDescription ?? entity.ShortDescription;
        entity.Description = dto.Description ?? entity.Description;
        entity.PrimaryImageUrl = dto.PrimaryImageUrl ?? entity.PrimaryImageUrl;
        entity.BulletPointsJson = dto.BulletPointsJson ?? entity.BulletPointsJson;
        entity.AttributesJson = dto.AttributesJson ?? entity.AttributesJson;
        entity.MediaGalleryJson = dto.MediaGalleryJson ?? entity.MediaGalleryJson;
        entity.DocumentsJson = dto.DocumentsJson ?? entity.DocumentsJson;
        entity.MetaTitle = dto.MetaTitle ?? entity.MetaTitle;
        entity.MetaDescription = dto.MetaDescription ?? entity.MetaDescription;
        entity.SearchKeywords = dto.SearchKeywords ?? entity.SearchKeywords;
        entity.MinOrderQuantity = dto.MinOrderQuantity ?? entity.MinOrderQuantity;
        entity.PackageQuantity = dto.PackageQuantity ?? entity.PackageQuantity;
        entity.SortOrder = dto.SortOrder ?? entity.SortOrder;
        entity.DefaultStockId = dto.DefaultStockId ?? entity.DefaultStockId;
        if (dto.IsPublished.HasValue && entity.IsPublished != dto.IsPublished.Value)
        {
            entity.IsPublished = dto.IsPublished.Value;
            entity.PublishedDate = dto.IsPublished.Value ? DateTimeProvider.Now : null;
        }
        entity.CompletenessScore = CalculateCatalogCompleteness(entity);
        entity.SearchText = BuildSearchText(entity);
        entity.SetUpdatedInfo();
        _catalogProducts.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogProductDto>.SuccessResult(MapCatalogProduct(entity), "Catalog product updated successfully");
    }

    public async Task<ApiResponse<CatalogVariantDto>> UpsertVariantAsync(long productId, UpsertCatalogVariantDto dto, CancellationToken cancellationToken = default)
    {
        var productExists = await _catalogProducts.Query().AnyAsync(x => x.Id == productId && !x.IsDeleted, cancellationToken);
        if (!productExists)
        {
            return ApiResponse<CatalogVariantDto>.ErrorResult("Catalog product not found", statusCode: 404);
        }

        var variant = dto.Id.HasValue
            ? await _catalogVariants.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.CatalogProductId == productId && !x.IsDeleted, cancellationToken)
            : null;

        if (variant == null)
        {
            variant = new CatalogVariant { CatalogProductId = productId, CreatedDate = DateTimeProvider.Now };
            await _catalogVariants.AddAsync(variant, cancellationToken);
        }

        variant.ErpStockId = dto.ErpStockId;
        variant.VariantSku = Normalize(dto.VariantSku);
        variant.VariantName = dto.VariantName.Trim();
        variant.Barcode = Trim(dto.Barcode);
        variant.Unit = Trim(dto.Unit);
        variant.AttributesJson = dto.AttributesJson;
        variant.MediaGalleryJson = dto.MediaGalleryJson;
        variant.SortOrder = dto.SortOrder;
        variant.IsActive = dto.IsActive;
        variant.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogVariantDto>.SuccessResult(MapCatalogVariant(variant), "Catalog variant saved successfully");
    }

    public async Task<ApiResponse<PagedResponse<CatalogCategoryDto>>> GetCatalogCategoriesAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _catalogCategories.Query().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.Code.Contains(search) ||
                x.Name.Contains(search) ||
                (x.FullPath != null && x.FullPath.Contains(search)));
        }

        query = query.OrderBy(x => x.Level).ThenBy(x => x.SortOrder).ThenBy(x => x.Name);
        var total = await query.CountAsync(cancellationToken);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 50 : request.PageSize;
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<CatalogCategoryDto>>.SuccessResult(
            new PagedResponse<CatalogCategoryDto>(items.Select(MapCatalogCategory).ToList(), total, pageNumber, pageSize),
            "Catalog categories retrieved successfully");
    }

    public async Task<ApiResponse<CatalogCategoryDto>> CreateCatalogCategoryAsync(CreateCatalogCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var code = Normalize(dto.Code);
        var duplicate = await _catalogCategories.Query().AnyAsync(x => x.Code == code && !x.IsDeleted, cancellationToken);
        if (duplicate)
        {
            return ApiResponse<CatalogCategoryDto>.ErrorResult("Catalog category code already exists", statusCode: 400);
        }

        var parent = dto.ParentCategoryId.HasValue
            ? await _catalogCategories.Query().FirstOrDefaultAsync(x => x.Id == dto.ParentCategoryId.Value && !x.IsDeleted, cancellationToken)
            : null;
        if (dto.ParentCategoryId.HasValue && parent == null)
        {
            return ApiResponse<CatalogCategoryDto>.ErrorResult("Parent category not found", statusCode: 404);
        }

        var name = dto.Name.Trim();
        var entity = new CatalogCategory
        {
            ParentCategoryId = dto.ParentCategoryId,
            Code = code,
            Name = name,
            Description = Trim(dto.Description),
            Level = parent == null ? 1 : parent.Level + 1,
            FullPath = BuildCategoryFullPath(parent, name),
            SortOrder = dto.SortOrder,
            ImageUrl = Trim(dto.ImageUrl),
            IconName = Trim(dto.IconName),
            ColorHex = Trim(dto.ColorHex),
            IsLeaf = dto.IsLeaf,
            IsActive = dto.IsActive,
            CreatedDate = DateTimeProvider.Now
        };

        await _catalogCategories.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogCategoryDto>.SuccessResult(MapCatalogCategory(entity), "Catalog category created successfully");
    }

    public async Task<ApiResponse<CatalogCategoryDto>> UpdateCatalogCategoryAsync(long id, UpdateCatalogCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _catalogCategories.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            return ApiResponse<CatalogCategoryDto>.ErrorResult("Catalog category not found", statusCode: 404);
        }

        if (!string.IsNullOrWhiteSpace(dto.Code))
        {
            var code = Normalize(dto.Code);
            var duplicate = await _catalogCategories.Query().AnyAsync(x => x.Id != id && x.Code == code && !x.IsDeleted, cancellationToken);
            if (duplicate)
            {
                return ApiResponse<CatalogCategoryDto>.ErrorResult("Catalog category code already exists", statusCode: 400);
            }
            entity.Code = code;
        }

        if (dto.ParentCategoryId.HasValue && dto.ParentCategoryId.Value == id)
        {
            return ApiResponse<CatalogCategoryDto>.ErrorResult("Category cannot be its own parent", statusCode: 400);
        }

        CatalogCategory? parent = null;
        if (dto.ParentCategoryId.HasValue)
        {
            parent = await _catalogCategories.Query().FirstOrDefaultAsync(x => x.Id == dto.ParentCategoryId.Value && !x.IsDeleted, cancellationToken);
            if (parent == null)
            {
                return ApiResponse<CatalogCategoryDto>.ErrorResult("Parent category not found", statusCode: 404);
            }
            entity.ParentCategoryId = dto.ParentCategoryId.Value;
            entity.Level = parent.Level + 1;
        }

        if (!string.IsNullOrWhiteSpace(dto.Name)) entity.Name = dto.Name.Trim();
        entity.Description = dto.Description ?? entity.Description;
        entity.SortOrder = dto.SortOrder ?? entity.SortOrder;
        entity.ImageUrl = dto.ImageUrl ?? entity.ImageUrl;
        entity.IconName = dto.IconName ?? entity.IconName;
        entity.ColorHex = dto.ColorHex ?? entity.ColorHex;
        entity.IsLeaf = dto.IsLeaf ?? entity.IsLeaf;
        entity.IsActive = dto.IsActive ?? entity.IsActive;
        entity.FullPath = BuildCategoryFullPath(parent, entity.Name);
        entity.SetUpdatedInfo();
        _catalogCategories.Update(entity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogCategoryDto>.SuccessResult(MapCatalogCategory(entity), "Catalog category updated successfully");
    }

    public async Task<ApiResponse<PagedResponse<CatalogProductFavoriteDto>>> GetCatalogProductFavoritesAsync(PagedRequest request, long companyId, long? buyerId = null, long? userId = null, CancellationToken cancellationToken = default)
    {
        var scopeValidation = await ValidateFavoriteScopeAsync(companyId, buyerId, cancellationToken);
        if (!scopeValidation.Success)
        {
            return ApiResponse<PagedResponse<CatalogProductFavoriteDto>>.ErrorResult(scopeValidation.Message, statusCode: scopeValidation.StatusCode);
        }

        request ??= new PagedRequest();
        var query = _catalogProductFavorites.Query()
            .Include(x => x.Company)
            .Include(x => x.Buyer)
            .Include(x => x.CatalogProduct)
            .Include(x => x.CatalogVariant)
            .Where(x => x.CompanyId == companyId);

        if (buyerId.HasValue) query = query.Where(x => x.BuyerId == buyerId.Value);
        if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.FavoriteKey.Contains(search) ||
                (x.Sku != null && x.Sku.Contains(search)) ||
                (x.CatalogProduct != null && (x.CatalogProduct.Name.Contains(search) || x.CatalogProduct.Sku.Contains(search))) ||
                (x.CatalogVariant != null && (x.CatalogVariant.VariantName.Contains(search) || x.CatalogVariant.VariantSku.Contains(search))));
        }

        query = query.OrderByDescending(x => x.Id);
        var total = await query.CountAsync(cancellationToken);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<CatalogProductFavoriteDto>>.SuccessResult(
            new PagedResponse<CatalogProductFavoriteDto>(items.Select(MapCatalogProductFavorite).ToList(), total, pageNumber, pageSize),
            "Catalog product favorites retrieved successfully");
    }

    public async Task<ApiResponse<CatalogFavoriteToggleResultDto>> ToggleCatalogProductFavoriteAsync(ToggleCatalogProductFavoriteDto dto, CancellationToken cancellationToken = default)
    {
        var scopeValidation = await ValidateFavoriteScopeAsync(dto.CompanyId, dto.BuyerId, cancellationToken);
        if (!scopeValidation.Success)
        {
            return ApiResponse<CatalogFavoriteToggleResultDto>.ErrorResult(scopeValidation.Message, statusCode: scopeValidation.StatusCode);
        }

        var favoriteTarget = await ResolveProductFavoriteTargetAsync(dto, cancellationToken);
        if (!favoriteTarget.Success)
        {
            return ApiResponse<CatalogFavoriteToggleResultDto>.ErrorResult(favoriteTarget.Message, statusCode: favoriteTarget.StatusCode);
        }

        var target = favoriteTarget.Data!;
        var favorite = await _catalogProductFavorites.Query(tracking: true, ignoreQueryFilters: true)
            .FirstOrDefaultAsync(x =>
                x.CompanyId == dto.CompanyId &&
                x.BuyerId == dto.BuyerId &&
                x.UserId == dto.UserId &&
                x.FavoriteKey == target.FavoriteKey,
                cancellationToken);

        var shouldFavorite = dto.IsFavorite ?? favorite is null or { IsDeleted: true };
        if (shouldFavorite)
        {
            if (favorite == null)
            {
                favorite = new CatalogProductFavorite
                {
                    CompanyId = dto.CompanyId,
                    BuyerId = dto.BuyerId,
                    UserId = dto.UserId,
                    CreatedDate = DateTimeProvider.Now
                };
                await _catalogProductFavorites.AddAsync(favorite, cancellationToken);
            }

            favorite.CatalogProductId = target.CatalogProductId;
            favorite.CatalogVariantId = target.CatalogVariantId;
            favorite.ErpStockId = target.ErpStockId;
            favorite.FavoriteKey = target.FavoriteKey;
            favorite.Sku = target.Sku;
            favorite.Note = Trim(dto.Note);
            favorite.IsDeleted = false;
            favorite.DeletedDate = null;
            favorite.DeletedBy = null;
            favorite.SetUpdatedInfo();
        }
        else if (favorite != null && !favorite.IsDeleted)
        {
            favorite.MarkAsDeleted();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogFavoriteToggleResultDto>.SuccessResult(new CatalogFavoriteToggleResultDto
        {
            IsFavorite = shouldFavorite,
            FavoriteId = shouldFavorite ? favorite?.Id : null,
            FavoriteKey = target.FavoriteKey
        }, shouldFavorite ? "Catalog product added to favorites" : "Catalog product removed from favorites");
    }

    public async Task<ApiResponse<PagedResponse<CatalogCategoryFavoriteDto>>> GetCatalogCategoryFavoritesAsync(PagedRequest request, long companyId, long? buyerId = null, long? userId = null, CancellationToken cancellationToken = default)
    {
        var scopeValidation = await ValidateFavoriteScopeAsync(companyId, buyerId, cancellationToken);
        if (!scopeValidation.Success)
        {
            return ApiResponse<PagedResponse<CatalogCategoryFavoriteDto>>.ErrorResult(scopeValidation.Message, statusCode: scopeValidation.StatusCode);
        }

        request ??= new PagedRequest();
        var query = _catalogCategoryFavorites.Query()
            .Include(x => x.Company)
            .Include(x => x.Buyer)
            .Include(x => x.CatalogCategory)
            .Where(x => x.CompanyId == companyId);

        if (buyerId.HasValue) query = query.Where(x => x.BuyerId == buyerId.Value);
        if (userId.HasValue) query = query.Where(x => x.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.CatalogCategory != null &&
                (x.CatalogCategory.Name.Contains(search) ||
                 x.CatalogCategory.Code.Contains(search) ||
                 (x.CatalogCategory.FullPath != null && x.CatalogCategory.FullPath.Contains(search))));
        }

        query = query.OrderByDescending(x => x.Id);
        var total = await query.CountAsync(cancellationToken);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<CatalogCategoryFavoriteDto>>.SuccessResult(
            new PagedResponse<CatalogCategoryFavoriteDto>(items.Select(MapCatalogCategoryFavorite).ToList(), total, pageNumber, pageSize),
            "Catalog category favorites retrieved successfully");
    }

    public async Task<ApiResponse<CatalogFavoriteToggleResultDto>> ToggleCatalogCategoryFavoriteAsync(ToggleCatalogCategoryFavoriteDto dto, CancellationToken cancellationToken = default)
    {
        var scopeValidation = await ValidateFavoriteScopeAsync(dto.CompanyId, dto.BuyerId, cancellationToken);
        if (!scopeValidation.Success)
        {
            return ApiResponse<CatalogFavoriteToggleResultDto>.ErrorResult(scopeValidation.Message, statusCode: scopeValidation.StatusCode);
        }

        var categoryExists = await _catalogCategories.Query()
            .AnyAsync(x => x.Id == dto.CatalogCategoryId && !x.IsDeleted && x.IsActive, cancellationToken);
        if (!categoryExists)
        {
            return ApiResponse<CatalogFavoriteToggleResultDto>.ErrorResult("Catalog category not found", statusCode: 404);
        }

        var favorite = await _catalogCategoryFavorites.Query(tracking: true, ignoreQueryFilters: true)
            .FirstOrDefaultAsync(x =>
                x.CompanyId == dto.CompanyId &&
                x.BuyerId == dto.BuyerId &&
                x.UserId == dto.UserId &&
                x.CatalogCategoryId == dto.CatalogCategoryId,
                cancellationToken);

        var shouldFavorite = dto.IsFavorite ?? favorite is null or { IsDeleted: true };
        if (shouldFavorite)
        {
            if (favorite == null)
            {
                favorite = new CatalogCategoryFavorite
                {
                    CompanyId = dto.CompanyId,
                    BuyerId = dto.BuyerId,
                    UserId = dto.UserId,
                    CatalogCategoryId = dto.CatalogCategoryId,
                    CreatedDate = DateTimeProvider.Now
                };
                await _catalogCategoryFavorites.AddAsync(favorite, cancellationToken);
            }

            favorite.Note = Trim(dto.Note);
            favorite.IsDeleted = false;
            favorite.DeletedDate = null;
            favorite.DeletedBy = null;
            favorite.SetUpdatedInfo();
        }
        else if (favorite != null && !favorite.IsDeleted)
        {
            favorite.MarkAsDeleted();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogFavoriteToggleResultDto>.SuccessResult(new CatalogFavoriteToggleResultDto
        {
            IsFavorite = shouldFavorite,
            FavoriteId = shouldFavorite ? favorite?.Id : null,
            FavoriteKey = $"C:{dto.CatalogCategoryId}"
        }, shouldFavorite ? "Catalog category added to favorites" : "Catalog category removed from favorites");
    }

    public async Task<ApiResponse<CatalogProductCategoryDto>> AssignCatalogProductCategoryAsync(long productId, AssignCatalogProductCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _catalogProducts.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == productId && !x.IsDeleted, cancellationToken);
        if (product == null) return ApiResponse<CatalogProductCategoryDto>.ErrorResult("Catalog product not found", statusCode: 404);

        var category = await _catalogCategories.Query().FirstOrDefaultAsync(x => x.Id == dto.CatalogCategoryId && !x.IsDeleted && x.IsActive, cancellationToken);
        if (category == null) return ApiResponse<CatalogProductCategoryDto>.ErrorResult("Catalog category not found", statusCode: 404);

        if (dto.IsPrimary)
        {
            var existingPrimary = await _catalogProductCategories.Query(tracking: true)
                .Where(x => !x.IsDeleted && x.CatalogProductId == productId && x.IsPrimary)
                .ToListAsync(cancellationToken);
            foreach (var item in existingPrimary)
            {
                item.IsPrimary = false;
                item.SetUpdatedInfo();
            }
            product.CategoryPath = category.FullPath ?? category.Name;
        }

        var entity = await _catalogProductCategories.Query(tracking: true)
            .FirstOrDefaultAsync(x => x.CatalogProductId == productId && x.CatalogCategoryId == dto.CatalogCategoryId && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            entity = new CatalogProductCategory { CatalogProductId = productId, CatalogCategoryId = dto.CatalogCategoryId, CreatedDate = DateTimeProvider.Now };
            await _catalogProductCategories.AddAsync(entity, cancellationToken);
        }

        entity.IsPrimary = dto.IsPrimary;
        entity.SortOrder = dto.SortOrder;
        entity.AssignmentSource = Trim(dto.AssignmentSource) ?? "Manual";
        entity.SetUpdatedInfo();
        product.CompletenessScore = CalculateCatalogCompleteness(product);
        product.SearchText = BuildSearchText(product);
        product.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogProductCategoryDto>.SuccessResult(MapCatalogProductCategory(entity, category), "Catalog product category assigned successfully");
    }

    public async Task<ApiResponse<PagedResponse<CatalogAttributeDefinitionDto>>> GetCatalogAttributeDefinitionsAsync(PagedRequest request, long? categoryId = null, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _catalogAttributeDefinitions.Query().Where(x => !x.IsDeleted);
        if (categoryId.HasValue) query = query.Where(x => x.CatalogCategoryId == categoryId.Value || x.CatalogCategoryId == null);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search));
        }

        query = query.OrderBy(x => x.CatalogCategoryId.HasValue).ThenBy(x => x.SortOrder).ThenBy(x => x.Name);
        var total = await query.CountAsync(cancellationToken);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 50 : request.PageSize;
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<CatalogAttributeDefinitionDto>>.SuccessResult(
            new PagedResponse<CatalogAttributeDefinitionDto>(items.Select(MapCatalogAttributeDefinition).ToList(), total, pageNumber, pageSize),
            "Catalog attribute definitions retrieved successfully");
    }

    public async Task<ApiResponse<CatalogAttributeDefinitionDto>> CreateCatalogAttributeDefinitionAsync(CreateCatalogAttributeDefinitionDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.CatalogCategoryId.HasValue)
        {
            var categoryExists = await _catalogCategories.Query().AnyAsync(x => x.Id == dto.CatalogCategoryId.Value && !x.IsDeleted, cancellationToken);
            if (!categoryExists) return ApiResponse<CatalogAttributeDefinitionDto>.ErrorResult("Catalog category not found", statusCode: 404);
        }

        var code = Normalize(dto.Code);
        var duplicate = await _catalogAttributeDefinitions.Query()
            .AnyAsync(x => x.CatalogCategoryId == dto.CatalogCategoryId && x.Code == code && !x.IsDeleted, cancellationToken);
        if (duplicate)
        {
            return ApiResponse<CatalogAttributeDefinitionDto>.ErrorResult("Catalog attribute code already exists for this category", statusCode: 400);
        }

        var entity = new CatalogAttributeDefinition
        {
            CatalogCategoryId = dto.CatalogCategoryId,
            Code = code,
            Name = dto.Name.Trim(),
            DataType = NormalizeStatus(dto.DataType, "Text"),
            IsRequired = dto.IsRequired,
            IsFilterable = dto.IsFilterable,
            IsComparable = dto.IsComparable,
            Unit = Trim(dto.Unit),
            AllowedValuesJson = Trim(dto.AllowedValuesJson),
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedDate = DateTimeProvider.Now
        };
        await _catalogAttributeDefinitions.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogAttributeDefinitionDto>.SuccessResult(MapCatalogAttributeDefinition(entity), "Catalog attribute definition created successfully");
    }

    public async Task<ApiResponse<CatalogProductAttributeDto>> UpsertCatalogProductAttributeAsync(long productId, UpsertCatalogProductAttributeDto dto, CancellationToken cancellationToken = default)
    {
        var productExists = await _catalogProducts.Query().AnyAsync(x => x.Id == productId && !x.IsDeleted, cancellationToken);
        if (!productExists) return ApiResponse<CatalogProductAttributeDto>.ErrorResult("Catalog product not found", statusCode: 404);

        var definition = await _catalogAttributeDefinitions.Query().FirstOrDefaultAsync(x => x.Id == dto.AttributeDefinitionId && !x.IsDeleted && x.IsActive, cancellationToken);
        if (definition == null) return ApiResponse<CatalogProductAttributeDto>.ErrorResult("Catalog attribute definition not found", statusCode: 404);

        var entity = await _catalogProductAttributes.Query(tracking: true)
            .FirstOrDefaultAsync(x => x.CatalogProductId == productId && x.AttributeDefinitionId == dto.AttributeDefinitionId && !x.IsDeleted, cancellationToken);
        if (entity == null)
        {
            entity = new CatalogProductAttribute { CatalogProductId = productId, AttributeDefinitionId = dto.AttributeDefinitionId, CreatedDate = DateTimeProvider.Now };
            await _catalogProductAttributes.AddAsync(entity, cancellationToken);
        }

        entity.Value = dto.Value.Trim();
        entity.NormalizedValue = NormalizeAttributeValue(dto.Value);
        entity.Unit = Trim(dto.Unit) ?? definition.Unit;
        entity.SortOrder = dto.SortOrder;
        entity.SetUpdatedInfo();
        await RefreshCatalogQualityAsync(productId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogProductAttributeDto>.SuccessResult(MapCatalogProductAttribute(entity, definition), "Catalog product attribute saved successfully");
    }

    public async Task<ApiResponse<CatalogProductMediaDto>> UpsertCatalogProductMediaAsync(long productId, UpsertCatalogProductMediaDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _catalogProducts.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == productId && !x.IsDeleted, cancellationToken);
        if (product == null) return ApiResponse<CatalogProductMediaDto>.ErrorResult("Catalog product not found", statusCode: 404);

        if (dto.IsPrimary)
        {
            var existingPrimary = await _catalogProductMedia.Query(tracking: true)
                .Where(x => !x.IsDeleted && x.CatalogProductId == productId && x.IsPrimary)
                .ToListAsync(cancellationToken);
            foreach (var item in existingPrimary)
            {
                item.IsPrimary = false;
                item.SetUpdatedInfo();
            }
            product.PrimaryImageUrl = dto.Url.Trim();
        }

        var entity = dto.Id.HasValue
            ? await _catalogProductMedia.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.CatalogProductId == productId && !x.IsDeleted, cancellationToken)
            : null;
        if (entity == null)
        {
            entity = new CatalogProductMedia { CatalogProductId = productId, CreatedDate = DateTimeProvider.Now };
            await _catalogProductMedia.AddAsync(entity, cancellationToken);
        }

        entity.Url = dto.Url.Trim();
        entity.MediaType = NormalizeStatus(dto.MediaType, "Image");
        entity.AltText = Trim(dto.AltText);
        entity.IsPrimary = dto.IsPrimary;
        entity.SortOrder = dto.SortOrder;
        entity.SetUpdatedInfo();
        product.CompletenessScore = CalculateCatalogCompleteness(product);
        product.SearchText = BuildSearchText(product);
        product.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogProductMediaDto>.SuccessResult(MapCatalogProductMedia(entity), "Catalog product media saved successfully");
    }

    public async Task<ApiResponse<List<CatalogProductMediaDto>>> UploadCatalogProductMediaAsync(long productId, UploadCatalogProductMediaDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Files == null || dto.Files.Count == 0)
        {
            return ApiResponse<List<CatalogProductMediaDto>>.ErrorResult("At least one image file is required", statusCode: 400);
        }

        var product = await _catalogProducts.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == productId && !x.IsDeleted, cancellationToken);
        if (product == null) return ApiResponse<List<CatalogProductMediaDto>>.ErrorResult("Catalog product not found", statusCode: 404);

        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var nextSortOrder = await _catalogProductMedia.Query()
                .Where(x => x.CatalogProductId == productId && !x.IsDeleted)
                .Select(x => (int?)x.SortOrder)
                .MaxAsync(cancellationToken) ?? 0;
            var hasPrimary = await _catalogProductMedia.Query()
                .AnyAsync(x => x.CatalogProductId == productId && x.IsPrimary && !x.IsDeleted, cancellationToken);

            var created = new List<CatalogProductMedia>();
            for (var index = 0; index < dto.Files.Count; index++)
            {
                var upload = await _fileUploadService.UploadCatalogProductImageAsync(dto.Files[index], productId, cancellationToken);
                if (!upload.Success || string.IsNullOrWhiteSpace(upload.Data))
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<List<CatalogProductMediaDto>>.ErrorResult(upload.Message, upload.ExceptionMessage, upload.StatusCode);
                }

                nextSortOrder++;
                var isPrimary = dto.FirstImageAsPrimary && !hasPrimary && index == 0;
                var media = new CatalogProductMedia
                {
                    CatalogProductId = productId,
                    BranchCode = product.BranchCode,
                    Url = upload.Data!,
                    MediaType = "Image",
                    AltText = dto.AltTexts != null && index < dto.AltTexts.Count && !string.IsNullOrWhiteSpace(dto.AltTexts[index]) ? dto.AltTexts[index].Trim() : product.Name,
                    IsPrimary = isPrimary,
                    SortOrder = nextSortOrder,
                    CreatedDate = DateTimeProvider.Now
                };
                await _catalogProductMedia.AddAsync(media, cancellationToken);
                created.Add(media);

                if (isPrimary)
                {
                    hasPrimary = true;
                    product.PrimaryImageUrl = media.Url;
                }
            }

            product.CompletenessScore = CalculateCatalogCompleteness(product);
            product.SearchText = BuildSearchText(product);
            product.SetUpdatedInfo();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return ApiResponse<List<CatalogProductMediaDto>>.SuccessResult(created.Select(MapCatalogProductMedia).ToList(), "Catalog product media uploaded successfully");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            return ApiResponse<List<CatalogProductMediaDto>>.ErrorResult("Catalog product media upload failed", ex.Message, 500);
        }
    }

    public async Task<ApiResponse<CatalogProductDocumentDto>> UpsertCatalogProductDocumentAsync(long productId, UpsertCatalogProductDocumentDto dto, CancellationToken cancellationToken = default)
    {
        var productExists = await _catalogProducts.Query().AnyAsync(x => x.Id == productId && !x.IsDeleted, cancellationToken);
        if (!productExists) return ApiResponse<CatalogProductDocumentDto>.ErrorResult("Catalog product not found", statusCode: 404);

        var entity = dto.Id.HasValue
            ? await _catalogProductDocuments.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.CatalogProductId == productId && !x.IsDeleted, cancellationToken)
            : null;
        if (entity == null)
        {
            entity = new CatalogProductDocument { CatalogProductId = productId, CreatedDate = DateTimeProvider.Now };
            await _catalogProductDocuments.AddAsync(entity, cancellationToken);
        }

        entity.Name = dto.Name.Trim();
        entity.Url = dto.Url.Trim();
        entity.DocumentType = NormalizeStatus(dto.DocumentType, "TechnicalSheet");
        entity.LanguageCode = Trim(dto.LanguageCode);
        entity.SortOrder = dto.SortOrder;
        entity.IsActive = dto.IsActive;
        entity.SetUpdatedInfo();
        await RefreshCatalogQualityAsync(productId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogProductDocumentDto>.SuccessResult(MapCatalogProductDocument(entity), "Catalog product document saved successfully");
    }

    public async Task<ApiResponse<PagedResponse<CustomerProductAliasDto>>> GetAliasesAsync(PagedRequest request, long? customerId = null, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _aliases.Query().Where(x => !x.IsDeleted);
        if (customerId.HasValue) query = query.Where(x => x.CustomerId == customerId.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.CustomerSku.Contains(search) || (x.CustomerProductName != null && x.CustomerProductName.Contains(search)));
        }

        query = query.OrderByDescending(x => x.Id);
        var total = await query.CountAsync(cancellationToken);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<CustomerProductAliasDto>>.SuccessResult(
            new PagedResponse<CustomerProductAliasDto>(items.Select(MapAlias).ToList(), total, pageNumber, pageSize),
            "Product aliases retrieved successfully");
    }

    public async Task<ApiResponse<CustomerProductAliasDto>> CreateAliasAsync(CreateCustomerProductAliasDto dto, CancellationToken cancellationToken = default)
    {
        var alias = new CustomerProductAlias
        {
            CustomerId = dto.CustomerId,
            ErpStockId = dto.ErpStockId,
            CatalogProductId = dto.CatalogProductId,
            CustomerSku = Normalize(dto.CustomerSku),
            CustomerProductName = Trim(dto.CustomerProductName),
            MatchStatus = NormalizeStatus(dto.MatchStatus, "Pending"),
            ConfidenceScore = dto.ConfidenceScore,
            Notes = dto.Notes,
            MatchedDate = IsMatched(dto.MatchStatus) ? DateTimeProvider.Now : null
        };
        await _aliases.AddAsync(alias, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerProductAliasDto>.SuccessResult(MapAlias(alias), "Product alias created successfully");
    }

    public async Task<ApiResponse<CustomerProductAliasDto>> UpdateAliasAsync(long id, UpdateCustomerProductAliasDto dto, CancellationToken cancellationToken = default)
    {
        var alias = await _aliases.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (alias == null) return ApiResponse<CustomerProductAliasDto>.ErrorResult("Product alias not found", statusCode: 404);
        alias.ErpStockId = dto.ErpStockId ?? alias.ErpStockId;
        alias.CatalogProductId = dto.CatalogProductId ?? alias.CatalogProductId;
        alias.CustomerProductName = dto.CustomerProductName ?? alias.CustomerProductName;
        alias.MatchStatus = NormalizeStatus(dto.MatchStatus, alias.MatchStatus);
        alias.ConfidenceScore = dto.ConfidenceScore ?? alias.ConfidenceScore;
        alias.Notes = dto.Notes ?? alias.Notes;
        alias.MatchedDate = IsMatched(alias.MatchStatus) ? DateTimeProvider.Now : alias.MatchedDate;
        alias.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerProductAliasDto>.SuccessResult(MapAlias(alias), "Product alias updated successfully");
    }

    public async Task<ApiResponse<CartDto>> GetDraftCartAsync(long customerId, long? userId = null, long? buyerId = null, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateDraftCart(customerId, userId, buyerId, "TRY", cancellationToken);
        return ApiResponse<CartDto>.SuccessResult(MapCart(cart), "Draft cart retrieved successfully");
    }

    public async Task<ApiResponse<CartDto>> AddCartLineAsync(AddCartLineDto dto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var resolved = await _pricingAvailabilityResolver.ResolveAsync(new ResolveB2bPriceAvailabilityDto
            {
                CustomerId = dto.CustomerId,
                CustomerGroupCode = dto.CustomerGroupCode,
                CustomerSku = dto.CustomerSku,
                CatalogProductId = dto.CatalogProductId,
                CatalogVariantId = dto.CatalogVariantId,
                ErpStockId = dto.ErpStockId,
                WarehouseCode = dto.WarehouseCode,
                Quantity = dto.Quantity,
                CurrencyCode = dto.CurrencyCode
            }, cancellationToken);

            if (!resolved.Success || resolved.Data == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<CartDto>.ErrorResult(resolved.Message, resolved.ExceptionMessage, resolved.StatusCode);
            }

            if (!resolved.Data.IsPriceResolved)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<CartDto>.ErrorResult("Cart line cannot be added because no effective B2B price was found", statusCode: 400);
            }

            if (!dto.AllowBackorder && !resolved.Data.IsAvailable)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<CartDto>.ErrorResult("Cart line cannot be added because requested quantity is not available", statusCode: 409);
            }

            var cart = await GetOrCreateDraftCart(dto.CustomerId, dto.UserId, dto.BuyerId, dto.CurrencyCode, cancellationToken);
            var warehouseCode = resolved.Data.PreferredWarehouseCode ?? dto.WarehouseCode;
            var line = new B2bCartLine
            {
                CartId = cart.Id,
                CatalogProductId = resolved.Data.CatalogProductId,
                CatalogVariantId = resolved.Data.CatalogVariantId,
                ErpStockId = resolved.Data.ErpStockId,
                WarehouseCode = warehouseCode,
                Quantity = dto.Quantity,
                UnitPrice = resolved.Data.UnitPrice ?? dto.UnitPrice ?? 0,
                CurrencyCode = resolved.Data.CurrencyCode,
                PriceSource = resolved.Data.PriceSource,
                PriceListId = resolved.Data.PriceListId,
                DiscountRate = resolved.Data.DiscountRate,
                VatRate = resolved.Data.VatRate,
                VatAmount = resolved.Data.VatAmount,
                ExchangeRate = resolved.Data.ExchangeRate,
                PriceResolvedAt = resolved.Data.PriceResolvedAt
            };
            await _cartLines.AddAsync(line, cancellationToken);
            var reserveResult = await ReserveInventoryAsync(line, dto.Quantity, dto.AllowBackorder, cancellationToken);
            if (!reserveResult.Success)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<CartDto>.ErrorResult(reserveResult.Message, statusCode: reserveResult.StatusCode);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return await GetDraftCartAsync(dto.CustomerId, dto.UserId, dto.BuyerId, cancellationToken);
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<QuickOrderResultDto>> AddQuickOrderLinesAsync(QuickOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.CustomerId <= 0)
        {
            return ApiResponse<QuickOrderResultDto>.ErrorResult("Customer is required", statusCode: 400);
        }

        if (dto.Lines.Count == 0)
        {
            return ApiResponse<QuickOrderResultDto>.ErrorResult("Quick order must include at least one line", statusCode: 400);
        }

        var result = new QuickOrderResultDto { RequestedLineCount = dto.Lines.Count };
        for (var index = 0; index < dto.Lines.Count; index++)
        {
            var input = dto.Lines[index];
            var lineResult = new QuickOrderLineResultDto
            {
                LineNumber = index + 1,
                CustomerSku = input.CustomerSku,
                CatalogProductId = input.CatalogProductId,
                CatalogVariantId = input.CatalogVariantId,
                ErpStockId = input.ErpStockId,
                Quantity = input.Quantity
            };

            var addResult = await AddCartLineAsync(new AddCartLineDto
            {
                CustomerId = dto.CustomerId,
                BuyerId = dto.BuyerId,
                UserId = dto.UserId,
                CustomerGroupCode = dto.CustomerGroupCode,
                CustomerSku = input.CustomerSku,
                CatalogProductId = input.CatalogProductId,
                CatalogVariantId = input.CatalogVariantId,
                ErpStockId = input.ErpStockId,
                WarehouseCode = input.WarehouseCode,
                Quantity = input.Quantity,
                CurrencyCode = dto.CurrencyCode,
                AllowBackorder = dto.AllowBackorder
            }, cancellationToken);

            lineResult.Success = addResult.Success;
            lineResult.Message = addResult.Success ? "Added to cart" : addResult.Message;
            if (addResult.Success)
            {
                result.AddedLineCount++;
                result.Cart = addResult.Data;
            }

            result.Lines.Add(lineResult);
        }

        result.Cart ??= (await GetDraftCartAsync(dto.CustomerId, dto.UserId, dto.BuyerId, cancellationToken)).Data;
        var statusCode = result.AddedLineCount == result.RequestedLineCount ? 200 : result.AddedLineCount == 0 ? 400 : 207;
        var response = result.AddedLineCount == 0
            ? ApiResponse<QuickOrderResultDto>.ErrorResult("No quick order lines could be added", statusCode: statusCode)
            : ApiResponse<QuickOrderResultDto>.SuccessResult(result, "Quick order processed");
        response.StatusCode = statusCode;
        return response;
    }

    public async Task<ApiResponse<CartDto>> UpdateCartLineAsync(long lineId, UpdateCartLineDto dto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var line = await _cartLines.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == lineId && !x.IsDeleted, cancellationToken);
            if (line == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<CartDto>.ErrorResult("Cart line not found", statusCode: 404);
            }

            var cart = await _carts.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstAsync(x => x.Id == line.CartId, cancellationToken);
            var oldQuantity = line.Quantity;
            var resolved = await ResolveCartLineAsync(cart, line, dto.Quantity, cancellationToken);
            if (!resolved.Success)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<CartDto>.ErrorResult(resolved.Message, resolved.ExceptionMessage, resolved.StatusCode);
            }

            var delta = dto.Quantity - oldQuantity;
            if (delta > 0)
            {
                var reserveResult = await ReserveInventoryAsync(line, delta, allowBackorder: false, cancellationToken);
                if (!reserveResult.Success)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<CartDto>.ErrorResult(reserveResult.Message, statusCode: reserveResult.StatusCode);
                }
            }
            else if (delta < 0)
            {
                await ReleaseInventoryAsync(line, Math.Abs(delta), cancellationToken);
            }

            line.Quantity = dto.Quantity;
            line.UnitPrice = resolved.Data!.UnitPrice!.Value;
            line.CurrencyCode = resolved.Data.CurrencyCode;
            line.WarehouseCode = resolved.Data.PreferredWarehouseCode ?? line.WarehouseCode;
            line.PriceSource = resolved.Data.PriceSource;
            line.PriceListId = resolved.Data.PriceListId;
            line.DiscountRate = resolved.Data.DiscountRate;
            line.VatRate = resolved.Data.VatRate;
            line.VatAmount = resolved.Data.VatAmount;
            line.ExchangeRate = resolved.Data.ExchangeRate;
            line.PriceResolvedAt = resolved.Data.PriceResolvedAt;
            line.SetUpdatedInfo();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            var updatedCart = await _carts.Query().Include(x => x.Lines.Where(l => !l.IsDeleted)).FirstAsync(x => x.Id == line.CartId, cancellationToken);
            return ApiResponse<CartDto>.SuccessResult(MapCart(updatedCart), "Cart line updated successfully");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> RemoveCartLineAsync(long lineId, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var line = await _cartLines.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == lineId && !x.IsDeleted, cancellationToken);
            if (line == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<bool>.ErrorResult("Cart line not found", statusCode: 404);
            }

            await ReleaseInventoryAsync(line, line.Quantity, cancellationToken);
            await _cartLines.SoftDelete(lineId, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            return ApiResponse<bool>.SuccessResult(true, "Cart line removed successfully");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<PagedResponse<OrderDto>>> GetOrdersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _orders.Query()
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Id);
        var total = await query.CountAsync(cancellationToken);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<OrderDto>>.SuccessResult(
            new PagedResponse<OrderDto>(items.Select(MapOrder).ToList(), total, pageNumber, pageSize),
            "Orders retrieved successfully");
    }

    public async Task<ApiResponse<OrderDto>> GetOrderAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _orders.Query()
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        return entity == null
            ? ApiResponse<OrderDto>.ErrorResult("Order not found", statusCode: 404)
            : ApiResponse<OrderDto>.SuccessResult(MapOrder(entity), "Order retrieved successfully");
    }

    public async Task<ApiResponse<OrderDto>> CreateOrderFromCartAsync(CreateOrderFromCartDto dto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var cart = await _carts.Query(tracking: true)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == dto.CartId && !x.IsDeleted && x.Status == B2bWorkflowStatuses.Draft, cancellationToken);
            if (cart == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<OrderDto>.ErrorResult("Draft cart not found", statusCode: 404);
            }
            if (cart.Lines.Count == 0)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<OrderDto>.ErrorResult("Cart is empty", statusCode: 400);
            }

            foreach (var line in cart.Lines)
            {
                var resolved = await ResolveCartLineAsync(cart, line, line.Quantity, cancellationToken);
                if (!resolved.Success)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<OrderDto>.ErrorResult($"Cart line #{line.Id} could not be validated: {resolved.Message}", resolved.ExceptionMessage, resolved.StatusCode);
                }

                line.UnitPrice = resolved.Data!.UnitPrice!.Value;
                line.CurrencyCode = resolved.Data.CurrencyCode;
                line.PriceSource = resolved.Data.PriceSource;
                line.PriceListId = resolved.Data.PriceListId;
                line.DiscountRate = resolved.Data.DiscountRate;
                line.VatRate = resolved.Data.VatRate;
                line.VatAmount = resolved.Data.VatAmount;
                line.ExchangeRate = resolved.Data.ExchangeRate;
                line.PriceResolvedAt = resolved.Data.PriceResolvedAt;
                line.SetUpdatedInfo();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            var subtotal = cart.Lines.Sum(x => x.Quantity * x.UnitPrice);
            if (dto.GeneralDiscountRate is > 0)
            {
                subtotal -= Math.Round(subtotal * dto.GeneralDiscountRate.Value / 100m, 4);
            }
            if (dto.GeneralDiscountAmount is > 0)
            {
                subtotal -= dto.GeneralDiscountAmount.Value;
            }
            subtotal = Math.Max(0, subtotal);
            var order = new B2bOrder
            {
                OrderNumber = string.IsNullOrWhiteSpace(dto.OfferNo) ? $"B2B-{DateTimeProvider.Now:yyyyMMddHHmmssfff}" : dto.OfferNo.Trim(),
                CustomerId = cart.CustomerId,
                BuyerId = cart.BuyerId,
                UserId = cart.UserId,
                Status = B2bWorkflowStatuses.WaitingPayment,
                CurrencyCode = cart.CurrencyCode,
                OfferType = Trim(dto.OfferType),
                OfferDate = dto.OfferDate ?? DateTimeProvider.Now,
                OfferNo = Trim(dto.OfferNo),
                RevisionNo = Trim(dto.RevisionNo),
                RevisionId = dto.RevisionId,
                ValidUntil = dto.ValidUntil,
                DeliveryDate = dto.DeliveryDate,
                DeliveryMethod = Trim(dto.DeliveryMethod),
                PaymentTypeId = dto.PaymentTypeId,
                QuoteRequestId = dto.QuoteRequestId,
                ErpProjectCode = Trim(dto.ErpProjectCode),
                GeneralDiscountRate = dto.GeneralDiscountRate,
                GeneralDiscountAmount = dto.GeneralDiscountAmount,
                Subtotal = subtotal,
                TaxTotal = dto.TaxTotal,
                GrandTotal = subtotal + dto.TaxTotal,
                Description = Trim(dto.Description),
                SubmittedDate = DateTimeProvider.Now
            };
            await _orders.AddAsync(order, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            foreach (var line in cart.Lines)
            {
                await _orderLines.AddAsync(new B2bOrderLine
                {
                    OrderId = order.Id,
                    CatalogProductId = line.CatalogProductId,
                    CatalogVariantId = line.CatalogVariantId,
                    ErpStockId = line.ErpStockId,
                    WarehouseCode = line.WarehouseCode,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    VatRate = line.VatRate,
                    VatAmount = line.VatAmount,
                    ErpProjectCode = Trim(dto.ErpProjectCode),
                    LineTotal = line.Quantity * line.UnitPrice,
                    LineGrandTotal = line.Quantity * line.UnitPrice + line.VatAmount,
                    PriceSource = line.PriceSource,
                    PriceListId = line.PriceListId,
                    ExchangeRate = line.ExchangeRate,
                    PriceResolvedAt = line.PriceResolvedAt
                }, cancellationToken);
            }

            cart.Status = B2bWorkflowStatuses.Submitted;
            cart.SetUpdatedInfo();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            var savedOrder = await _orders.Query().Include(x => x.Lines.Where(l => !l.IsDeleted)).FirstAsync(x => x.Id == order.Id, cancellationToken);
            return ApiResponse<OrderDto>.SuccessResult(MapOrder(savedOrder), "Order created from cart successfully");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<QuickOrderResultDto>> ReorderAsync(ReorderDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orders.Query()
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == dto.OrderId && !x.IsDeleted, cancellationToken);
        if (order == null)
        {
            return ApiResponse<QuickOrderResultDto>.ErrorResult("Order not found", statusCode: 404);
        }

        var quickOrder = new QuickOrderDto
        {
            CustomerId = order.CustomerId,
            BuyerId = order.BuyerId,
            UserId = dto.UserId ?? order.UserId,
            CurrencyCode = order.CurrencyCode,
            AllowBackorder = dto.AllowBackorder,
            Lines = order.Lines
                .Where(x => !x.IsDeleted)
                .Select(x => new QuickOrderLineDto
                {
                    CatalogProductId = x.CatalogProductId,
                    CatalogVariantId = x.CatalogVariantId,
                    ErpStockId = x.ErpStockId,
                    Quantity = x.Quantity
                })
                .ToList()
        };

        return await AddQuickOrderLinesAsync(quickOrder, cancellationToken);
    }

    public async Task<ApiResponse<CustomerPortalSummaryDto>> GetCustomerPortalSummaryAsync(long customerId, long? userId = null, long? buyerId = null, bool includeCompanyHistory = true, CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            return ApiResponse<CustomerPortalSummaryDto>.ErrorResult("Customer is required", statusCode: 400);
        }

        var draftCartQuery = ApplyPortalScope(
            _carts.Query().Where(x => !x.IsDeleted && x.CustomerId == customerId && x.Status == B2bWorkflowStatuses.Draft),
            includeCompanyHistory,
            userId,
            buyerId);

        var orderQuery = ApplyPortalScope(
            _orders.Query().Where(x => !x.IsDeleted && x.CustomerId == customerId),
            includeCompanyHistory,
            userId,
            buyerId);

        var quoteQuery = ApplyPortalScope(
            _quotes.Query().Where(x => !x.IsDeleted && x.CustomerId == customerId),
            includeCompanyHistory,
            userId,
            buyerId);

        var draftCart = await draftCartQuery
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var orders = await orderQuery
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .OrderByDescending(x => x.Id)
            .Take(10)
            .ToListAsync(cancellationToken);

        var scopedOrderIds = orderQuery.Select(x => x.Id);
        var pendingPayments = await _payments.Query()
            .Include(x => x.Order)
            .Where(x => !x.IsDeleted && x.Order != null && scopedOrderIds.Contains(x.Order.Id) && x.Status == B2bWorkflowStatuses.Pending)
            .OrderByDescending(x => x.Id)
            .Take(10)
            .ToListAsync(cancellationToken);

        var openOrderStatuses = new[] { B2bWorkflowStatuses.WaitingPayment, "PendingApproval", B2bWorkflowStatuses.Submitted, B2bWorkflowStatuses.Processing };
        var openOrderTotal = await orderQuery
            .Where(x => openOrderStatuses.Contains(x.Status))
            .SumAsync(x => x.GrandTotal, cancellationToken);

        var summary = new CustomerPortalSummaryDto
        {
            CustomerId = customerId,
            CurrencyCode = draftCart?.CurrencyCode ?? orders.FirstOrDefault()?.CurrencyCode ?? "TRY",
            DraftCartCount = await draftCartQuery.CountAsync(cancellationToken),
            OrderCount = await orderQuery.CountAsync(cancellationToken),
            OpenOrderCount = await orderQuery.CountAsync(x => openOrderStatuses.Contains(x.Status), cancellationToken),
            QuoteCount = await quoteQuery.CountAsync(cancellationToken),
            PendingQuoteCount = await quoteQuery.CountAsync(x => x.Status != B2bWorkflowStatuses.Approved && x.Status != B2bWorkflowStatuses.Rejected && x.Status != B2bWorkflowStatuses.Cancelled, cancellationToken),
            PendingPaymentCount = pendingPayments.Count,
            OpenOrderTotal = openOrderTotal,
            PendingPaymentTotal = pendingPayments.Sum(x => x.Amount),
            DraftCart = draftCart == null ? null : MapCart(draftCart),
            RecentOrders = orders.Select(MapOrder).ToList(),
            PendingPayments = pendingPayments.Select(MapPayment).ToList()
        };

        return ApiResponse<CustomerPortalSummaryDto>.SuccessResult(summary, "Customer portal summary retrieved successfully");
    }

    public async Task<ApiResponse<PaymentTransactionDto>> CreatePaymentTransactionAsync(CreatePaymentTransactionDto dto, CancellationToken cancellationToken = default)
    {
        var paymentOrder = dto.PaymentOrderId.HasValue
            ? await _paymentOrders.Query(tracking: true)
                .Include(x => x.Installments.Where(i => !i.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == dto.PaymentOrderId.Value && !x.IsDeleted, cancellationToken)
            : null;

        var installment = dto.PaymentInstallmentId.HasValue
            ? await _paymentInstallments.Query(tracking: true)
                .FirstOrDefaultAsync(x => x.Id == dto.PaymentInstallmentId.Value && !x.IsDeleted, cancellationToken)
            : null;

        var payment = new PaymentTransaction
        {
            OrderId = dto.OrderId,
            PaymentOrderId = paymentOrder?.Id ?? dto.PaymentOrderId,
            PaymentInstallmentId = installment?.Id ?? dto.PaymentInstallmentId,
            ProviderKey = Normalize(dto.ProviderKey),
            ExternalTransactionId = Trim(dto.ExternalTransactionId),
            Status = B2bWorkflowStatuses.Pending,
            Amount = dto.Amount,
            CurrencyCode = NormalizeCurrency(dto.CurrencyCode),
            PaymentMethod = Trim(dto.PaymentMethod),
            DueDate = dto.DueDate ?? installment?.DueDate ?? paymentOrder?.DueDate,
            PaymentTermDays = dto.PaymentTermDays ?? paymentOrder?.PaymentTermDays,
            InstallmentCount = Math.Max(1, dto.InstallmentCount > 0 ? dto.InstallmentCount : paymentOrder?.InstallmentCount ?? 1),
            InstallmentPlanJson = dto.InstallmentPlanJson ?? (paymentOrder is null ? null : BuildInstallmentPlanJson(paymentOrder.Installments)),
            RequestedDate = DateTimeProvider.Now
        };
        await _payments.AddAsync(payment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<PaymentTransactionDto>.SuccessResult(MapPayment(payment), "Payment transaction created successfully");
    }

    public async Task<ApiResponse<PagedResponse<PaymentTransactionDto>>> GetPaymentsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _payments.Query().Where(x => !x.IsDeleted).OrderByDescending(x => x.Id);
        var total = await query.CountAsync(cancellationToken);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return ApiResponse<PagedResponse<PaymentTransactionDto>>.SuccessResult(
            new PagedResponse<PaymentTransactionDto>(items.Select(MapPayment).ToList(), total, pageNumber, pageSize),
            "Payment transactions retrieved successfully");
    }

    public async Task<ApiResponse<PaymentTransactionDto>> UpdatePaymentStatusAsync(long id, UpdatePaymentStatusDto dto, CancellationToken cancellationToken = default)
    {
        var payment = await _payments.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (payment == null) return ApiResponse<PaymentTransactionDto>.ErrorResult("Payment transaction not found", statusCode: 404);
        var nextStatus = B2bWorkflowStatuses.NormalizeRequired(dto.Status);
        if (!B2bWorkflowStatuses.PaymentStatuses.Contains(nextStatus))
        {
            return ApiResponse<PaymentTransactionDto>.ErrorResult($"Unsupported payment status: {dto.Status}", statusCode: 400);
        }

        payment.Status = nextStatus;
        payment.ExternalTransactionId = dto.ExternalTransactionId ?? payment.ExternalTransactionId;
        payment.CallbackPayloadJson = dto.CallbackPayloadJson ?? payment.CallbackPayloadJson;
        payment.CompletedDate = IsFinalPaymentStatus(payment.Status) ? DateTimeProvider.Now : payment.CompletedDate;
        if (string.Equals(payment.Status, B2bWorkflowStatuses.Completed, StringComparison.OrdinalIgnoreCase))
        {
            await ApplyPaymentCompletionAsync(payment, cancellationToken);
        }
        payment.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<PaymentTransactionDto>.SuccessResult(MapPayment(payment), "Payment status updated successfully");
    }

    public async Task<ApiResponse<PagedResponse<PaymentOrderDto>>> GetPaymentOrdersAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _paymentOrders.Query()
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.Id);

        var total = await query.CountAsync(cancellationToken);
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return ApiResponse<PagedResponse<PaymentOrderDto>>.SuccessResult(
            new PagedResponse<PaymentOrderDto>(items.Select(MapPaymentOrder).ToList(), total, pageNumber, pageSize),
            "Payment orders retrieved successfully");
    }

    public async Task<ApiResponse<PaymentOrderDto>> CreatePaymentOrderAsync(CreatePaymentOrderDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _orders.Query()
            .FirstOrDefaultAsync(x => x.Id == dto.OrderId && !x.IsDeleted, cancellationToken);
        if (order == null)
        {
            return ApiResponse<PaymentOrderDto>.ErrorResult("Order not found", statusCode: 404);
        }

        var existing = await _paymentOrders.Query(tracking: true)
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.OrderId == order.Id && !x.IsDeleted && x.Status != B2bWorkflowStatuses.Cancelled, cancellationToken);
        if (existing != null)
        {
            return ApiResponse<PaymentOrderDto>.SuccessResult(MapPaymentOrder(existing), "Payment order already exists");
        }

        var customer = await _customers.Query()
            .FirstOrDefaultAsync(x => x.Id == order.CustomerId && !x.IsDeleted, cancellationToken);
        var termDays = dto.PaymentTermDays ?? customer?.PaymentTermDays ?? 0;
        var dueDate = ResolveDueDate(dto.DueDate, termDays);
        var installmentCount = Math.Max(1, dto.InstallmentCount);

        var paymentOrder = new PaymentOrder
        {
            PaymentOrderNumber = BuildPaymentOrderNumber(order.Id),
            OrderId = order.Id,
            CustomerId = order.CustomerId,
            BuyerId = order.BuyerId,
            UserId = order.UserId,
            Status = B2bWorkflowStatuses.Pending,
            Amount = order.GrandTotal,
            PaidAmount = 0,
            RemainingAmount = order.GrandTotal,
            CurrencyCode = NormalizeCurrency(order.CurrencyCode),
            PaymentTermDays = termDays,
            DueDate = dueDate,
            IsDueDateOverridden = dto.DueDate.HasValue,
            InstallmentCount = installmentCount,
            PaymentMethod = Trim(dto.PaymentMethod),
            ProviderKey = Trim(dto.ProviderKey)?.ToUpperInvariant(),
            Notes = Trim(dto.Notes),
            CreatedDate = DateTimeProvider.Now,
            UpdatedDate = DateTimeProvider.Now
        };

        foreach (var installment in BuildInstallments(paymentOrder.Amount, dueDate, installmentCount))
        {
            paymentOrder.Installments.Add(installment);
        }

        await _paymentOrders.AddAsync(paymentOrder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<PaymentOrderDto>.SuccessResult(MapPaymentOrder(paymentOrder), "Payment order created successfully");
    }

    public async Task<ApiResponse<PaymentOrderDto>> UpdatePaymentOrderPlanAsync(long id, UpdatePaymentOrderPlanDto dto, CancellationToken cancellationToken = default)
    {
        var paymentOrder = await _paymentOrders.Query(tracking: true)
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (paymentOrder == null)
        {
            return ApiResponse<PaymentOrderDto>.ErrorResult("Payment order not found", statusCode: 404);
        }

        if (paymentOrder.PaidAmount > 0)
        {
            return ApiResponse<PaymentOrderDto>.ErrorResult("Paid payment order plan cannot be changed", statusCode: 400);
        }

        var termDays = dto.PaymentTermDays ?? paymentOrder.PaymentTermDays ?? 0;
        var dueDate = ResolveDueDate(dto.DueDate, termDays);
        var installmentCount = Math.Max(1, dto.InstallmentCount);

        _paymentInstallments.SoftDeleteRange(paymentOrder.Installments.Select(x => x.Id));
        paymentOrder.Installments.Clear();
        foreach (var installment in BuildInstallments(paymentOrder.Amount, dueDate, installmentCount))
        {
            paymentOrder.Installments.Add(installment);
        }

        paymentOrder.PaymentTermDays = termDays;
        paymentOrder.DueDate = dueDate;
        paymentOrder.IsDueDateOverridden = dto.DueDate.HasValue;
        paymentOrder.InstallmentCount = installmentCount;
        paymentOrder.PaymentMethod = Trim(dto.PaymentMethod) ?? paymentOrder.PaymentMethod;
        paymentOrder.ProviderKey = Trim(dto.ProviderKey)?.ToUpperInvariant() ?? paymentOrder.ProviderKey;
        paymentOrder.Notes = Trim(dto.Notes) ?? paymentOrder.Notes;
        paymentOrder.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<PaymentOrderDto>.SuccessResult(MapPaymentOrder(paymentOrder), "Payment order plan updated successfully");
    }

    public async Task<ApiResponse<PaymentOrderDto>> SelectPaymentProviderInstallmentAsync(long id, SelectPaymentProviderInstallmentDto dto, CancellationToken cancellationToken = default)
    {
        var paymentOrder = await _paymentOrders.Query(tracking: true)
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (paymentOrder == null)
        {
            return ApiResponse<PaymentOrderDto>.ErrorResult("Payment order not found", statusCode: 404);
        }

        if (paymentOrder.PaidAmount > 0)
        {
            return ApiResponse<PaymentOrderDto>.ErrorResult("Paid payment order provider plan cannot be changed", statusCode: 400);
        }

        var installmentNumber = Math.Max(1, dto.InstallmentNumber);
        var totalPrice = dto.TotalPrice > 0 ? dto.TotalPrice : paymentOrder.Amount;
        var dueDate = paymentOrder.DueDate;

        _paymentInstallments.SoftDeleteRange(paymentOrder.Installments.Select(x => x.Id));
        paymentOrder.Installments.Clear();
        foreach (var installment in BuildInstallments(totalPrice, dueDate, installmentNumber))
        {
            paymentOrder.Installments.Add(installment);
        }

        paymentOrder.ProviderKey = Normalize(dto.ProviderKey);
        paymentOrder.PaymentMethod = $"{paymentOrder.ProviderKey} Kart";
        paymentOrder.ProviderConversationId = Trim(dto.ProviderConversationId);
        paymentOrder.BinNumber = NormalizeBinOrNull(dto.BinNumber);
        paymentOrder.CardType = Trim(dto.CardType);
        paymentOrder.CardAssociation = Trim(dto.CardAssociation);
        paymentOrder.CardFamily = Trim(dto.CardFamily);
        paymentOrder.BankName = Trim(dto.BankName);
        paymentOrder.BankCode = Trim(dto.BankCode);
        paymentOrder.IsCommercialCard = dto.IsCommercialCard;
        paymentOrder.InstallmentCount = installmentNumber;
        paymentOrder.ProviderInstallmentNumber = installmentNumber;
        paymentOrder.ProviderInstallmentPrice = dto.InstallmentPrice;
        paymentOrder.ProviderTotalPrice = totalPrice;
        paymentOrder.ProviderRate = dto.ProviderRate;
        paymentOrder.ProviderCommissionAmount = dto.ProviderCommissionAmount ?? Math.Max(0, totalPrice - paymentOrder.Amount);
        paymentOrder.ProviderInstallmentSnapshotJson = dto.ProviderInstallmentSnapshotJson ?? JsonSerializer.Serialize(dto, JsonOptions);
        paymentOrder.RemainingAmount = Math.Max(0, totalPrice - paymentOrder.PaidAmount);
        paymentOrder.SetUpdatedInfo();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<PaymentOrderDto>.SuccessResult(MapPaymentOrder(paymentOrder), "Payment provider installment selected successfully");
    }

    private async Task<B2bCart> GetOrCreateDraftCart(long customerId, long? userId, long? buyerId, string currencyCode, CancellationToken cancellationToken)
    {
        var cart = await _carts.Query(tracking: true)
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.UserId == userId && x.BuyerId == buyerId && x.Status == B2bWorkflowStatuses.Draft && !x.IsDeleted, cancellationToken);
        if (cart != null) return cart;

        cart = new B2bCart
        {
            CustomerId = customerId,
            BuyerId = buyerId,
            UserId = userId,
            CurrencyCode = NormalizeCurrency(currencyCode),
            Status = B2bWorkflowStatuses.Draft
        };
        await _carts.AddAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return cart;
    }

    private static IQueryable<T> ApplyPortalScope<T>(IQueryable<T> query, bool includeCompanyHistory, long? userId, long? buyerId)
        where T : class
    {
        if (includeCompanyHistory)
        {
            return query;
        }

        return query.Where(x =>
            (buyerId.HasValue && EF.Property<long?>(x, nameof(B2bCart.BuyerId)) == buyerId.Value) ||
            (userId.HasValue && EF.Property<long?>(x, nameof(B2bCart.UserId)) == userId.Value));
    }

    private async Task<ApiResponse<B2bPriceAvailabilityDto>> ResolveCartLineAsync(B2bCart cart, B2bCartLine line, decimal quantity, CancellationToken cancellationToken)
    {
        var resolved = await _pricingAvailabilityResolver.ResolveAsync(new ResolveB2bPriceAvailabilityDto
        {
            CustomerId = cart.CustomerId,
            CatalogProductId = line.CatalogProductId,
            CatalogVariantId = line.CatalogVariantId,
            ErpStockId = line.ErpStockId,
            WarehouseCode = line.WarehouseCode,
            Quantity = quantity,
            CurrencyCode = line.CurrencyCode
        }, cancellationToken);

        if (!resolved.Success || resolved.Data == null)
        {
            return resolved;
        }

        if (!resolved.Data.IsPriceResolved)
        {
            return ApiResponse<B2bPriceAvailabilityDto>.ErrorResult("No effective B2B price was found", statusCode: 400);
        }

        if (!resolved.Data.IsAvailable)
        {
            return ApiResponse<B2bPriceAvailabilityDto>.ErrorResult("Requested quantity is not available", statusCode: 409);
        }

        return resolved;
    }

    private async Task<ApiResponse<bool>> ReserveInventoryAsync(B2bCartLine line, decimal quantity, bool allowBackorder, CancellationToken cancellationToken)
    {
        if (quantity <= 0 || line.WarehouseCode == null)
        {
            return ApiResponse<bool>.SuccessResult(true, "No reservation required");
        }

        var snapshot = await FindLatestInventorySnapshotAsync(line, tracking: true, cancellationToken);
        if (snapshot == null)
        {
            return allowBackorder
                ? ApiResponse<bool>.SuccessResult(true, "Inventory snapshot not found; backorder allowed")
                : ApiResponse<bool>.ErrorResult("Inventory snapshot not found for reservation", statusCode: 409);
        }

        var availableToSell = Math.Max(0, snapshot.AvailableQuantity - snapshot.ReservedQuantity);
        if (!allowBackorder && availableToSell < quantity)
        {
            return ApiResponse<bool>.ErrorResult("Requested quantity is no longer available for reservation", statusCode: 409);
        }

        snapshot.ReservedQuantity += quantity;
        snapshot.SetUpdatedInfo();
        return ApiResponse<bool>.SuccessResult(true, "Inventory reserved");
    }

    private async Task ReleaseInventoryAsync(B2bCartLine line, decimal quantity, CancellationToken cancellationToken)
    {
        if (quantity <= 0 || line.WarehouseCode == null)
        {
            return;
        }

        var snapshot = await FindLatestInventorySnapshotAsync(line, tracking: true, cancellationToken);
        if (snapshot == null)
        {
            return;
        }

        snapshot.ReservedQuantity = Math.Max(0, snapshot.ReservedQuantity - quantity);
        snapshot.SetUpdatedInfo();
    }

    private Task<InventorySnapshot?> FindLatestInventorySnapshotAsync(B2bCartLine line, bool tracking, CancellationToken cancellationToken)
    {
        var query = _inventory.Query(tracking).Where(x => !x.IsDeleted && x.WarehouseCode == line.WarehouseCode);
        query = query.Where(x =>
            (line.CatalogVariantId.HasValue && x.CatalogVariantId == line.CatalogVariantId.Value) ||
            (line.CatalogProductId.HasValue && x.CatalogProductId == line.CatalogProductId.Value) ||
            (line.ErpStockId.HasValue && x.ErpStockId == line.ErpStockId.Value));

        return query.OrderByDescending(x => x.SnapshotDate).FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<ApiResponse<bool>> ValidateFavoriteScopeAsync(long companyId, long? buyerId, CancellationToken cancellationToken)
    {
        if (companyId <= 0)
        {
            return ApiResponse<bool>.ErrorResult("Company is required for favorites", statusCode: 400);
        }

        var companyExists = await _companies.Query()
            .AnyAsync(x => x.Id == companyId && !x.IsDeleted && x.Status == "Active", cancellationToken);
        if (!companyExists)
        {
            return ApiResponse<bool>.ErrorResult("B2B company not found", statusCode: 404);
        }

        if (!buyerId.HasValue)
        {
            return ApiResponse<bool>.SuccessResult(true, "Favorite scope is valid");
        }

        var buyerExists = await _buyers.Query()
            .AnyAsync(x => x.Id == buyerId.Value && x.CompanyId == companyId && !x.IsDeleted && x.IsActive, cancellationToken);
        return buyerExists
            ? ApiResponse<bool>.SuccessResult(true, "Favorite scope is valid")
            : ApiResponse<bool>.ErrorResult("B2B buyer not found for company", statusCode: 404);
    }

    private async Task<ApiResponse<ProductFavoriteTarget>> ResolveProductFavoriteTargetAsync(ToggleCatalogProductFavoriteDto dto, CancellationToken cancellationToken)
    {
        if (dto.CatalogVariantId.HasValue)
        {
            var variant = await _catalogVariants.Query()
                .Include(x => x.CatalogProduct)
                .FirstOrDefaultAsync(x => x.Id == dto.CatalogVariantId.Value && !x.IsDeleted && x.IsActive, cancellationToken);
            if (variant?.CatalogProduct == null || variant.CatalogProduct.IsDeleted)
            {
                return ApiResponse<ProductFavoriteTarget>.ErrorResult("Catalog variant not found", statusCode: 404);
            }

            return ApiResponse<ProductFavoriteTarget>.SuccessResult(new ProductFavoriteTarget
            {
                FavoriteKey = $"V:{variant.Id}",
                CatalogProductId = variant.CatalogProductId,
                CatalogVariantId = variant.Id,
                ErpStockId = variant.ErpStockId,
                Sku = variant.VariantSku
            }, "Favorite target resolved");
        }

        if (dto.CatalogProductId.HasValue)
        {
            var product = await _catalogProducts.Query()
                .FirstOrDefaultAsync(x => x.Id == dto.CatalogProductId.Value && !x.IsDeleted, cancellationToken);
            if (product == null)
            {
                return ApiResponse<ProductFavoriteTarget>.ErrorResult("Catalog product not found", statusCode: 404);
            }

            return ApiResponse<ProductFavoriteTarget>.SuccessResult(new ProductFavoriteTarget
            {
                FavoriteKey = $"P:{product.Id}",
                CatalogProductId = product.Id,
                ErpStockId = product.DefaultStockId,
                Sku = product.Sku
            }, "Favorite target resolved");
        }

        if (dto.ErpStockId.HasValue)
        {
            return ApiResponse<ProductFavoriteTarget>.SuccessResult(new ProductFavoriteTarget
            {
                FavoriteKey = $"S:{dto.ErpStockId.Value}",
                ErpStockId = dto.ErpStockId,
                Sku = Trim(dto.Sku)
            }, "Favorite target resolved");
        }

        if (!string.IsNullOrWhiteSpace(dto.Sku))
        {
            var sku = Normalize(dto.Sku);
            var product = await _catalogProducts.Query()
                .FirstOrDefaultAsync(x => x.Sku == sku && !x.IsDeleted, cancellationToken);

            return ApiResponse<ProductFavoriteTarget>.SuccessResult(new ProductFavoriteTarget
            {
                FavoriteKey = $"SKU:{sku}",
                CatalogProductId = product?.Id,
                ErpStockId = product?.DefaultStockId,
                Sku = sku
            }, "Favorite target resolved");
        }

        return ApiResponse<ProductFavoriteTarget>.ErrorResult("Product, variant, ERP stock or SKU is required for favorites", statusCode: 400);
    }

    private sealed class ProductFavoriteTarget
    {
        public string FavoriteKey { get; set; } = string.Empty;
        public long? CatalogProductId { get; set; }
        public long? CatalogVariantId { get; set; }
        public long? ErpStockId { get; set; }
        public string? Sku { get; set; }
    }

    private async Task RefreshCatalogQualityAsync(long productId, CancellationToken cancellationToken)
    {
        var product = await _catalogProducts.Query(tracking: true)
            .FirstOrDefaultAsync(x => x.Id == productId && !x.IsDeleted, cancellationToken);
        if (product == null)
        {
            return;
        }

        product.CompletenessScore = CalculateCatalogCompleteness(product);
        product.SearchText = BuildSearchText(product);
        product.SetUpdatedInfo();
    }

    private static CatalogProductDto MapCatalogProduct(CatalogProduct entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        Sku = entity.Sku,
        Name = entity.Name,
        Slug = entity.Slug,
        Brand = entity.Brand,
        ProductType = entity.ProductType,
        ManufacturerCode = entity.ManufacturerCode,
        Barcode = entity.Barcode,
        Unit = entity.Unit,
        CategoryPath = entity.CategoryPath,
        ShortDescription = entity.ShortDescription,
        Description = entity.Description,
        PrimaryImageUrl = entity.PrimaryImageUrl,
        BulletPointsJson = entity.BulletPointsJson,
        AttributesJson = entity.AttributesJson,
        MediaGalleryJson = entity.MediaGalleryJson,
        DocumentsJson = entity.DocumentsJson,
        MetaTitle = entity.MetaTitle,
        MetaDescription = entity.MetaDescription,
        SearchKeywords = entity.SearchKeywords,
        MinOrderQuantity = entity.MinOrderQuantity,
        PackageQuantity = entity.PackageQuantity,
        SortOrder = entity.SortOrder,
        CompletenessScore = entity.CompletenessScore,
        IsPublished = entity.IsPublished,
        DefaultStockId = entity.DefaultStockId,
        PublishedDate = entity.PublishedDate,
        Variants = entity.Variants.Where(x => !x.IsDeleted).Select(MapCatalogVariant).ToList(),
        Categories = entity.ProductCategories.Where(x => !x.IsDeleted).Select(x => MapCatalogProductCategory(x, x.CatalogCategory)).ToList(),
        Attributes = entity.ProductAttributes.Where(x => !x.IsDeleted).Select(x => MapCatalogProductAttribute(x, x.AttributeDefinition)).ToList(),
        MediaItems = entity.MediaItems.Where(x => !x.IsDeleted).Select(MapCatalogProductMedia).ToList(),
        Documents = entity.Documents.Where(x => !x.IsDeleted).Select(MapCatalogProductDocument).ToList()
    };

    private static CatalogVariantDto MapCatalogVariant(CatalogVariant entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CatalogProductId = entity.CatalogProductId,
        ErpStockId = entity.ErpStockId,
        VariantSku = entity.VariantSku,
        VariantName = entity.VariantName,
        Barcode = entity.Barcode,
        Unit = entity.Unit,
        AttributesJson = entity.AttributesJson,
        MediaGalleryJson = entity.MediaGalleryJson,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };

    private static CatalogCategoryDto MapCatalogCategory(CatalogCategory entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        ParentCategoryId = entity.ParentCategoryId,
        Code = entity.Code,
        Name = entity.Name,
        Description = entity.Description,
        Level = entity.Level,
        FullPath = entity.FullPath,
        SortOrder = entity.SortOrder,
        ImageUrl = entity.ImageUrl,
        IconName = entity.IconName,
        ColorHex = entity.ColorHex,
        IsLeaf = entity.IsLeaf,
        IsActive = entity.IsActive
    };

    private static CatalogProductFavoriteDto MapCatalogProductFavorite(CatalogProductFavorite entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CompanyId = entity.CompanyId,
        CompanyName = entity.Company?.CompanyName,
        BuyerId = entity.BuyerId,
        BuyerName = entity.Buyer?.FullName,
        UserId = entity.UserId,
        CatalogProductId = entity.CatalogProductId,
        CatalogVariantId = entity.CatalogVariantId,
        ErpStockId = entity.ErpStockId,
        FavoriteKey = entity.FavoriteKey,
        Sku = entity.Sku ?? entity.CatalogVariant?.VariantSku ?? entity.CatalogProduct?.Sku,
        ProductName = entity.CatalogVariant?.VariantName ?? entity.CatalogProduct?.Name,
        ProductImageUrl = entity.CatalogProduct?.PrimaryImageUrl,
        Brand = entity.CatalogProduct?.Brand,
        CategoryPath = entity.CatalogProduct?.CategoryPath,
        VariantName = entity.CatalogVariant?.VariantName,
        Note = entity.Note
    };

    private static CatalogCategoryFavoriteDto MapCatalogCategoryFavorite(CatalogCategoryFavorite entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CompanyId = entity.CompanyId,
        CompanyName = entity.Company?.CompanyName,
        BuyerId = entity.BuyerId,
        BuyerName = entity.Buyer?.FullName,
        UserId = entity.UserId,
        CatalogCategoryId = entity.CatalogCategoryId,
        CategoryCode = entity.CatalogCategory?.Code,
        CategoryName = entity.CatalogCategory?.Name,
        CategoryFullPath = entity.CatalogCategory?.FullPath,
        ImageUrl = entity.CatalogCategory?.ImageUrl,
        Note = entity.Note
    };

    private static CatalogProductCategoryDto MapCatalogProductCategory(CatalogProductCategory entity, CatalogCategory? category) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CatalogProductId = entity.CatalogProductId,
        CatalogCategoryId = entity.CatalogCategoryId,
        CategoryCode = category?.Code,
        CategoryName = category?.Name,
        CategoryFullPath = category?.FullPath,
        IsPrimary = entity.IsPrimary,
        SortOrder = entity.SortOrder,
        AssignmentSource = entity.AssignmentSource
    };

    private static CatalogAttributeDefinitionDto MapCatalogAttributeDefinition(CatalogAttributeDefinition entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CatalogCategoryId = entity.CatalogCategoryId,
        Code = entity.Code,
        Name = entity.Name,
        DataType = entity.DataType,
        IsRequired = entity.IsRequired,
        IsFilterable = entity.IsFilterable,
        IsComparable = entity.IsComparable,
        Unit = entity.Unit,
        AllowedValuesJson = entity.AllowedValuesJson,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };

    private static CatalogProductAttributeDto MapCatalogProductAttribute(CatalogProductAttribute entity, CatalogAttributeDefinition? definition) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CatalogProductId = entity.CatalogProductId,
        AttributeDefinitionId = entity.AttributeDefinitionId,
        AttributeCode = definition?.Code,
        AttributeName = definition?.Name,
        DataType = definition?.DataType,
        Value = entity.Value,
        NormalizedValue = entity.NormalizedValue,
        Unit = entity.Unit,
        SortOrder = entity.SortOrder
    };

    private static CatalogProductMediaDto MapCatalogProductMedia(CatalogProductMedia entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CatalogProductId = entity.CatalogProductId,
        Url = entity.Url,
        MediaType = entity.MediaType,
        AltText = entity.AltText,
        IsPrimary = entity.IsPrimary,
        SortOrder = entity.SortOrder
    };

    private static CatalogProductDocumentDto MapCatalogProductDocument(CatalogProductDocument entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CatalogProductId = entity.CatalogProductId,
        Name = entity.Name,
        Url = entity.Url,
        DocumentType = entity.DocumentType,
        LanguageCode = entity.LanguageCode,
        SortOrder = entity.SortOrder,
        IsActive = entity.IsActive
    };

    private static CustomerProductAliasDto MapAlias(CustomerProductAlias entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CustomerId = entity.CustomerId,
        ErpStockId = entity.ErpStockId,
        CatalogProductId = entity.CatalogProductId,
        CustomerSku = entity.CustomerSku,
        CustomerProductName = entity.CustomerProductName,
        MatchStatus = entity.MatchStatus,
        ConfidenceScore = entity.ConfidenceScore,
        Notes = entity.Notes,
        MatchedDate = entity.MatchedDate
    };

    private static CartDto MapCart(B2bCart entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CustomerId = entity.CustomerId,
        BuyerId = entity.BuyerId,
        UserId = entity.UserId,
        Status = entity.Status,
        CurrencyCode = entity.CurrencyCode,
        Lines = entity.Lines.Where(x => !x.IsDeleted).Select(MapCartLine).ToList()
    };

    private static CartLineDto MapCartLine(B2bCartLine entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CartId = entity.CartId,
        CatalogProductId = entity.CatalogProductId,
        CatalogVariantId = entity.CatalogVariantId,
        ErpStockId = entity.ErpStockId,
        WarehouseCode = entity.WarehouseCode,
        Quantity = entity.Quantity,
        UnitPrice = entity.UnitPrice,
        CurrencyCode = entity.CurrencyCode,
        PriceSource = entity.PriceSource,
        PriceListId = entity.PriceListId,
        DiscountRate = entity.DiscountRate,
        VatRate = entity.VatRate,
        VatAmount = entity.VatAmount,
        ExchangeRate = entity.ExchangeRate,
        PriceResolvedAt = entity.PriceResolvedAt
    };

    private static OrderDto MapOrder(B2bOrder entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        OrderNumber = entity.OrderNumber,
        CustomerId = entity.CustomerId,
        BuyerId = entity.BuyerId,
        UserId = entity.UserId,
        Status = entity.Status,
        CurrencyCode = entity.CurrencyCode,
        OfferType = entity.OfferType,
        OfferDate = entity.OfferDate,
        OfferNo = entity.OfferNo,
        RevisionNo = entity.RevisionNo,
        RevisionId = entity.RevisionId,
        ValidUntil = entity.ValidUntil,
        DeliveryDate = entity.DeliveryDate,
        DeliveryMethod = entity.DeliveryMethod,
        PaymentTypeId = entity.PaymentTypeId,
        QuoteRequestId = entity.QuoteRequestId,
        ErpProjectCode = entity.ErpProjectCode,
        GeneralDiscountRate = entity.GeneralDiscountRate,
        GeneralDiscountAmount = entity.GeneralDiscountAmount,
        Subtotal = entity.Subtotal,
        TaxTotal = entity.TaxTotal,
        GrandTotal = entity.GrandTotal,
        Description = entity.Description,
        ExternalErpOrderNumber = entity.ExternalErpOrderNumber,
        SubmittedDate = entity.SubmittedDate,
        Lines = entity.Lines.Where(x => !x.IsDeleted).Select(MapOrderLine).ToList()
    };

    private static OrderLineDto MapOrderLine(B2bOrderLine entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        OrderId = entity.OrderId,
        CatalogProductId = entity.CatalogProductId,
        CatalogVariantId = entity.CatalogVariantId,
        ErpStockId = entity.ErpStockId,
        WarehouseCode = entity.WarehouseCode,
        ProductSku = entity.ProductSku,
        ProductName = entity.ProductName,
        Quantity = entity.Quantity,
        UnitPrice = entity.UnitPrice,
        DiscountRate1 = entity.DiscountRate1,
        DiscountAmount1 = entity.DiscountAmount1,
        DiscountRate2 = entity.DiscountRate2,
        DiscountAmount2 = entity.DiscountAmount2,
        DiscountRate3 = entity.DiscountRate3,
        DiscountAmount3 = entity.DiscountAmount3,
        VatRate = entity.VatRate,
        VatAmount = entity.VatAmount,
        LineTotal = entity.LineTotal,
        LineGrandTotal = entity.LineGrandTotal,
        PriceSource = entity.PriceSource,
        PriceListId = entity.PriceListId,
        ExchangeRate = entity.ExchangeRate,
        PriceResolvedAt = entity.PriceResolvedAt,
        Description = entity.Description,
        Description1 = entity.Description1,
        Description2 = entity.Description2,
        Description3 = entity.Description3,
        PricingRuleHeaderId = entity.PricingRuleHeaderId,
        RelatedProductKey = entity.RelatedProductKey,
        IsMainRelatedProduct = entity.IsMainRelatedProduct,
        ErpProjectCode = entity.ErpProjectCode
    };

    private static PaymentTransactionDto MapPayment(PaymentTransaction entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        OrderId = entity.OrderId,
        PaymentOrderId = entity.PaymentOrderId,
        PaymentInstallmentId = entity.PaymentInstallmentId,
        ProviderKey = entity.ProviderKey,
        ExternalTransactionId = entity.ExternalTransactionId,
        Status = entity.Status,
        Amount = entity.Amount,
        ProviderPaymentAmount = entity.ProviderPaymentAmount,
        ProviderCollectedAmount = entity.ProviderCollectedAmount,
        CurrencyCode = entity.CurrencyCode,
        PaymentMethod = entity.PaymentMethod,
        DueDate = entity.DueDate,
        PaymentTermDays = entity.PaymentTermDays,
        InstallmentCount = entity.InstallmentCount,
        InstallmentPlanJson = entity.InstallmentPlanJson,
        ProviderConversationId = entity.ProviderConversationId,
        BinNumber = entity.BinNumber,
        CardType = entity.CardType,
        CardAssociation = entity.CardAssociation,
        CardFamily = entity.CardFamily,
        BankName = entity.BankName,
        BankCode = entity.BankCode,
        IsCommercialCard = entity.IsCommercialCard,
        ProviderRate = entity.ProviderRate,
        ProviderCommissionAmount = entity.ProviderCommissionAmount,
        RequestedDate = entity.RequestedDate,
        CompletedDate = entity.CompletedDate
    };

    private static PaymentOrderDto MapPaymentOrder(PaymentOrder entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        PaymentOrderNumber = entity.PaymentOrderNumber,
        OrderId = entity.OrderId,
        CustomerId = entity.CustomerId,
        BuyerId = entity.BuyerId,
        UserId = entity.UserId,
        Status = entity.Status,
        Amount = entity.Amount,
        PaidAmount = entity.PaidAmount,
        RemainingAmount = entity.RemainingAmount,
        CurrencyCode = entity.CurrencyCode,
        PaymentTermDays = entity.PaymentTermDays,
        DueDate = entity.DueDate,
        IsDueDateOverridden = entity.IsDueDateOverridden,
        InstallmentCount = entity.InstallmentCount,
        PaymentMethod = entity.PaymentMethod,
        ProviderKey = entity.ProviderKey,
        ProviderConversationId = entity.ProviderConversationId,
        BinNumber = entity.BinNumber,
        CardType = entity.CardType,
        CardAssociation = entity.CardAssociation,
        CardFamily = entity.CardFamily,
        BankName = entity.BankName,
        BankCode = entity.BankCode,
        IsCommercialCard = entity.IsCommercialCard,
        ProviderInstallmentNumber = entity.ProviderInstallmentNumber,
        ProviderInstallmentPrice = entity.ProviderInstallmentPrice,
        ProviderTotalPrice = entity.ProviderTotalPrice,
        ProviderRate = entity.ProviderRate,
        ProviderCommissionAmount = entity.ProviderCommissionAmount,
        ProviderInstallmentSnapshotJson = entity.ProviderInstallmentSnapshotJson,
        Notes = entity.Notes,
        Installments = entity.Installments
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.InstallmentNumber)
            .Select(MapPaymentInstallment)
            .ToList()
    };

    private static PaymentInstallmentDto MapPaymentInstallment(PaymentInstallment entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        PaymentOrderId = entity.PaymentOrderId,
        InstallmentNumber = entity.InstallmentNumber,
        Status = entity.Status,
        DueDate = entity.DueDate,
        Amount = entity.Amount,
        PaidAmount = entity.PaidAmount,
        PaidDate = entity.PaidDate,
        Notes = entity.Notes
    };

    private async Task ApplyPaymentCompletionAsync(PaymentTransaction payment, CancellationToken cancellationToken)
    {
        if (!payment.PaymentOrderId.HasValue)
        {
            return;
        }

        var paymentOrder = await _paymentOrders.Query(tracking: true)
            .Include(x => x.Installments.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == payment.PaymentOrderId.Value && !x.IsDeleted, cancellationToken);
        if (paymentOrder == null)
        {
            return;
        }

        var collectedAmount = payment.ProviderCollectedAmount ?? payment.Amount;
        paymentOrder.PaidAmount = Math.Min(paymentOrder.Amount, paymentOrder.PaidAmount + collectedAmount);
        paymentOrder.RemainingAmount = Math.Max(0, paymentOrder.Amount - paymentOrder.PaidAmount);
        paymentOrder.Status = paymentOrder.RemainingAmount <= 0 ? B2bWorkflowStatuses.Completed : B2bWorkflowStatuses.Processing;
        paymentOrder.SetUpdatedInfo();

        if (payment.PaymentInstallmentId.HasValue)
        {
            var installment = paymentOrder.Installments.FirstOrDefault(x => x.Id == payment.PaymentInstallmentId.Value);
            if (installment != null)
            {
                installment.PaidAmount = Math.Min(installment.Amount, installment.PaidAmount + collectedAmount);
                installment.Status = installment.PaidAmount >= installment.Amount ? B2bWorkflowStatuses.Completed : B2bWorkflowStatuses.Processing;
                installment.PaidDate = installment.Status == B2bWorkflowStatuses.Completed ? DateTimeProvider.Now : installment.PaidDate;
                installment.SetUpdatedInfo();
            }
        }
        else
        {
            AllocatePaymentToInstallments(paymentOrder.Installments.Where(x => !x.IsDeleted).OrderBy(x => x.InstallmentNumber), collectedAmount);
        }
    }

    private static void AllocatePaymentToInstallments(IEnumerable<PaymentInstallment> installments, decimal amount)
    {
        var remaining = amount;
        foreach (var installment in installments)
        {
            if (remaining <= 0)
            {
                return;
            }

            var openAmount = Math.Max(0, installment.Amount - installment.PaidAmount);
            if (openAmount <= 0)
            {
                continue;
            }

            var paid = Math.Min(openAmount, remaining);
            installment.PaidAmount += paid;
            installment.Status = installment.PaidAmount >= installment.Amount ? B2bWorkflowStatuses.Completed : B2bWorkflowStatuses.Processing;
            installment.PaidDate = installment.Status == B2bWorkflowStatuses.Completed ? DateTimeProvider.Now : installment.PaidDate;
            installment.SetUpdatedInfo();
            remaining -= paid;
        }
    }

    private static List<PaymentInstallment> BuildInstallments(decimal amount, DateTime dueDate, int installmentCount)
    {
        installmentCount = Math.Max(1, installmentCount);
        var installments = new List<PaymentInstallment>(installmentCount);
        var baseAmount = Math.Round(amount / installmentCount, 4);
        var allocated = 0m;
        for (var i = 1; i <= installmentCount; i++)
        {
            var installmentAmount = i == installmentCount ? amount - allocated : baseAmount;
            allocated += installmentAmount;
            installments.Add(new PaymentInstallment
            {
                InstallmentNumber = i,
                Status = B2bWorkflowStatuses.Pending,
                DueDate = dueDate.Date.AddMonths(i - 1),
                Amount = installmentAmount,
                PaidAmount = 0,
                CreatedDate = DateTimeProvider.Now,
                UpdatedDate = DateTimeProvider.Now
            });
        }

        return installments;
    }

    private static DateTime ResolveDueDate(DateTime? requestedDueDate, short? paymentTermDays)
    {
        return requestedDueDate?.Date ?? DateTimeProvider.Now.Date.AddDays(Math.Max(0, (int)(paymentTermDays ?? 0)));
    }

    private static string BuildPaymentOrderNumber(long orderId)
    {
        return $"PO-{orderId}-{DateTimeProvider.Now:yyyyMMddHHmmssfff}";
    }

    private static string BuildInstallmentPlanJson(IEnumerable<PaymentInstallment> installments)
    {
        return JsonSerializer.Serialize(installments
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.InstallmentNumber)
            .Select(x => new
            {
                x.InstallmentNumber,
                x.DueDate,
                x.Amount,
                x.PaidAmount,
                x.Status
            }));
    }

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string? NormalizeBinOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : new string(value.Where(char.IsDigit).Take(8).ToArray());
    private static string NormalizeAttributeValue(string value) => value.Trim().ToUpperInvariant();
    private static string BuildCategoryFullPath(CatalogCategory? parent, string name)
    {
        var normalizedName = name.Trim();
        return string.IsNullOrWhiteSpace(parent?.FullPath) ? normalizedName : $"{parent.FullPath} / {normalizedName}";
    }
    private static string NormalizeCurrency(string value) => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();
    private static string NormalizeStatus(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    private static bool IsMatched(string? status) => string.Equals(status, "Matched", StringComparison.OrdinalIgnoreCase);
    private static bool IsFinalPaymentStatus(string status) => status is B2bWorkflowStatuses.Completed or B2bWorkflowStatuses.Failed or B2bWorkflowStatuses.Cancelled;
    private static string NormalizeSlug(string? slug, string name) => (string.IsNullOrWhiteSpace(slug) ? name : slug).Trim().ToLowerInvariant().Replace(' ', '-');
    private static string BuildSearchText(CatalogProduct product) => string.Join(" ", new[]
    {
        product.Sku,
        product.Name,
        product.Brand,
        product.ProductType,
        product.ManufacturerCode,
        product.Barcode,
        product.Unit,
        product.CategoryPath,
        product.ShortDescription,
        product.SearchKeywords
    }.Where(x => !string.IsNullOrWhiteSpace(x)));

    private static int CalculateCatalogCompleteness(CatalogProduct product)
    {
        var score = 0;
        if (!string.IsNullOrWhiteSpace(product.Sku)) score += 10;
        if (!string.IsNullOrWhiteSpace(product.Name)) score += 10;
        if (!string.IsNullOrWhiteSpace(product.Brand)) score += 8;
        if (!string.IsNullOrWhiteSpace(product.ProductType)) score += 8;
        if (!string.IsNullOrWhiteSpace(product.CategoryPath)) score += 10;
        if (!string.IsNullOrWhiteSpace(product.ShortDescription)) score += 8;
        if (!string.IsNullOrWhiteSpace(product.Description)) score += 10;
        if (!string.IsNullOrWhiteSpace(product.PrimaryImageUrl)) score += 8;
        if (!string.IsNullOrWhiteSpace(product.BulletPointsJson)) score += 8;
        if (!string.IsNullOrWhiteSpace(product.AttributesJson)) score += 10;
        if (!string.IsNullOrWhiteSpace(product.DocumentsJson)) score += 5;
        if (product.DefaultStockId.HasValue) score += 5;
        return Math.Min(100, score);
    }
}
