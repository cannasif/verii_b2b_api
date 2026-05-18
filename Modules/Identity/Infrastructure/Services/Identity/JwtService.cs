using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Wms.Application.Common;
using Wms.Application.Identity.Services;
using Wms.Domain.Entities.Identity;

namespace Wms.Infrastructure.Services.Identity;

public sealed class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILocalizationService _localizationService;

    public JwtService(IConfiguration configuration, ILocalizationService localizationService)
    {
        _configuration = configuration;
        _localizationService = localizationService;
    }

    public ApiResponse<string> GenerateToken(User user, IReadOnlyCollection<string>? permissions = null, bool isSystemAdmin = false)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new("firstName", user.FirstName ?? string.Empty),
            new("lastName", user.LastName ?? string.Empty),
            new(ClaimTypes.Role, user.RoleNavigation?.Title ?? "User")
        };

        if (isSystemAdmin)
        {
            claims.Add(new Claim("system_admin", "true"));
        }

        if (permissions != null)
        {
            claims.AddRange(
                permissions
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Select(x => new Claim("permission", x)));
        }

        var secretKey = _configuration["Jwt:SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
        var issuer = _configuration["Jwt:Issuer"] ?? "WMS_API";
        var audience = _configuration["Jwt:Audience"] ?? "WMS_Client";
        var accessMinutes = Convert.ToDouble(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(accessMinutes),
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return ApiResponse<string>.SuccessResult(tokenString, _localizationService.GetLocalizedString("TokenGeneratedSuccessfully"));
    }
}
