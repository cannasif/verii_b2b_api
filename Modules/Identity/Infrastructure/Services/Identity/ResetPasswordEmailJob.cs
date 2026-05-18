using Wms.Application.Common;
using Wms.Application.Communications.Services;

namespace Wms.Infrastructure.Services.Identity;

public sealed class ResetPasswordEmailJob : IResetPasswordEmailJob
{
    private readonly IMailService _mailService;

    public ResetPasswordEmailJob(IMailService mailService)
    {
        _mailService = mailService;
    }

    public Task SendUserCreatedEmailAsync(string email, string username, string password, string? firstName, string? lastName)
    {
        var displayName = BuildDisplayName(firstName, lastName, username);
        var subject = "User account created";
        var body = $"Hello {displayName},<br/>Your account has been created.<br/>Username: {username}<br/>Temporary Password: {password}";
        return _mailService.SendEmailAsync(email, subject, body, true);
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string fullName, string token)
    {
        var subject = "Password reset request";
        var body = $"Hello {fullName},<br/>Use this reset token to update your password:<br/><b>{token}</b>";
        return _mailService.SendEmailAsync(toEmail, subject, body, true);
    }

    public Task SendPasswordResetCompletedEmailAsync(string toEmail, string displayName)
    {
        var subject = "Password reset completed";
        var body = $"Hello {displayName},<br/>Your password reset operation has been completed successfully.";
        return _mailService.SendEmailAsync(toEmail, subject, body, true);
    }

    public Task SendPasswordChangedEmailAsync(string toEmail, string displayName)
    {
        var subject = "Password changed";
        var body = $"Hello {displayName},<br/>Your password has been changed successfully.";
        return _mailService.SendEmailAsync(toEmail, subject, body, true);
    }

    private static string BuildDisplayName(string? firstName, string? lastName, string fallback)
    {
        var displayName = string.Join(' ', new[] { firstName, lastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        return string.IsNullOrWhiteSpace(displayName) ? fallback : displayName;
    }
}
