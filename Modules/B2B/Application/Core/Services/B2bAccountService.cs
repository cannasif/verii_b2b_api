using Microsoft.EntityFrameworkCore;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class B2bAccountService : IB2bAccountService
{
    private readonly IRepository<B2bCompany> _companies;
    private readonly IRepository<B2bBuyer> _buyers;
    private readonly IRepository<CatalogVisibilityRule> _visibilityRules;
    private readonly IRepository<ShoppingList> _shoppingLists;
    private readonly IRepository<PurchaseApprovalRule> _approvalRules;
    private readonly IUnitOfWork _unitOfWork;

    public B2bAccountService(
        IRepository<B2bCompany> companies,
        IRepository<B2bBuyer> buyers,
        IRepository<CatalogVisibilityRule> visibilityRules,
        IRepository<ShoppingList> shoppingLists,
        IRepository<PurchaseApprovalRule> approvalRules,
        IUnitOfWork unitOfWork)
    {
        _companies = companies;
        _buyers = buyers;
        _visibilityRules = visibilityRules;
        _shoppingLists = shoppingLists;
        _approvalRules = approvalRules;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PagedResponse<B2bCompanyDto>>> GetCompaniesAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _companies.Query().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.CompanyCode.Contains(search) || x.CompanyName.Contains(search));
        }

        return ApiResponse<PagedResponse<B2bCompanyDto>>.SuccessResult(await PageAsync(query.OrderByDescending(x => x.Id), MapCompany, request, cancellationToken), "Companies retrieved successfully");
    }

    public async Task<ApiResponse<B2bCompanyDto>> CreateCompanyAsync(CreateB2bCompanyDto dto, CancellationToken cancellationToken = default)
    {
        var code = Normalize(dto.CompanyCode);
        var exists = await _companies.Query().AnyAsync(x => x.CompanyCode == code && !x.IsDeleted, cancellationToken);
        if (exists) return ApiResponse<B2bCompanyDto>.ErrorResult("Company code already exists", statusCode: 400);
        var company = new B2bCompany
        {
            CompanyCode = code,
            CompanyName = dto.CompanyName.Trim(),
            CustomerId = dto.CustomerId,
            ParentCompanyId = dto.ParentCompanyId,
            CustomerGroupCode = Trim(dto.CustomerGroupCode),
            CreditLimit = dto.CreditLimit,
            CurrencyCode = NormalizeCurrency(dto.CurrencyCode),
            CreatedDate = DateTimeProvider.Now
        };
        await _companies.AddAsync(company, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<B2bCompanyDto>.SuccessResult(MapCompany(company), "Company created successfully");
    }

    public async Task<ApiResponse<PagedResponse<B2bBuyerDto>>> GetBuyersAsync(PagedRequest request, long? companyId = null, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        var query = _buyers.Query().Where(x => !x.IsDeleted);
        if (companyId.HasValue) query = query.Where(x => x.CompanyId == companyId.Value);
        return ApiResponse<PagedResponse<B2bBuyerDto>>.SuccessResult(await PageAsync(query.OrderByDescending(x => x.Id), MapBuyer, request, cancellationToken), "Buyers retrieved successfully");
    }

    public async Task<ApiResponse<B2bBuyerDto>> CreateBuyerAsync(CreateB2bBuyerDto dto, CancellationToken cancellationToken = default)
    {
        var buyer = new B2bBuyer
        {
            CompanyId = dto.CompanyId,
            UserId = dto.UserId,
            Email = dto.Email.Trim().ToLowerInvariant(),
            FullName = dto.FullName.Trim(),
            RoleCode = NormalizeStatus(dto.RoleCode, "Buyer"),
            OrderLimit = dto.OrderLimit,
            RequiresApproval = dto.RequiresApproval,
            CreatedDate = DateTimeProvider.Now
        };
        await _buyers.AddAsync(buyer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<B2bBuyerDto>.SuccessResult(MapBuyer(buyer), "Buyer created successfully");
    }

    public async Task<ApiResponse<PagedResponse<CatalogVisibilityRuleDto>>> GetVisibilityRulesAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        return ApiResponse<PagedResponse<CatalogVisibilityRuleDto>>.SuccessResult(
            await PageAsync(_visibilityRules.Query().Where(x => !x.IsDeleted).OrderByDescending(x => x.Id), MapVisibilityRule, request, cancellationToken),
            "Catalog visibility rules retrieved successfully");
    }

    public async Task<ApiResponse<CatalogVisibilityRuleDto>> CreateVisibilityRuleAsync(CreateCatalogVisibilityRuleDto dto, CancellationToken cancellationToken = default)
    {
        var rule = new CatalogVisibilityRule
        {
            CompanyId = dto.CompanyId,
            CustomerId = dto.CustomerId,
            CustomerGroupCode = Trim(dto.CustomerGroupCode),
            CatalogProductId = dto.CatalogProductId,
            CategoryPath = Trim(dto.CategoryPath),
            RuleType = NormalizeStatus(dto.RuleType, "Include"),
            CreatedDate = DateTimeProvider.Now
        };
        await _visibilityRules.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<CatalogVisibilityRuleDto>.SuccessResult(MapVisibilityRule(rule), "Catalog visibility rule created successfully");
    }

    public async Task<ApiResponse<PagedResponse<ShoppingListDto>>> GetShoppingListsAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        return ApiResponse<PagedResponse<ShoppingListDto>>.SuccessResult(
            await PageAsync(_shoppingLists.Query().Where(x => !x.IsDeleted).OrderByDescending(x => x.Id), MapShoppingList, request, cancellationToken),
            "Shopping lists retrieved successfully");
    }

    public async Task<ApiResponse<ShoppingListDto>> CreateShoppingListAsync(CreateShoppingListDto dto, CancellationToken cancellationToken = default)
    {
        var list = new ShoppingList
        {
            CompanyId = dto.CompanyId,
            BuyerId = dto.BuyerId,
            Name = dto.Name.Trim(),
            IsShared = dto.IsShared,
            ListType = NormalizeStatus(dto.ListType, "ShoppingList"),
            CreatedDate = DateTimeProvider.Now
        };
        await _shoppingLists.AddAsync(list, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<ShoppingListDto>.SuccessResult(MapShoppingList(list), "Shopping list created successfully");
    }

    public async Task<ApiResponse<PagedResponse<PurchaseApprovalRuleDto>>> GetApprovalRulesAsync(PagedRequest request, CancellationToken cancellationToken = default)
    {
        request ??= new PagedRequest();
        return ApiResponse<PagedResponse<PurchaseApprovalRuleDto>>.SuccessResult(
            await PageAsync(_approvalRules.Query().Where(x => !x.IsDeleted).OrderByDescending(x => x.Id), MapApprovalRule, request, cancellationToken),
            "Approval rules retrieved successfully");
    }

    public async Task<ApiResponse<PurchaseApprovalRuleDto>> CreateApprovalRuleAsync(CreatePurchaseApprovalRuleDto dto, CancellationToken cancellationToken = default)
    {
        var rule = new PurchaseApprovalRule
        {
            CompanyId = dto.CompanyId,
            RuleName = dto.RuleName.Trim(),
            MinOrderAmount = dto.MinOrderAmount,
            MaxOrderAmount = dto.MaxOrderAmount,
            CurrencyCode = NormalizeCurrency(dto.CurrencyCode),
            ApproverRoleCode = NormalizeStatus(dto.ApproverRoleCode, "Approver"),
            CreatedDate = DateTimeProvider.Now
        };
        await _approvalRules.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<PurchaseApprovalRuleDto>.SuccessResult(MapApprovalRule(rule), "Approval rule created successfully");
    }

    private static async Task<PagedResponse<TDto>> PageAsync<TEntity, TDto>(IQueryable<TEntity> query, Func<TEntity, TDto> map, PagedRequest request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResponse<TDto>(items.Select(map).ToList(), total, pageNumber, pageSize);
    }

    private static B2bCompanyDto MapCompany(B2bCompany entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CompanyCode = entity.CompanyCode,
        CompanyName = entity.CompanyName,
        CustomerId = entity.CustomerId,
        ParentCompanyId = entity.ParentCompanyId,
        CustomerGroupCode = entity.CustomerGroupCode,
        CreditLimit = entity.CreditLimit,
        CurrencyCode = entity.CurrencyCode,
        Status = entity.Status
    };

    private static B2bBuyerDto MapBuyer(B2bBuyer entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CompanyId = entity.CompanyId,
        UserId = entity.UserId,
        Email = entity.Email,
        FullName = entity.FullName,
        RoleCode = entity.RoleCode,
        OrderLimit = entity.OrderLimit,
        RequiresApproval = entity.RequiresApproval,
        IsActive = entity.IsActive
    };

    private static CatalogVisibilityRuleDto MapVisibilityRule(CatalogVisibilityRule entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CompanyId = entity.CompanyId,
        CustomerId = entity.CustomerId,
        CustomerGroupCode = entity.CustomerGroupCode,
        CatalogProductId = entity.CatalogProductId,
        CategoryPath = entity.CategoryPath,
        RuleType = entity.RuleType,
        IsActive = entity.IsActive
    };

    private static ShoppingListDto MapShoppingList(ShoppingList entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CompanyId = entity.CompanyId,
        BuyerId = entity.BuyerId,
        Name = entity.Name,
        IsShared = entity.IsShared,
        ListType = entity.ListType
    };

    private static PurchaseApprovalRuleDto MapApprovalRule(PurchaseApprovalRule entity) => new()
    {
        Id = entity.Id,
        BranchCode = entity.BranchCode,
        CreatedDate = entity.CreatedDate,
        UpdatedDate = entity.UpdatedDate,
        CompanyId = entity.CompanyId,
        RuleName = entity.RuleName,
        MinOrderAmount = entity.MinOrderAmount,
        MaxOrderAmount = entity.MaxOrderAmount,
        CurrencyCode = entity.CurrencyCode,
        ApproverRoleCode = entity.ApproverRoleCode,
        IsActive = entity.IsActive
    };

    private static string Normalize(string value) => value.Trim().ToUpperInvariant();
    private static string NormalizeCurrency(string value) => string.IsNullOrWhiteSpace(value) ? "TRY" : value.Trim().ToUpperInvariant();
    private static string NormalizeStatus(string? value, string fallback) => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    private static string? Trim(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
