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
    private readonly IRepository<B2bCart> _carts;
    private readonly byte[] _secret;
    private readonly TimeProvider _timeProvider;

    public B2bPortalAccessService(
        IRepository<B2bCompany> companies,
        IRepository<B2bCart> carts,
        IConfiguration configuration,
        TimeProvider timeProvider)
    {
        _companies = companies;
        _carts = carts;
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

        var expiresAt = _timeProvider.GetUtcNow().UtcDateTime.AddHours(8);
        var token = CreateToken(new B2bPortalTokenPayload
        {
            CompanyId = company.Id,
            CompanyCode = company.CompanyCode,
            CustomerId = company.CustomerId.Value,
            CustomerGroupCode = company.CustomerGroupCode,
            ExpiresAtUnix = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
        });

        return ApiResponse<B2bPortalSessionDto>.SuccessResult(new B2bPortalSessionDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            Company = MapCompany(company)
        }, "Portal oturumu oluşturuldu");
    }

    public async Task<ApiResponse<long>> ValidateRequestAsync(HttpRequest request, CancellationToken cancellationToken = default)
    {
        if (request.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            return ApiResponse<long>.SuccessResult(AuthenticatedBackofficeUser, "Backoffice erişimi doğrulandı");
        }

        var payload = ValidateToken(request);
        if (!payload.Success)
        {
            return payload;
        }

        var customerId = payload.Data;
        var exists = await _companies.Query()
            .AnyAsync(x => !x.IsDeleted && x.CustomerId == customerId && x.Status != "Passive", cancellationToken);

        return exists
            ? ApiResponse<long>.SuccessResult(customerId, "Portal erişimi doğrulandı")
            : ApiResponse<long>.ErrorResult("Portal erişimi geçersiz", statusCode: 401);
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

        var customerId = await _carts.Query()
            .Where(x => !x.IsDeleted && x.Id == cartId)
            .Select(x => x.CustomerId)
            .FirstOrDefaultAsync(cancellationToken);

        return customerId > 0 && customerId == validation.Data
            ? validation
            : ApiResponse<long>.ErrorResult("Bu sepet için portal erişiminiz yok", statusCode: 403);
    }

    private ApiResponse<long> ValidateToken(HttpRequest request)
    {
        var token = request.Headers[HeaderName].ToString();
        if (string.IsNullOrWhiteSpace(token))
        {
            return ApiResponse<long>.ErrorResult("Portal erişim anahtarı zorunludur", statusCode: 401);
        }

        var parts = token.Split('.');
        if (parts.Length != 2)
        {
            return ApiResponse<long>.ErrorResult("Portal erişim anahtarı geçersiz", statusCode: 401);
        }

        var expectedSignature = Sign(parts[0]);
        if (!CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(parts[1]), Encoding.UTF8.GetBytes(expectedSignature)))
        {
            return ApiResponse<long>.ErrorResult("Portal erişim anahtarı doğrulanamadı", statusCode: 401);
        }

        B2bPortalTokenPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<B2bPortalTokenPayload>(Encoding.UTF8.GetString(Base64UrlDecode(parts[0])));
        }
        catch
        {
            return ApiResponse<long>.ErrorResult("Portal erişim anahtarı okunamadı", statusCode: 401);
        }

        if (payload is null || payload.CustomerId <= 0 || payload.ExpiresAtUnix <= _timeProvider.GetUtcNow().ToUnixTimeSeconds())
        {
            return ApiResponse<long>.ErrorResult("Portal oturum süresi doldu", statusCode: 401);
        }

        return ApiResponse<long>.SuccessResult(payload.CustomerId, "Portal erişimi doğrulandı");
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

    private sealed class B2bPortalTokenPayload
    {
        public long CompanyId { get; set; }
        public long CustomerId { get; set; }
        public string CompanyCode { get; set; } = string.Empty;
        public string? CustomerGroupCode { get; set; }
        public long ExpiresAtUnix { get; set; }
    }
}
