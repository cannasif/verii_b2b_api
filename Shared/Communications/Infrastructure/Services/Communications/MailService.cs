using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Wms.Application.Common;
using Wms.Application.Communications.Dtos;
using Wms.Application.Communications.Services;
using Wms.Domain.Entities.Identity;

namespace Wms.Infrastructure.Services.Communications;

public sealed class MailService : IMailService
{
    private readonly ISmtpSettingsService _smtpSettingsService;
    private readonly IRepository<User> _users;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<MailService> _logger;

    public MailService(
        ISmtpSettingsService smtpSettingsService,
        IRepository<User> users,
        ILocalizationService localizationService,
        ILogger<MailService> logger)
    {
        _smtpSettingsService = smtpSettingsService;
        _users = users;
        _localizationService = localizationService;
        _logger = logger;
    }

    public Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        string? cc = null,
        string? bcc = null,
        List<string>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        return SendEmailAsync(to, subject, body, null, null, isHtml, cc, bcc, attachments, cancellationToken);
    }

    public async Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        string? fromEmail = null,
        string? fromName = null,
        bool isHtml = true,
        string? cc = null,
        string? bcc = null,
        List<string>? attachments = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var smtp = await _smtpSettingsService.GetRuntimeAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(smtp.Host) || string.IsNullOrWhiteSpace(smtp.Username) || string.IsNullOrWhiteSpace(smtp.Password))
        {
            _logger.LogError("SMTP host/username/password is not configured.");
            return false;
        }

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            Credentials = new NetworkCredential(smtp.Username, smtp.Password),
            Timeout = smtp.Timeout * 1000
        };

        using var message = new MailMessage();
        var resolvedFromEmail = !string.IsNullOrWhiteSpace(fromEmail) ? fromEmail : smtp.FromEmail;
        var resolvedFromName = !string.IsNullOrWhiteSpace(fromName) ? fromName : smtp.FromName;

        if (string.IsNullOrWhiteSpace(resolvedFromEmail))
        {
            _logger.LogError("SMTP from email is not configured.");
            return false;
        }

        message.From = new MailAddress(resolvedFromEmail, resolvedFromName);
        message.To.Add(to);

        if (!string.IsNullOrWhiteSpace(cc))
        {
            message.CC.Add(cc);
        }

        if (!string.IsNullOrWhiteSpace(bcc))
        {
            message.Bcc.Add(bcc);
        }

        message.Subject = subject;
        message.Body = body;
        message.IsBodyHtml = isHtml;

        if (attachments is { Count: > 0 })
        {
            foreach (var attachmentPath in attachments)
            {
                if (File.Exists(attachmentPath))
                {
                    message.Attachments.Add(new Attachment(attachmentPath));
                }
                else
                {
                    _logger.LogWarning("Attachment file not found: {AttachmentPath}", attachmentPath);
                }
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        await client.SendMailAsync(message, cancellationToken);
        return true;
    }

    public async Task<ApiResponse<bool>> SendTestAsync(SendTestMailDto dto, long? currentUserId, CancellationToken cancellationToken = default)
    {
        SmtpSettingsRuntimeDto smtp;
        try
        {
            smtp = await _smtpSettingsService.GetRuntimeAsync(cancellationToken);
        }
        catch
        {
            return ApiResponse<bool>.ErrorResult(
                _localizationService.GetLocalizedString("ValidationError"),
                _localizationService.GetLocalizedString("SmtpSettingsMissingOrInvalid"),
                400);
        }

        if (string.IsNullOrWhiteSpace(smtp.Host) || string.IsNullOrWhiteSpace(smtp.Username) || string.IsNullOrWhiteSpace(smtp.Password) || string.IsNullOrWhiteSpace(smtp.FromEmail))
        {
            return ApiResponse<bool>.ErrorResult(
                _localizationService.GetLocalizedString("ValidationError"),
                _localizationService.GetLocalizedString("SmtpSettingsIncomplete"),
                400);
        }

        var to = dto.To;
        if (string.IsNullOrWhiteSpace(to))
        {
            if (!currentUserId.HasValue)
            {
                var unauthorized = _localizationService.GetLocalizedString("Unauthorized");
                return ApiResponse<bool>.ErrorResult(unauthorized, unauthorized, 401);
            }

            var user = await _users.Query()
                .Where(x => x.Id == currentUserId.Value && !x.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null || string.IsNullOrWhiteSpace(user.Email))
            {
                var notFound = _localizationService.GetLocalizedString("UserNotFound");
                return ApiResponse<bool>.ErrorResult(notFound, notFound, 404);
            }

            to = user.Email;
        }

        var ok = await SendEmailAsync(
            to,
            "SMTP Test Mail",
            $"SMTP test email sent at {DateTime.UtcNow:O}",
            false,
            null,
            null,
            null,
            cancellationToken);

        if (!ok)
        {
            return ApiResponse<bool>.ErrorResult(
                _localizationService.GetLocalizedString("ValidationError"),
                _localizationService.GetLocalizedString("FailedToSendTestMail"),
                400);
        }

        return ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("OperationSuccessful"));
    }
}
