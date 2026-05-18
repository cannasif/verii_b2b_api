namespace Wms.WebApi.Options;

public sealed class PragmaticCorsOptions
{
    public List<string> AllowedOrigins { get; set; } = new();
}
