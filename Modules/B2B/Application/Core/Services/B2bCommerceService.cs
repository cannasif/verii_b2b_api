using Microsoft.EntityFrameworkCore;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class B2bCommerceService : IB2bCommerceService
{
    private readonly IRepository<CatalogProduct> _catalogProducts;
    private readonly IRepository<CatalogVariant> _catalogVariants;
    private readonly IRepository<CustomerProductAlias> _aliases;
    private readonly IRepository<B2bCart> _carts;
    private readonly IRepository<B2bCartLine> _cartLines;
    private readonly IRepository<B2bOrder> _orders;
    private readonly IRepository<B2bOrderLine> _orderLines;
    private readonly IRepository<PaymentTransaction> _payments;
    private readonly IRepository<QuoteRequest> _quotes;
    private readonly IRepository<InventorySnapshot> _inventory;
    private readonly IB2bPricingAvailabilityResolver _pricingAvailabilityResolver;
    private readonly IUnitOfWork _unitOfWork;

    public B2bCommerceService(
        IRepository<CatalogProduct> catalogProducts,
        IRepository<CatalogVariant> catalogVariants,
        IRepository<CustomerProductAlias> aliases,
        IRepository<B2bCart> carts,
        IRepository<B2bCartLine> cartLines,
        IRepository<B2bOrder> orders,
        IRepository<B2bOrderLine> orderLines,
        IRepository<PaymentTransaction> payments,
        IRepository<QuoteRequest> quotes,
        IRepository<InventorySnapshot> inventory,
        IB2bPricingAvailabilityResolver pricingAvailabilityResolver,
        IUnitOfWork unitOfWork)
    {
        _catalogProducts = catalogProducts;
        _catalogVariants = catalogVariants;
        _aliases = aliases;
        _carts = carts;
        _cartLines = cartLines;
        _orders = orders;
        _orderLines = orderLines;
        _payments = payments;
        _quotes = quotes;
        _inventory = inventory;
        _pricingAvailabilityResolver = pricingAvailabilityResolver;
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
            .Where(x => !x.IsDeleted);

        if (publishedOnly)
        {
            query = query.Where(x => x.IsPublished);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Sku.Contains(search) || x.Name.Contains(search) || (x.SearchText != null && x.SearchText.Contains(search)));
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
            CategoryPath = Trim(dto.CategoryPath),
            Description = dto.Description,
            PrimaryImageUrl = Trim(dto.PrimaryImageUrl),
            IsPublished = dto.IsPublished,
            DefaultStockId = dto.DefaultStockId,
            PublishedDate = dto.IsPublished ? now : null,
            SearchText = BuildSearchText(sku, dto.Name, dto.Brand, dto.CategoryPath),
            CreatedDate = now
        };

        await _catalogProducts.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogProductDto>.SuccessResult(MapCatalogProduct(entity), "Catalog product created successfully");
    }

    public async Task<ApiResponse<CatalogProductDto>> UpdateCatalogProductAsync(long id, UpdateCatalogProductDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _catalogProducts.Query(tracking: true)
            .Include(x => x.Variants.Where(v => !v.IsDeleted))
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
        entity.CategoryPath = dto.CategoryPath ?? entity.CategoryPath;
        entity.Description = dto.Description ?? entity.Description;
        entity.PrimaryImageUrl = dto.PrimaryImageUrl ?? entity.PrimaryImageUrl;
        entity.DefaultStockId = dto.DefaultStockId ?? entity.DefaultStockId;
        if (dto.IsPublished.HasValue && entity.IsPublished != dto.IsPublished.Value)
        {
            entity.IsPublished = dto.IsPublished.Value;
            entity.PublishedDate = dto.IsPublished.Value ? DateTimeProvider.Now : null;
        }
        entity.SearchText = BuildSearchText(entity.Sku, entity.Name, entity.Brand, entity.CategoryPath);
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
        variant.AttributesJson = dto.AttributesJson;
        variant.IsActive = dto.IsActive;
        variant.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogVariantDto>.SuccessResult(MapCatalogVariant(variant), "Catalog variant saved successfully");
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

    public async Task<ApiResponse<CartDto>> GetDraftCartAsync(long customerId, long? userId = null, CancellationToken cancellationToken = default)
    {
        var cart = await GetOrCreateDraftCart(customerId, userId, "TRY", cancellationToken);
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

            var cart = await GetOrCreateDraftCart(dto.CustomerId, dto.UserId, dto.CurrencyCode, cancellationToken);
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
                CurrencyCode = resolved.Data.CurrencyCode
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
            return await GetDraftCartAsync(dto.CustomerId, dto.UserId, cancellationToken);
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

        result.Cart ??= (await GetDraftCartAsync(dto.CustomerId, dto.UserId, cancellationToken)).Data;
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
                    ErpProjectCode = Trim(dto.ErpProjectCode),
                    LineTotal = line.Quantity * line.UnitPrice,
                    LineGrandTotal = line.Quantity * line.UnitPrice
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

    public async Task<ApiResponse<CustomerPortalSummaryDto>> GetCustomerPortalSummaryAsync(long customerId, long? userId = null, CancellationToken cancellationToken = default)
    {
        if (customerId <= 0)
        {
            return ApiResponse<CustomerPortalSummaryDto>.ErrorResult("Customer is required", statusCode: 400);
        }

        var draftCart = await _carts.Query()
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .Where(x => !x.IsDeleted && x.CustomerId == customerId && x.Status == B2bWorkflowStatuses.Draft && (!userId.HasValue || x.UserId == userId.Value))
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var orders = await _orders.Query()
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .Where(x => !x.IsDeleted && x.CustomerId == customerId)
            .OrderByDescending(x => x.Id)
            .Take(10)
            .ToListAsync(cancellationToken);

        var pendingPayments = await _payments.Query()
            .Include(x => x.Order)
            .Where(x => !x.IsDeleted && x.Order != null && x.Order.CustomerId == customerId && x.Status == B2bWorkflowStatuses.Pending)
            .OrderByDescending(x => x.Id)
            .Take(10)
            .ToListAsync(cancellationToken);

        var openOrderStatuses = new[] { B2bWorkflowStatuses.WaitingPayment, "PendingApproval", B2bWorkflowStatuses.Submitted, B2bWorkflowStatuses.Processing };
        var openOrderTotal = await _orders.Query()
            .Where(x => !x.IsDeleted && x.CustomerId == customerId && openOrderStatuses.Contains(x.Status))
            .SumAsync(x => x.GrandTotal, cancellationToken);

        var summary = new CustomerPortalSummaryDto
        {
            CustomerId = customerId,
            CurrencyCode = draftCart?.CurrencyCode ?? orders.FirstOrDefault()?.CurrencyCode ?? "TRY",
            DraftCartCount = await _carts.Query().CountAsync(x => !x.IsDeleted && x.CustomerId == customerId && x.Status == B2bWorkflowStatuses.Draft, cancellationToken),
            OrderCount = await _orders.Query().CountAsync(x => !x.IsDeleted && x.CustomerId == customerId, cancellationToken),
            OpenOrderCount = await _orders.Query().CountAsync(x => !x.IsDeleted && x.CustomerId == customerId && openOrderStatuses.Contains(x.Status), cancellationToken),
            QuoteCount = await _quotes.Query().CountAsync(x => !x.IsDeleted && x.CustomerId == customerId, cancellationToken),
            PendingQuoteCount = await _quotes.Query().CountAsync(x => !x.IsDeleted && x.CustomerId == customerId && x.Status != B2bWorkflowStatuses.Approved && x.Status != B2bWorkflowStatuses.Rejected && x.Status != B2bWorkflowStatuses.Cancelled, cancellationToken),
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
        var payment = new PaymentTransaction
        {
            OrderId = dto.OrderId,
            ProviderKey = Normalize(dto.ProviderKey),
            ExternalTransactionId = Trim(dto.ExternalTransactionId),
            Status = B2bWorkflowStatuses.Pending,
            Amount = dto.Amount,
            CurrencyCode = NormalizeCurrency(dto.CurrencyCode),
            PaymentMethod = Trim(dto.PaymentMethod),
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
        payment.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<PaymentTransactionDto>.SuccessResult(MapPayment(payment), "Payment status updated successfully");
    }

    private async Task<B2bCart> GetOrCreateDraftCart(long customerId, long? userId, string currencyCode, CancellationToken cancellationToken)
    {
        var cart = await _carts.Query(tracking: true)
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(x => x.CustomerId == customerId && x.UserId == userId && x.Status == B2bWorkflowStatuses.Draft && !x.IsDeleted, cancellationToken);
        if (cart != null) return cart;

        cart = new B2bCart
        {
            CustomerId = customerId,
            UserId = userId,
            CurrencyCode = NormalizeCurrency(currencyCode),
            Status = B2bWorkflowStatuses.Draft
        };
        await _carts.AddAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return cart;
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
        CategoryPath = entity.CategoryPath,
        Description = entity.Description,
        PrimaryImageUrl = entity.PrimaryImageUrl,
        IsPublished = entity.IsPublished,
        DefaultStockId = entity.DefaultStockId,
        PublishedDate = entity.PublishedDate,
        Variants = entity.Variants.Where(x => !x.IsDeleted).Select(MapCatalogVariant).ToList()
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
        AttributesJson = entity.AttributesJson,
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
        CurrencyCode = entity.CurrencyCode
    };

    private static OrderDto MapOrder(B2bOrder entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        OrderNumber = entity.OrderNumber,
        CustomerId = entity.CustomerId,
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
        ProviderKey = entity.ProviderKey,
        ExternalTransactionId = entity.ExternalTransactionId,
        Status = entity.Status,
        Amount = entity.Amount,
        CurrencyCode = entity.CurrencyCode,
        PaymentMethod = entity.PaymentMethod,
        RequestedDate = entity.RequestedDate,
        CompletedDate = entity.CompletedDate
    };

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string NormalizeCurrency(string value) => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();
    private static string NormalizeStatus(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    private static bool IsMatched(string? status) => string.Equals(status, "Matched", StringComparison.OrdinalIgnoreCase);
    private static bool IsFinalPaymentStatus(string status) => status is B2bWorkflowStatuses.Completed or B2bWorkflowStatuses.Failed or B2bWorkflowStatuses.Cancelled;
    private static string NormalizeSlug(string? slug, string name) => (string.IsNullOrWhiteSpace(slug) ? name : slug).Trim().ToLowerInvariant().Replace(' ', '-');
    private static string BuildSearchText(string sku, string name, string? brand, string? categoryPath) => string.Join(" ", new[] { sku, name, brand, categoryPath }.Where(x => !string.IsNullOrWhiteSpace(x)));
}
