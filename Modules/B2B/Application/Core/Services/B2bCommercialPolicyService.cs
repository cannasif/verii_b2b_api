using Microsoft.EntityFrameworkCore;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class B2bCommercialPolicyService : IB2bCommercialPolicyService
{
    private readonly IRepository<CustomerPriceList> _priceLists;
    private readonly IRepository<CustomerPriceListItem> _priceListItems;
    private readonly IRepository<InventorySnapshot> _inventory;
    private readonly IRepository<QuoteRequest> _quotes;
    private readonly IRepository<QuoteRequestLine> _quoteLines;
    private readonly IRepository<B2bIntegrationEvent> _integrationEvents;
    private readonly IRepository<B2bCart> _carts;
    private readonly IRepository<B2bCartLine> _cartLines;
    private readonly IB2bPricingAvailabilityResolver _pricingAvailabilityResolver;
    private readonly IUnitOfWork _unitOfWork;

    public B2bCommercialPolicyService(
        IRepository<CustomerPriceList> priceLists,
        IRepository<CustomerPriceListItem> priceListItems,
        IRepository<InventorySnapshot> inventory,
        IRepository<QuoteRequest> quotes,
        IRepository<QuoteRequestLine> quoteLines,
        IRepository<B2bIntegrationEvent> integrationEvents,
        IRepository<B2bCart> carts,
        IRepository<B2bCartLine> cartLines,
        IB2bPricingAvailabilityResolver pricingAvailabilityResolver,
        IUnitOfWork unitOfWork)
    {
        _priceLists = priceLists;
        _priceListItems = priceListItems;
        _inventory = inventory;
        _quotes = quotes;
        _quoteLines = quoteLines;
        _integrationEvents = integrationEvents;
        _carts = carts;
        _cartLines = cartLines;
        _pricingAvailabilityResolver = pricingAvailabilityResolver;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResponse<CustomerPriceListDto>>> GetPriceListsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _priceLists.Query()
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Code.Contains(search) || x.Name.Contains(search) || (x.CustomerGroupCode != null && x.CustomerGroupCode.Contains(search)));
        }

        var page = await PageAsync(query.OrderByDescending(x => x.Id), MapPriceList, request, cancellationToken);
        return ApiResponse<PagedResponse<CustomerPriceListDto>>.SuccessResult(page, "Price lists retrieved successfully");
    }

    public async Task<ApiResponse<CustomerPriceListDto>> CreatePriceListAsync(CreateCustomerPriceListDto dto, CancellationToken cancellationToken = default)
    {
        var code = Normalize(dto.Code);
        var exists = await _priceLists.Query().AnyAsync(x => x.Code == code && !x.IsDeleted, cancellationToken);
        if (exists)
        {
            return ApiResponse<CustomerPriceListDto>.ErrorResult("Price list code already exists", statusCode: 400);
        }

        var entity = new CustomerPriceList
        {
            Code = code,
            Name = dto.Name.Trim(),
            CustomerId = dto.CustomerId,
            CustomerGroupCode = Trim(dto.CustomerGroupCode),
            CurrencyCode = NormalizeCurrency(dto.CurrencyCode),
            ValidFrom = dto.ValidFrom,
            ValidTo = dto.ValidTo,
            IsActive = dto.IsActive,
            CreatedDate = DateTimeProvider.Now
        };
        await _priceLists.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerPriceListDto>.SuccessResult(MapPriceList(entity), "Price list created successfully");
    }

    public async Task<ApiResponse<CustomerPriceListItemDto>> UpsertPriceListItemAsync(long priceListId, UpsertCustomerPriceListItemDto dto, CancellationToken cancellationToken = default)
    {
        var listExists = await _priceLists.Query().AnyAsync(x => x.Id == priceListId && !x.IsDeleted, cancellationToken);
        if (!listExists)
        {
            return ApiResponse<CustomerPriceListItemDto>.ErrorResult("Price list not found", statusCode: 404);
        }

        var entity = dto.Id.HasValue
            ? await _priceListItems.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == dto.Id.Value && x.PriceListId == priceListId && !x.IsDeleted, cancellationToken)
            : null;
        if (entity == null)
        {
            entity = new CustomerPriceListItem { PriceListId = priceListId, CreatedDate = DateTimeProvider.Now };
            await _priceListItems.AddAsync(entity, cancellationToken);
        }

        entity.CustomerId = dto.CustomerId;
        entity.CatalogProductId = dto.CatalogProductId;
        entity.CatalogVariantId = dto.CatalogVariantId;
        entity.ErpStockId = dto.ErpStockId;
        entity.UnitPrice = dto.UnitPrice;
        entity.MinQuantity = dto.MinQuantity;
        entity.DiscountRate = dto.DiscountRate;
        entity.CurrencyCode = NormalizeCurrency(dto.CurrencyCode);
        entity.ValidFrom = dto.ValidFrom;
        entity.ValidTo = dto.ValidTo;
        entity.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CustomerPriceListItemDto>.SuccessResult(MapPriceListItem(entity), "Price list item saved successfully");
    }

    public Task<ApiResponse<B2bPriceAvailabilityDto>> ResolvePriceAvailabilityAsync(ResolveB2bPriceAvailabilityDto dto, CancellationToken cancellationToken = default)
    {
        return _pricingAvailabilityResolver.ResolveAsync(dto, cancellationToken);
    }

    public async Task<ApiResponse<PagedResponse<InventorySnapshotDto>>> GetInventoryAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _inventory.Query().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => (x.ErpStockCode != null && x.ErpStockCode.Contains(search)) || (x.WarehouseName != null && x.WarehouseName.Contains(search)));
        }

        var page = await PageAsync(query.OrderByDescending(x => x.SnapshotDate), MapInventory, request, cancellationToken);
        return ApiResponse<PagedResponse<InventorySnapshotDto>>.SuccessResult(page, "Inventory snapshots retrieved successfully");
    }

    public async Task<ApiResponse<InventorySnapshotDto>> UpsertInventoryAsync(UpsertInventorySnapshotDto dto, CancellationToken cancellationToken = default)
    {
        var entity = dto.Id.HasValue
            ? await _inventory.Query(tracking: true).FirstOrDefaultAsync(x => x.Id == dto.Id.Value && !x.IsDeleted, cancellationToken)
            : null;
        if (entity == null)
        {
            entity = new InventorySnapshot { CreatedDate = DateTimeProvider.Now };
            await _inventory.AddAsync(entity, cancellationToken);
        }

        entity.CatalogProductId = dto.CatalogProductId;
        entity.CatalogVariantId = dto.CatalogVariantId;
        entity.ErpStockId = dto.ErpStockId;
        entity.ErpStockCode = Trim(dto.ErpStockCode);
        entity.WarehouseCode = dto.WarehouseCode;
        entity.WarehouseName = Trim(dto.WarehouseName);
        entity.AvailableQuantity = dto.AvailableQuantity;
        entity.ReservedQuantity = dto.ReservedQuantity;
        entity.Unit = Trim(dto.Unit);
        entity.SnapshotDate = DateTimeProvider.Now;
        entity.LastErpSyncDate = dto.LastErpSyncDate;
        entity.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<InventorySnapshotDto>.SuccessResult(MapInventory(entity), "Inventory snapshot saved successfully");
    }

    public async Task<ApiResponse<PagedResponse<QuoteRequestDto>>> GetQuotesAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _quotes.Query()
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.QuoteNumber.Contains(search) || (x.CustomerNote != null && x.CustomerNote.Contains(search)));
        }

        var page = await PageAsync(query.OrderByDescending(x => x.Id), MapQuote, request, cancellationToken);
        return ApiResponse<PagedResponse<QuoteRequestDto>>.SuccessResult(page, "Quote requests retrieved successfully");
    }

    public async Task<ApiResponse<QuoteRequestDto>> CreateQuoteAsync(CreateQuoteRequestDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Lines.Count == 0)
        {
            return ApiResponse<QuoteRequestDto>.ErrorResult("Quote must include at least one line", statusCode: 400);
        }

        var calculatedLines = dto.Lines.Select(CalculateQuoteLine).ToList();
        var total = calculatedLines.Sum(x => x.lineTotal);
        var grandTotal = calculatedLines.Sum(x => x.lineGrandTotal);
        if (dto.GeneralDiscountRate is > 0)
        {
            var discount = Math.Round(grandTotal * dto.GeneralDiscountRate.Value / 100m, 4);
            grandTotal -= discount;
        }
        if (dto.GeneralDiscountAmount is > 0)
        {
            grandTotal -= dto.GeneralDiscountAmount.Value;
        }
        grandTotal = Math.Max(0, grandTotal);

        var quote = new QuoteRequest
        {
            QuoteNumber = string.IsNullOrWhiteSpace(dto.OfferNo) ? $"Q-{DateTimeProvider.Now:yyyyMMddHHmmssfff}" : dto.OfferNo.Trim(),
            CustomerId = dto.CustomerId,
            UserId = dto.UserId,
            Status = B2bWorkflowStatuses.Submitted,
            CurrencyCode = NormalizeCurrency(dto.CurrencyCode),
            OfferType = Trim(dto.OfferType),
            OfferDate = dto.OfferDate ?? DateTimeProvider.Now,
            OfferNo = Trim(dto.OfferNo),
            RevisionNo = Trim(dto.RevisionNo),
            RevisionId = dto.RevisionId,
            ValidUntil = dto.ValidUntil,
            DeliveryDate = dto.DeliveryDate,
            DeliveryMethod = Trim(dto.DeliveryMethod),
            PaymentTypeId = dto.PaymentTypeId,
            ErpProjectCode = Trim(dto.ErpProjectCode),
            GeneralDiscountRate = dto.GeneralDiscountRate,
            GeneralDiscountAmount = dto.GeneralDiscountAmount,
            CustomerNote = dto.CustomerNote,
            SubmittedDate = DateTimeProvider.Now,
            Total = total,
            EstimatedTotal = grandTotal
        };
        await _quotes.AddAsync(quote, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var calculatedLine in calculatedLines)
        {
            var line = calculatedLine.source;
            await _quoteLines.AddAsync(new QuoteRequestLine
            {
                QuoteRequestId = quote.Id,
                CatalogProductId = line.CatalogProductId,
                CatalogVariantId = line.CatalogVariantId,
                ErpStockId = line.ErpStockId,
                RequestedSku = Trim(line.RequestedSku),
                RequestedName = Trim(line.RequestedName),
                Quantity = line.Quantity,
                TargetUnitPrice = line.TargetUnitPrice,
                DiscountRate1 = line.DiscountRate1,
                DiscountAmount1 = line.DiscountAmount1,
                DiscountRate2 = line.DiscountRate2,
                DiscountAmount2 = line.DiscountAmount2,
                DiscountRate3 = line.DiscountRate3,
                DiscountAmount3 = line.DiscountAmount3,
                VatRate = line.VatRate,
                VatAmount = calculatedLine.vatAmount,
                LineTotal = calculatedLine.lineTotal,
                LineGrandTotal = calculatedLine.lineGrandTotal,
                Description = Trim(line.Description),
                Description1 = Trim(line.Description1),
                Description2 = Trim(line.Description2),
                Description3 = Trim(line.Description3),
                PricingRuleHeaderId = line.PricingRuleHeaderId,
                RelatedProductKey = Trim(line.RelatedProductKey),
                IsMainRelatedProduct = line.IsMainRelatedProduct,
                ErpProjectCode = Trim(line.ErpProjectCode) ?? Trim(dto.ErpProjectCode)
            }, cancellationToken);
        }

        await _integrationEvents.AddAsync(new B2bIntegrationEvent
        {
            Direction = "Internal",
            EventType = "QuoteSubmitted",
            EntityName = nameof(QuoteRequest),
            EntityId = quote.Id,
            Status = B2bWorkflowStatuses.Pending
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        var saved = await _quotes.Query().Include(x => x.Lines.Where(l => !l.IsDeleted)).FirstAsync(x => x.Id == quote.Id, cancellationToken);
        return ApiResponse<QuoteRequestDto>.SuccessResult(MapQuote(saved), "Quote request created successfully");
    }

    public async Task<ApiResponse<QuoteRequestDto>> UpdateQuoteStatusAsync(long id, UpdateQuoteStatusDto dto, CancellationToken cancellationToken = default)
    {
        var quote = await _quotes.Query(tracking: true)
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);
        if (quote == null)
        {
            return ApiResponse<QuoteRequestDto>.ErrorResult("Quote request not found", statusCode: 404);
        }

        var nextStatus = B2bWorkflowStatuses.NormalizeRequired(dto.Status);
        if (!B2bWorkflowStatuses.QuoteStatuses.Contains(nextStatus))
        {
            return ApiResponse<QuoteRequestDto>.ErrorResult($"Unsupported quote status: {dto.Status}", statusCode: 400);
        }

        if (string.Equals(quote.Status, B2bWorkflowStatuses.ConvertedToCart, StringComparison.OrdinalIgnoreCase))
        {
            return ApiResponse<QuoteRequestDto>.ErrorResult("Converted quotes cannot be moved to another status", statusCode: 409);
        }

        quote.Status = nextStatus;
        quote.SalesNote = dto.SalesNote ?? quote.SalesNote;
        quote.ApprovedDate = string.Equals(quote.Status, B2bWorkflowStatuses.Approved, StringComparison.OrdinalIgnoreCase) ? DateTimeProvider.Now : quote.ApprovedDate;
        quote.SetUpdatedInfo();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<QuoteRequestDto>.SuccessResult(MapQuote(quote), "Quote request updated successfully");
    }

    public async Task<ApiResponse<CartDto>> ConvertQuoteToCartAsync(ConvertQuoteToCartDto dto, CancellationToken cancellationToken = default)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var quote = await _quotes.Query(tracking: true)
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == dto.QuoteId && !x.IsDeleted, cancellationToken);
            if (quote == null)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<CartDto>.ErrorResult("Quote request not found", statusCode: 404);
            }

            if (!string.Equals(quote.Status, B2bWorkflowStatuses.Approved, StringComparison.OrdinalIgnoreCase))
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<CartDto>.ErrorResult("Only approved quotes can be converted to cart", statusCode: 409);
            }

            if (quote.Lines.Count == 0)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return ApiResponse<CartDto>.ErrorResult("Quote has no lines", statusCode: 400);
            }

            var cart = await GetOrCreateDraftCartAsync(quote.CustomerId, dto.UserId ?? quote.UserId, quote.CurrencyCode, cancellationToken);
            foreach (var quoteLine in quote.Lines.Where(x => !x.IsDeleted))
            {
                var resolved = await _pricingAvailabilityResolver.ResolveAsync(new ResolveB2bPriceAvailabilityDto
                {
                    CustomerId = quote.CustomerId,
                    CatalogProductId = quoteLine.CatalogProductId,
                    CatalogVariantId = quoteLine.CatalogVariantId,
                    ErpStockId = quoteLine.ErpStockId,
                    Quantity = quoteLine.Quantity,
                    CurrencyCode = quote.CurrencyCode
                }, cancellationToken);

                if (!resolved.Success || resolved.Data == null)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<CartDto>.ErrorResult($"Quote line #{quoteLine.Id} could not be validated: {resolved.Message}", resolved.ExceptionMessage, resolved.StatusCode);
                }

                if (!dto.AllowBackorder && !resolved.Data.IsAvailable)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<CartDto>.ErrorResult($"Quote line #{quoteLine.Id} is not available", statusCode: 409);
                }

                var unitPrice = quoteLine.ApprovedUnitPrice ?? resolved.Data.UnitPrice;
                if (!unitPrice.HasValue)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<CartDto>.ErrorResult($"Quote line #{quoteLine.Id} has no approved or effective price", statusCode: 400);
                }

                var cartLine = new B2bCartLine
                {
                    CartId = cart.Id,
                    CatalogProductId = quoteLine.CatalogProductId,
                    CatalogVariantId = quoteLine.CatalogVariantId,
                    ErpStockId = quoteLine.ErpStockId,
                    WarehouseCode = resolved.Data.PreferredWarehouseCode,
                    Quantity = quoteLine.Quantity,
                    UnitPrice = unitPrice.Value,
                    CurrencyCode = quote.CurrencyCode,
                    CreatedDate = DateTimeProvider.Now
                };
                await _cartLines.AddAsync(cartLine, cancellationToken);
                var reserveResult = await ReserveInventoryAsync(cartLine, dto.AllowBackorder, cancellationToken);
                if (!reserveResult.Success)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return ApiResponse<CartDto>.ErrorResult(reserveResult.Message, statusCode: reserveResult.StatusCode);
                }
            }

            quote.Status = B2bWorkflowStatuses.ConvertedToCart;
            quote.SetUpdatedInfo();
            await _integrationEvents.AddAsync(new B2bIntegrationEvent
            {
                Direction = "Internal",
                EventType = "QuoteConvertedToCart",
                EntityName = nameof(QuoteRequest),
                EntityId = quote.Id,
                Status = B2bWorkflowStatuses.Completed,
                ProcessedDate = DateTimeProvider.Now
            }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            var savedCart = await _carts.Query()
                .Include(x => x.Lines.Where(l => !l.IsDeleted))
                .FirstAsync(x => x.Id == cart.Id, cancellationToken);
            return ApiResponse<CartDto>.SuccessResult(MapCart(savedCart), "Approved quote converted to cart successfully");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task<ApiResponse<PagedResponse<B2bIntegrationEventDto>>> GetIntegrationEventsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _integrationEvents.Query().Where(x => !x.IsDeleted);
        var page = await PageAsync(query.OrderByDescending(x => x.Id), MapIntegrationEvent, request, cancellationToken);
        return ApiResponse<PagedResponse<B2bIntegrationEventDto>>.SuccessResult(page, "Integration events retrieved successfully");
    }

    private static async Task<PagedResponse<TDto>> PageAsync<TEntity, TDto>(
        IQueryable<TEntity> query,
        Func<TEntity, TDto> map,
        PagedRequest request,
        CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResponse<TDto>(items.Select(map).ToList(), total, pageNumber, pageSize);
    }

    private async Task<B2bCart> GetOrCreateDraftCartAsync(long customerId, long? userId, string currencyCode, CancellationToken cancellationToken)
    {
        var cart = await _carts.Query(tracking: true)
            .Include(x => x.Lines.Where(l => !l.IsDeleted))
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.CustomerId == customerId && x.UserId == userId && x.Status == B2bWorkflowStatuses.Draft, cancellationToken);
        if (cart != null)
        {
            return cart;
        }

        cart = new B2bCart
        {
            CustomerId = customerId,
            UserId = userId,
            Status = B2bWorkflowStatuses.Draft,
            CurrencyCode = NormalizeCurrency(currencyCode),
            CreatedDate = DateTimeProvider.Now
        };
        await _carts.AddAsync(cart, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return cart;
    }

    private async Task<ApiResponse<bool>> ReserveInventoryAsync(B2bCartLine line, bool allowBackorder, CancellationToken cancellationToken)
    {
        if (line.Quantity <= 0 || line.WarehouseCode == null)
        {
            return ApiResponse<bool>.SuccessResult(true, "No reservation required");
        }

        var query = _inventory.Query(tracking: true).Where(x => !x.IsDeleted && x.WarehouseCode == line.WarehouseCode);
        query = query.Where(x =>
            (line.CatalogVariantId.HasValue && x.CatalogVariantId == line.CatalogVariantId.Value) ||
            (line.CatalogProductId.HasValue && x.CatalogProductId == line.CatalogProductId.Value) ||
            (line.ErpStockId.HasValue && x.ErpStockId == line.ErpStockId.Value));

        var snapshot = await query.OrderByDescending(x => x.SnapshotDate).FirstOrDefaultAsync(cancellationToken);
        if (snapshot == null)
        {
            return allowBackorder
                ? ApiResponse<bool>.SuccessResult(true, "Inventory snapshot not found; backorder allowed")
                : ApiResponse<bool>.ErrorResult("Inventory snapshot not found for reservation", statusCode: 409);
        }

        var availableToSell = Math.Max(0, snapshot.AvailableQuantity - snapshot.ReservedQuantity);
        if (!allowBackorder && availableToSell < line.Quantity)
        {
            return ApiResponse<bool>.ErrorResult("Requested quantity is no longer available for reservation", statusCode: 409);
        }

        snapshot.ReservedQuantity += line.Quantity;
        snapshot.SetUpdatedInfo();
        return ApiResponse<bool>.SuccessResult(true, "Inventory reserved");
    }

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

    private static CustomerPriceListDto MapPriceList(CustomerPriceList entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        Code = entity.Code,
        Name = entity.Name,
        CustomerId = entity.CustomerId,
        CustomerGroupCode = entity.CustomerGroupCode,
        CurrencyCode = entity.CurrencyCode,
        ValidFrom = entity.ValidFrom,
        ValidTo = entity.ValidTo,
        IsActive = entity.IsActive,
        Items = entity.Items.Where(x => !x.IsDeleted).Select(MapPriceListItem).ToList()
    };

    private static CustomerPriceListItemDto MapPriceListItem(CustomerPriceListItem entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        PriceListId = entity.PriceListId,
        CustomerId = entity.CustomerId,
        CatalogProductId = entity.CatalogProductId,
        CatalogVariantId = entity.CatalogVariantId,
        ErpStockId = entity.ErpStockId,
        UnitPrice = entity.UnitPrice,
        MinQuantity = entity.MinQuantity,
        DiscountRate = entity.DiscountRate,
        CurrencyCode = entity.CurrencyCode,
        ValidFrom = entity.ValidFrom,
        ValidTo = entity.ValidTo
    };

    private static InventorySnapshotDto MapInventory(InventorySnapshot entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CatalogProductId = entity.CatalogProductId,
        CatalogVariantId = entity.CatalogVariantId,
        ErpStockId = entity.ErpStockId,
        ErpStockCode = entity.ErpStockCode,
        WarehouseCode = entity.WarehouseCode,
        WarehouseName = entity.WarehouseName,
        AvailableQuantity = entity.AvailableQuantity,
        ReservedQuantity = entity.ReservedQuantity,
        Unit = entity.Unit,
        SnapshotDate = entity.SnapshotDate,
        LastErpSyncDate = entity.LastErpSyncDate
    };

    private static QuoteRequestDto MapQuote(QuoteRequest entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        QuoteNumber = entity.QuoteNumber,
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
        ErpProjectCode = entity.ErpProjectCode,
        GeneralDiscountRate = entity.GeneralDiscountRate,
        GeneralDiscountAmount = entity.GeneralDiscountAmount,
        Total = entity.Total,
        EstimatedTotal = entity.EstimatedTotal,
        CustomerNote = entity.CustomerNote,
        SalesNote = entity.SalesNote,
        SubmittedDate = entity.SubmittedDate,
        ApprovedDate = entity.ApprovedDate,
        Lines = entity.Lines.Where(x => !x.IsDeleted).Select(MapQuoteLine).ToList()
    };

    private static QuoteRequestLineDto MapQuoteLine(QuoteRequestLine entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        QuoteRequestId = entity.QuoteRequestId,
        CatalogProductId = entity.CatalogProductId,
        CatalogVariantId = entity.CatalogVariantId,
        ErpStockId = entity.ErpStockId,
        RequestedSku = entity.RequestedSku,
        RequestedName = entity.RequestedName,
        Quantity = entity.Quantity,
        TargetUnitPrice = entity.TargetUnitPrice,
        ApprovedUnitPrice = entity.ApprovedUnitPrice,
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

    private static (CreateQuoteRequestLineDto source, decimal lineTotal, decimal vatAmount, decimal lineGrandTotal) CalculateQuoteLine(CreateQuoteRequestLineDto line)
    {
        var gross = Math.Max(0, line.Quantity) * Math.Max(0, line.TargetUnitPrice ?? 0);
        var afterDiscount1 = ApplyDiscount(gross, line.DiscountRate1, line.DiscountAmount1);
        var afterDiscount2 = ApplyDiscount(afterDiscount1, line.DiscountRate2, line.DiscountAmount2);
        var lineTotal = ApplyDiscount(afterDiscount2, line.DiscountRate3, line.DiscountAmount3);
        var vatAmount = Math.Round(lineTotal * Math.Max(0, line.VatRate) / 100m, 4);
        return (line, lineTotal, vatAmount, lineTotal + vatAmount);
    }

    private static decimal ApplyDiscount(decimal amount, decimal rate, decimal fixedAmount)
    {
        var discounted = amount;
        if (rate > 0)
        {
            discounted -= Math.Round(discounted * rate / 100m, 4);
        }
        if (fixedAmount > 0)
        {
            discounted -= fixedAmount;
        }
        return Math.Max(0, discounted);
    }

    private static B2bIntegrationEventDto MapIntegrationEvent(B2bIntegrationEvent entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        Direction = entity.Direction,
        EventType = entity.EventType,
        EntityName = entity.EntityName,
        EntityId = entity.EntityId,
        Status = entity.Status,
        ExternalReference = entity.ExternalReference,
        ErrorMessage = entity.ErrorMessage,
        ProcessedDate = entity.ProcessedDate
    };

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string NormalizeCurrency(string value) => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();
    private static string NormalizeStatus(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
