using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Wms.Application.B2B.Dtos;
using Wms.Application.Common;
using Wms.Domain.Entities.B2B;

namespace Wms.Application.B2B.Services;

public sealed class B2bPortalAccessService : IB2bPortalAccessService
{
    private const string HeaderName = "X-B2B-Portal-Token";
    private const long AuthenticatedBackofficeUser = -1;
    private readonly IRepository<B2bCompany> _companies;
    private readonly IRepository<B2bBuyer> _buyers;
    private readonly IRepository<B2bCart> _carts;
    private readonly IRepository<B2bOrder> _orders;
    private readonly byte[] _secret;
    private readonly TimeProvider _timeProvider;

    public B2bPortalAccessService(
        IRepository<B2bCompany> companies,
        IRepository<B2bBuyer> buyers,
        IRepository<B2bCart> carts,
        IRepository<B2bOrder> orders,
        IConfiguration configuration,
        TimeProvider timeProvider)
    {
        _companies = companies;
        _buyers = buyers;
        _carts = carts;
        _orders = orders;
        _timeProvider = timeProvider;
        _secret = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
    }

    public async Task<ApiResponse<B2bPortalSessionDto>> CreateSessionAsync(CreateB2bPortalSessionDto dto, CancellationToken cancellationToken = default)
    {
        var companyCode = Normalize(dto.CompanyCode);
        if (string.IsNullOrWhiteSpace(companyCode))
        {
            return ApiResponse<B2bPortalSessionDto>.ErrorResult("Şirket kodu zorunludur", statusCode: 400);
        }

        var company = await _companies.Query()
            .FirstOrDefaultAsync(x => !x.IsDeleted && x.CompanyCode == companyCode && x.Status != "Passive", cancellationToken);

        if (company is null || !company.CustomerId.HasValue || company.CustomerId.Value <= 0)
        {
            return ApiResponse<B2bPortalSessionDto>.ErrorResult("Portal erişimi için geçerli şirket bulunamadı", statusCode: 404);
        }

        var buyerEmail = NormalizeEmail(dto.BuyerEmail);
        var activeBuyers = _buyers.Query().Where(x => !x.IsDeleted && x.CompanyId == company.Id && x.IsActive);
        B2bBuyer? buyer = null;
        if (!string.IsNullOrWhiteSpace(buyerEmail))
        {
            buyer = await activeBuyers.FirstOrDefaultAsync(x => x.Email.ToUpper() == buyerEmail, cancellationToken);
            if (buyer is null)
            {
                return ApiResponse<B2bPortalSessionDto>.ErrorResult("Bu şirkete bağlı aktif portal kullanıcısı bulunamadı", statusCode: 404);
            }
        }
        else if (await activeBuyers.AnyAsync(cancellationToken))
        {
            return ApiResponse<B2bPortalSessionDto>.ErrorResult("Portal kullanıcısı e-postası zorunludur", statusCode: 400);
        }

        var canViewCompanyHistory = buyer is null || CanViewCompanyHistory(buyer.RoleCode);
        var expiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddHours(8);
        var token = CreateToken(new B2bPortalTokenPayload
        {
            CompanyId = company.Id,
            CompanyCode = company.CompanyCode,
            CustomerId = company.CustomerId.Value,
            CustomerGroupCode = company.CustomerGroupCode,
            BuyerId = buyer?.Id,
            UserId = buyer?.UserId,
            BuyerEmail = buyer?.Email,
            BuyerName = buyer?.FullName,
            RoleCode = buyer?.RoleCode ?? "CompanyAdmin",
            CanViewCompanyHistory = canViewCompanyHistory,
            ExpiresAtUnix = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
        });

        return ApiResponse<B2bPortalSessionDto>.SuccessResult(new B2bPortalSessionDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Company = MapCompany(company),
            Buyer = buyer is null ? null : MapBuyer(buyer),
            Scope = canViewCompanyHistory ? "Company" : "Buyer",
            CanViewCompanyHistory = canViewCompanyHistory
        }, "Portal oturumu oluşturuldu");
    }

    public async Task<ApiResponse<B2bPortalSessionDto>> CreateSessionForUserAsync(long userId, CancellationToken cancellationToken = default)
    {
        var buyer = await _buyers.Query()
            .Include(x => x.Company)
            .Where(x => !x.IsDeleted && x.IsActive && x.UserId == userId)
            .OrderByDescending(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (buyer?.Company is null || buyer.Company.IsDeleted || buyer.Company.Status == "Passive" || !buyer.Company.CustomerId.HasValue || buyer.Company.CustomerId.Value <= 0)
        {
            return ApiResponse<B2bPortalSessionDto>.ErrorResult("Bu kullanıcıya bağlı aktif B2B alıcı profili bulunamadı", statusCode: 404);
        }

        return CreateSessionFromCompanyAndBuyer(buyer.Company, buyer);
    }

    private ApiResponse<B2bPortalSessionDto> CreateSessionFromCompanyAndBuyer(B2bCompany company, B2bBuyer? buyer)
    {
        var canViewCompanyHistory = buyer is null || CanViewCompanyHistory(buyer.RoleCode);
        var expiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddHours(8);
        var token = CreateToken(new B2bPortalTokenPayload
        {
            CompanyId = company.Id,
            CompanyCode = company.CompanyCode,
            CustomerId = company.CustomerId!.Value,
            CustomerGroupCode = company.CustomerGroupCode,
            BuyerId = buyer?.Id,
            UserId = buyer?.UserId,
            BuyerEmail = buyer?.Email,
            BuyerName = buyer?.FullName,
            RoleCode = buyer?.RoleCode ?? "CompanyAdmin",
            CanViewCompanyHistory = canViewCompanyHistory,
            ExpiresAtUnix = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
        });

        return ApiResponse<B2bPortalSessionDto>.SuccessResult(new B2bPortalSessionDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Company = MapCompany(company),
            Buyer = buyer is null ? null : MapBuyer(buyer),
            Scope = canViewCompanyHistory ? "Company" : "Buyer",
            CanViewCompanyHistory = canViewCompanyHistory
        }, "Portal oturumu oluşturuldu");
    }

    public async Task<ApiResponse<long>> ValidateRequestAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (request.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            return ApiResponse<long>.SuccessResult(AuthenticatedBackofficeUser, "Backoffice erişimi doğrulandı");
        }

        var context = await ValidateContextAsync(request, cancellationToken);
        if (!context.Success)
        {
            return ApiResponse<long>.ErrorResult(context.Message, context.ExceptionMessage, context.StatusCode);
        }

        return ApiResponse<long>.SuccessResult(context.Data!.IsBackoffice ? AuthenticatedBackofficeUser : context.Data.CustomerId, "Portal erişimi doğrulandı");
    }

    public async Task<ApiResponse<B2bPortalContextDto>> ValidateContextAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (request.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            return ApiResponse<B2bPortalContextDto>.SuccessResult(new B2bPortalContextDto
            {
                IsBackoffice = true,
                CustomerId = AuthenticatedBackofficeUser,
                CanViewCompanyHistory = true,
                RoleCode = "Backoffice"
            }, "Backoffice erişimi doğrulandı");
        }

        var payloadResult = ValidateToken(request);
        if (!payloadResult.Success || payloadResult.Data is null)
        {
            return ApiResponse<B2bPortalContextDto>.ErrorResult(payloadResult.Message, payloadResult.ExceptionMessage, payloadResult.StatusCode);
        }

        var payload = payloadResult.Data;
        var companyExists = await _companies.Query()
            .AnyAsync(x => !x.IsDeleted && x.Id == payload.CompanyId && x.CustomerId == payload.CustomerId && x.Status != "Passive", cancellationToken);

        if (!companyExists)
        {
            return ApiResponse<B2bPortalContextDto>.ErrorResult("Portal erişimi geçersiz", statusCode: 401);
        }

        if (payload.BuyerId.HasValue)
        {
            var buyerExists = await _buyers.Query()
                .AnyAsync(x => !x.IsDeleted && x.Id == payload.BuyerId.Value && x.CompanyId == payload.CompanyId && x.IsActive, cancellationToken);
            if (!buyerExists)
            {
                return ApiResponse<B2bPortalContextDto>.ErrorResult("Portal kullanıcısı pasif veya geçersiz", statusCode: 401);
            }
        }

        return ApiResponse<B2bPortalContextDto>.SuccessResult(new B2bPortalContextDto
        {
            CompanyId = payload.CompanyId,
            CustomerId = payload.CustomerId,
            CompanyCode = payload.CompanyCode,
            CustomerGroupCode = payload.CustomerGroupCode,
            BuyerId = payload.BuyerId,
            UserId = payload.UserId,
            BuyerEmail = payload.BuyerEmail,
            BuyerName = payload.BuyerName,
            RoleCode = payload.RoleCode,
            CanViewCompanyHistory = payload.CanViewCompanyHistory
        }, "Portal erişimi doğrulandı");
    }

    public async Task<ApiResponse<B2bPortalContextDto>> ValidateCustomerContextAsync(HttpRequest request, long customerId, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateContextAsync(request, cancellationToken);
        if (!validation.Success)
        {
            return validation;
        }

        if (validation.Data!.IsBackoffice || validation.Data.CustomerId == customerId)
        {
            return validation;
        }

        return ApiResponse<B2bPortalContextDto>.ErrorResult("Bu müşteri için portal erişiminiz yok", statusCode: 403);
    }

    public async Task<ApiResponse<long>> ValidateCustomerAccessAsync(HttpRequest request, long customerId, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateRequestAsync(request, cancellationToken);
        if (!validation.Success)
        {
            return validation;
        }

        return validation.Data == AuthenticatedBackofficeUser || validation.Data == customerId
            ? validation
            : ApiResponse<long>.ErrorResult("Bu müşteri için portal erişiminiz yok", statusCode: 403);
    }

    public async Task<ApiResponse<long>> ValidateCartAccessAsync(HttpRequest request, long cartId, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateRequestAsync(request, cancellationToken);
        if (!validation.Success)
        {
            return validation;
        }

        if (validation.Data == AuthenticatedBackofficeUser)
        {
            return validation;
        }

        var cart = await _carts.Query()
            .Where(x => !x.IsDeleted && x.Id == cartId)
            .Select(x => new { x.CustomerId, x.BuyerId, x.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (cart is null || cart.CustomerId != validation.Data)
        {
            return ApiResponse<long>.ErrorResult("Bu sepet için portal erişiminiz yok", statusCode: 403);
        }

        var context = await ValidateContextAsync(request, cancellationToken);
        if (!context.Success || context.Data is null)
        {
            return ApiResponse<long>.ErrorResult(context.Message, context.ExceptionMessage, context.StatusCode);
        }

        if (context.Data.CanViewCompanyHistory || (context.Data.BuyerId.HasValue && cart.BuyerId == context.Data.BuyerId) || (context.Data.UserId.HasValue && cart.UserId == context.Data.UserId))
        {
            return validation;
        }

        return ApiResponse<long>.ErrorResult("Bu sepet için portal erişiminiz yok", statusCode: 403);
    }

    public async Task<ApiResponse<long>> ValidateOrderAccessAsync(HttpRequest request, long orderId, CancellationToken cancellationToken = default)
    {
        var validation = await ValidateRequestAsync(request, cancellationToken);
        if (!validation.Success)
        {
            return validation;
        }

        if (validation.Data == AuthenticatedBackofficeUser)
        {
            return validation;
        }

        var order = await _orders.Query()
            .Where(x => !x.IsDeleted && x.Id == orderId)
            .Select(x => new { x.CustomerId, x.BuyerId, x.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null || order.CustomerId != validation.Data)
        {
            return ApiResponse<long>.ErrorResult("Bu sipariş için portal erişiminiz yok", statusCode: 403);
        }

        var context = await ValidateContextAsync(request, cancellationToken);
        if (!context.Success || context.Data is null)
        {
            return ApiResponse<long>.ErrorResult(context.Message, context.ExceptionMessage, context.StatusCode);
        }

        if (context.Data.CanViewCompanyHistory || (context.Data.BuyerId.HasValue && order.BuyerId == context.Data.BuyerId) || (context.Data.UserId.HasValue && order.UserId == context.Data.UserId))
        {
            return validation;
        }

        return ApiResponse<long>.ErrorResult("Bu sipariş için portal erişiminiz yok", statusCode: 403);
    }

    private ApiResponse<B2bPortalTokenPayload> ValidateToken(HttpRequest request)
    {
        var token = request.Headers[HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(token))
        {
            return ApiResponse<B2bPortalTokenPayload>.ErrorResult("Portal erişim anahtarı zorunludur", statusCode: 401);
        }

        var parts = token.Split('.');
        if (parts.Length != 2)
        {
            return ApiResponse<B2bPortalTokenPayload>.ErrorResult("Portal erişim anahtarı geçersiz", statusCode: 401);
        }

        var expectedSignature = Sign(parts[0]);
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(parts[1]), Encoding.UTF8.GetBytes(expectedSignature)))
        {
            return ApiResponse<B2bPortalTokenPayload>.ErrorResult("Portal erişim anahtarı doğrulanamadı", statusCode: 401);
        }

        B2bPortalTokenPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<B2bPortalTokenPayload>(Encoding.UTF8.GetString(Base64UrlDecode(parts[0])));
        }
        catch
        {
            return ApiResponse<B2bPortalTokenPayload>.ErrorResult("Portal erişim anahtarı okunamadı", statusCode: 401);
        }

        if (payload is null || payload.CustomerId <= 0 || payload.ExpiresAtUnix <= _timeProvider.GetUtcNow().ToUnixTimeSeconds())
        {
            return ApiResponse<B2bPortalTokenPayload>.ErrorResult("Portal oturum süresi doldu", statusCode: 401);
        }

        return ApiResponse<B2bPortalTokenPayload>.SuccessResult(payload, "Portal erişimi doğrulandı");
    }

    private string CreateToken(B2bPortalTokenPayload payload)
    {
        var body = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        return $"{body}.{Sign(body)}";
    }

    private string Sign(string body)
    {
        using var hmac = new HMACSHA256(_secret);
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        return Convert.FromBase64String(base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '='));
    }

    private static string Normalize(string value)
    {
        return value.Trim().ToUpperInvariant();
    }

    private static string? NormalizeEmail(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
    }

    private static bool CanViewCompanyHistory(string? roleCode)
    {
        var role = Normalize(roleCode ?? string.Empty);
        return role is "ADMIN" or "COMPANYADMIN" or "MANAGER" or "SUPERVISOR" or "APPROVER";
    }

    private static B2bCompanyDto MapCompany(B2bCompany company)
    {
        return new B2bCompanyDto
        {
            Id = company.Id,
            CompanyCode = company.CompanyCode,
            CompanyName = company.CompanyName,
            CustomerId = company.CustomerId,
            ParentCompanyId = company.ParentCompanyId,
            CustomerGroupCode = company.CustomerGroupCode,
            CreditLimit = company.CreditLimit,
            CurrencyCode = company.CurrencyCode,
            Status = company.Status,
            CreatedDate = company.CreatedDate,
            UpdatedDate = company.UpdatedDate
        };
    }

    private static B2bBuyerDto MapBuyer(B2bBuyer buyer)
    {
        return new B2bBuyerDto
        {
            Id = buyer.Id,
            CompanyId = buyer.CompanyId,
            UserId = buyer.UserId,
            Email = buyer.Email,
            FullName = buyer.FullName,
            RoleCode = buyer.RoleCode,
            OrderLimit = buyer.OrderLimit,
            RequiresApproval = buyer.RequiresApproval,
            IsActive = buyer.IsActive,
            CreatedDate = buyer.CreatedDate,
            UpdatedDate = buyer.UpdatedDate
        };
    }

    private sealed class B2bPortalTokenPayload
    {
        public long CompanyId { get; set; }
        public long CustomerId { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string? CustomerGroupCode { get; set; }
        public long? BuyerId { get; set; }
        public long? UserId { get; set; }
        public string? BuyerEmail { get; set; }
        public string? BuyerName { get; set; }
        public string RoleCode { get; set; } = "Buyer";
        public bool CanViewCompanyHistory { get; set; }
        public long ExpiresAtUnix { get; set; }
    }
}
