namespace Wms.Application.Common;

/// <summary>
/// Mesaj üretimini application katmanında soyutlar; `_old` localization davranışını korumak için kullanılır.
/// </summary>
public interface ILocalizationService
{
    string GetLocalizedString(string key);
    string GetLocalizedString(string key, params object[] args);
}
