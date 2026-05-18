using Wms.Domain.Entities.Common;

namespace Wms.Domain.Entities.Communications;

public sealed class SmtpSetting : BaseEntity
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string PasswordEncrypted { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "VERII B2B";
    public int Timeout { get; set; } = 30;
}
