namespace Wms.Infrastructure.Options;

public sealed class SmtpOptions
{
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = "no-reply@localhost";
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "VERII B2B";
    public bool UsePickupDirectory { get; set; }
    public string? PickupDirectory { get; set; }
    public string? ClientBaseUrl { get; set; }
    public string? ResetPath { get; set; }
    public int Timeout { get; set; } = 30;
}
