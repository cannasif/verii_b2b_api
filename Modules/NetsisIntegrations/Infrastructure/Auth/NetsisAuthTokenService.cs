using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Wms.Application.Common;
using Wms.Modules.NetsisIntegrations.Application.Dtos;
using Wms.Modules.NetsisIntegrations.Application.Services;
using Wms.Modules.NetsisIntegrations.Infrastructure.Options;

namespace Wms.Modules.NetsisIntegrations.Infrastructure.Auth;

public sealed class NetsisAuthTokenService : INetsisAuthTokenService
{
    private const string CacheKeyPrefix = "wms:netsis:token";
    private static readonly SemaphoreSlim TokenSemaphore = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly IOptions<NetsisOptions> _options;
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IRequestTraceAccessor _requestTraceAccessor;
    private readonly ILogger<NetsisAuthTokenService> _logger;

    public NetsisAuthTokenService(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        IOptions<NetsisOptions> options,
        ICurrentUserAccessor currentUserAccessor,
        IRequestTraceAccessor requestTraceAccessor,
        ILogger<NetsisAuthTokenService> logger)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _options = options;
        _currentUserAccessor = currentUserAccessor;
        _requestTraceAccessor = requestTraceAccessor;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        var token = await GetTokenAsync(false, cancellationToken);
        return token.AccessToken;
    }

    public async Task<NetsisTokenResultDto> GetTokenAsync(bool forceRefresh = false, CancellationToken cancellationToken = default)
    {
        var options = _options.Value;
        ValidateOptions(options);

        var branchCode = ResolveBranchCode(options);
        var cacheKey = $"{CacheKeyPrefix}:{branchCode ?? "default"}";

        if (!forceRefresh && TryGetCachedToken(cacheKey, options, out var cachedToken))
        {
            return cachedToken!;
        }

        await TokenSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (!forceRefresh && TryGetCachedToken(cacheKey, options, out cachedToken))
            {
                return cachedToken!;
            }

            var freshToken = await RequestPasswordTokenAsync(options, branchCode, cancellationToken);
            CacheToken(cacheKey, freshToken);
            return freshToken;
        }
        finally
        {
            TokenSemaphore.Release();
        }
    }

    private bool TryGetCachedToken(string cacheKey, NetsisOptions options, out NetsisTokenResultDto? token)
    {
        token = null;
        var cached = _memoryCache.Get<NetsisTokenCacheEntry>(cacheKey);
        if (cached == null || string.IsNullOrWhiteSpace(cached.AccessToken))
        {
            return false;
        }

        if (cached.AccessTokenExpiresAtUtc <= DateTime.UtcNow.AddSeconds(options.Rest.TokenExpirySkewSeconds))
        {
            return false;
        }

        token = cached.ToResult("memory");
        return true;
    }

    private async Task<NetsisTokenResultDto> RequestPasswordTokenAsync(
        NetsisOptions options,
        string? branchCode,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, ResolveLoginPath(options))
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["branchcode"] = branchCode?.Trim() ?? string.Empty,
                ["username"] = options.Rest.Username,
                ["password"] = options.Rest.Password,
                ["dbname"] = options.Rest.DbName,
                ["dbuser"] = options.Rest.DbUser,
                ["dbpassword"] = options.Rest.DbPassword,
                ["dbtype"] = options.Rest.DbType,
            })
        };
        request.Headers.Accept.ParseAdd("application/json");

        _logger.LogInformation(
            "Netsis token request started TraceId={TraceId} BranchCode={BranchCode}",
            _requestTraceAccessor.TraceId,
            branchCode);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Netsis token request failed ({(int)response.StatusCode}). {Truncate(responseBody, 500)}");
        }

        var tokenResponse = JsonSerializer.Deserialize<NetsisTokenResponse>(responseBody, JsonOptions);
        if (tokenResponse == null || string.IsNullOrWhiteSpace(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("Netsis token response is invalid or access_token is missing.");
        }

        var now = DateTime.UtcNow;
        var expiresInSeconds = tokenResponse.ExpiresIn > 0
            ? tokenResponse.ExpiresIn
            : options.Rest.DefaultTokenLifetimeMinutes * 60;

        return new NetsisTokenResultDto
        {
            AccessToken = tokenResponse.AccessToken,
            RefreshToken = tokenResponse.RefreshToken,
            TokenType = string.IsNullOrWhiteSpace(tokenResponse.TokenType) ? "Bearer" : tokenResponse.TokenType,
            ExpiresInSeconds = expiresInSeconds,
            AccessTokenExpiresAtUtc = now.AddSeconds(expiresInSeconds),
            RefreshTokenExpiresAtUtc = tokenResponse.RefreshExpiresIn > 0 ? now.AddSeconds(tokenResponse.RefreshExpiresIn) : null,
            BranchCode = branchCode,
            Source = "password",
        };
    }

    private void CacheToken(string cacheKey, NetsisTokenResultDto token)
    {
        _memoryCache.Set(cacheKey, new NetsisTokenCacheEntry
        {
            AccessToken = token.AccessToken,
            RefreshToken = token.RefreshToken,
            TokenType = token.TokenType,
            AccessTokenExpiresAtUtc = token.AccessTokenExpiresAtUtc,
            RefreshTokenExpiresAtUtc = token.RefreshTokenExpiresAtUtc,
            BranchCode = token.BranchCode,
        }, new MemoryCacheEntryOptions { AbsoluteExpiration = token.AccessTokenExpiresAtUtc });
    }

    private string? ResolveBranchCode(NetsisOptions options)
    {
        return !string.IsNullOrWhiteSpace(_currentUserAccessor.BranchCode)
            ? _currentUserAccessor.BranchCode.Trim()
            : string.IsNullOrWhiteSpace(options.Rest.BranchCode)
                ? null
                : options.Rest.BranchCode.Trim();
    }

    private static string ResolveLoginPath(NetsisOptions options)
        => string.IsNullOrWhiteSpace(options.Rest.LoginPath) ? "/api/v2/token" : options.Rest.LoginPath;

    private static void ValidateOptions(NetsisOptions options)
    {
        if (!options.Enabled)
        {
            throw new InvalidOperationException("Netsis REST integration is disabled.");
        }

        if (string.IsNullOrWhiteSpace(options.Rest.BaseUrl)
            || string.IsNullOrWhiteSpace(options.Rest.Username)
            || string.IsNullOrWhiteSpace(options.Rest.Password)
            || string.IsNullOrWhiteSpace(options.Rest.DbName)
            || string.IsNullOrWhiteSpace(options.Rest.DbUser))
        {
            throw new InvalidOperationException("Netsis REST options are incomplete.");
        }
    }

    private static string? Truncate(string? value, int maxLength)
        => string.IsNullOrWhiteSpace(value) || value.Length <= maxLength ? value : value[..maxLength];

    private sealed class NetsisTokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public string? TokenType { get; set; }
        public int ExpiresIn { get; set; }
        public int RefreshExpiresIn { get; set; }
    }

    private sealed class NetsisTokenCacheEntry
    {
        public string AccessToken { get; init; } = string.Empty;
        public string? RefreshToken { get; init; }
        public string TokenType { get; init; } = "Bearer";
        public DateTime AccessTokenExpiresAtUtc { get; init; }
        public DateTime? RefreshTokenExpiresAtUtc { get; init; }
        public string? BranchCode { get; init; }

        public NetsisTokenResultDto ToResult(string source) => new()
        {
            AccessToken = AccessToken,
            RefreshToken = RefreshToken,
            TokenType = TokenType,
            ExpiresInSeconds = Math.Max(0, (int)(AccessTokenExpiresAtUtc - DateTime.UtcNow).TotalSeconds),
            AccessTokenExpiresAtUtc = AccessTokenExpiresAtUtc,
            RefreshTokenExpiresAtUtc = RefreshTokenExpiresAtUtc,
            BranchCode = BranchCode,
            Source = source,
        };
    }
}
