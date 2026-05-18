namespace Wms.Domain.Entities.AccessControl;

public static class PermissionPlatforms
{
    public const string Web = "web";
    public const string Mobile = "mobile";

    public static bool IsSupported(string? platform)
        => string.Equals(platform, Web, StringComparison.OrdinalIgnoreCase)
           || string.Equals(platform, Mobile, StringComparison.OrdinalIgnoreCase);
}
