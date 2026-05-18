namespace Wms.Application.Common;

public interface IResetPasswordEmailJob
{
    Task SendUserCreatedEmailAsync(string email, string username, string password, string? firstName, string? lastName);
    Task SendPasswordResetEmailAsync(string toEmail, string fullName, string token);
    Task SendPasswordResetCompletedEmailAsync(string toEmail, string displayName);
    Task SendPasswordChangedEmailAsync(string toEmail, string displayName);
}
