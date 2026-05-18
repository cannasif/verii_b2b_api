namespace Wms.Application.Common;

public interface ILocalizationService
{
    string GetLocalizedString(string key);
    string GetLocalizedString(string key, params object[] args);
}
