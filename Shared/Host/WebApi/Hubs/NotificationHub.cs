using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Wms.Infrastructure.Persistence.Context;

namespace Wms.WebApi.Hubs;

[Authorize]
public sealed class NotificationHub : Hub
{
    private readonly WmsDbContext _dbContext;

    public NotificationHub(WmsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override async Task OnConnectedAsync()
    {
        var userIdRaw = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdRaw, out var userId))
        {
            Context.Abort();
            return;
        }

        var token = ResolveAccessToken();
        if (string.IsNullOrWhiteSpace(token))
        {
            Context.Abort();
            return;
        }

        var tokenHash = ComputeSha256Hash(token);
        var hasActiveSession = await _dbContext.UserSessions
            .AsNoTracking()
            .AnyAsync(
                session => session.UserId == userId
                    && session.Token == tokenHash
                    && session.RevokedAt == null,
                Context.ConnectionAborted);

        if (!hasActiveSession)
        {
            Context.Abort();
            return;
        }

        await base.OnConnectedAsync();
    }

    public static Task SendNotificationToUser(
        IHubContext<NotificationHub> hubContext,
        string userId,
        object payload,
        CancellationToken cancellationToken = default)
    {
        return hubContext.Clients.User(userId).SendAsync("ReceiveNotification", payload, cancellationToken);
    }

    public static Task SendNotificationToAll(
        IHubContext<NotificationHub> hubContext,
        object payload,
        CancellationToken cancellationToken = default)
    {
        return hubContext.Clients.All.SendAsync("ReceiveNotification", payload, cancellationToken);
    }

    private string? ResolveAccessToken()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null)
        {
            return null;
        }

        var queryToken = httpContext.Request.Query["access_token"].ToString();
        if (!string.IsNullOrWhiteSpace(queryToken))
        {
            return queryToken;
        }

        var authorizationHeader = httpContext.Request.Headers.Authorization.ToString();
        const string bearerPrefix = "Bearer ";
        if (authorizationHeader.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return authorizationHeader[bearerPrefix.Length..].Trim();
        }

        return null;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using var sha256Hash = SHA256.Create();
        var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
        var builder = new StringBuilder();

        foreach (var value in bytes)
        {
            builder.Append(value.ToString("x2"));
        }

        return builder.ToString();
    }
}
