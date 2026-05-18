namespace Wms.Application.Common;

public interface ILocalizationResource
{
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MessagesByCulture { get; }
}
