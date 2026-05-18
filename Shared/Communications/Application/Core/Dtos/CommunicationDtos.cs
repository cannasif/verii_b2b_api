using System.ComponentModel.DataAnnotations;
using Wms.Application.Common;
using Wms.Domain.Entities.Communications.Enums;

namespace Wms.Application.Communications.Dtos;

public sealed class SmtpSettingsDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public int Timeout { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class UpdateSmtpSettingsDto
{
    [Required]
    [MaxLength(200)]
    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    [MaxLength(200)]
    public string Username { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Password { get; set; }

    [Required]
    [MaxLength(200)]
    public string FromEmail { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string FromName { get; set; } = string.Empty;

    [Range(1, 300)]
    public int Timeout { get; set; } = 30;
}

public sealed class SmtpSettingsRuntimeDto
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public int Timeout { get; set; }
}

public sealed class SendTestMailDto
{
    public string? To { get; set; }
}

public sealed class NotificationDto : BaseEntityDto
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? TitleKey { get; set; }
    public string? MessageKey { get; set; }
    public NotificationChannel Channel { get; set; }
    public NotificationSeverity? Severity { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long? RecipientUserId { get; set; }
    public string? RelatedEntityType { get; set; }
    public long? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public string? TerminalActionCode { get; set; }
}

public sealed class CreateNotificationDto
{
    [Required(ErrorMessage = "Notification_Title_Required")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Notification_Message_Required")]
    [StringLength(2000)]
    public string Message { get; set; } = string.Empty;

    [Required(ErrorMessage = "Notification_Channel_Required")]
    public NotificationChannel Channel { get; set; }

    public NotificationSeverity? Severity { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long? RecipientUserId { get; set; }
    public List<long>? RecipientUserIds { get; set; }
    public string? RelatedEntityType { get; set; }
    public long? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public string? TerminalActionCode { get; set; }
}

public sealed class UpdateNotificationDto
{
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Message { get; set; }

    public NotificationChannel? Channel { get; set; }
    public NotificationSeverity? Severity { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? ReadDate { get; set; }
    public DateTime? ScheduledAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public long? RecipientUserId { get; set; }
    public string? RelatedEntityType { get; set; }
    public long? RelatedEntityId { get; set; }
    public string? ActionUrl { get; set; }
    public string? TerminalActionCode { get; set; }
}
