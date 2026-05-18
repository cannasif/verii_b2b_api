using Wms.Application.Common;

namespace Wms.Modules.YapKod.Localization;

public sealed class YapKodLocalizationResource : ILocalizationResource
{
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MessagesByCulture { get; } = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["YapKodRetrievedSuccessfully"] = "YapKod records retrieved successfully",
            ["YapKodCreatedSuccessfully"] = "YapKod created successfully",
            ["YapKodUpdatedSuccessfully"] = "YapKod updated successfully",
            ["YapKodDeletedSuccessfully"] = "YapKod deleted successfully",
            ["YapKodNotFound"] = "YapKod not found",
            ["YapKodAlreadyExists"] = "YapKod already exists",
            ["YapKodSyncCompletedSuccessfully"] = "YapKod sync completed successfully",
            ["YapKodStockNotFound"] = "Selected stock was not found"
        },
        ["tr"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["YapKodRetrievedSuccessfully"] = "YapKod kayıtları başarıyla getirildi",
            ["YapKodCreatedSuccessfully"] = "YapKod başarıyla oluşturuldu",
            ["YapKodUpdatedSuccessfully"] = "YapKod başarıyla güncellendi",
            ["YapKodDeletedSuccessfully"] = "YapKod başarıyla silindi",
            ["YapKodNotFound"] = "YapKod bulunamadı",
            ["YapKodAlreadyExists"] = "YapKod zaten mevcut",
            ["YapKodSyncCompletedSuccessfully"] = "YapKod senkronizasyonu başarıyla tamamlandı",
            ["YapKodStockNotFound"] = "Seçili stok bulunamadı"
        }
    };
}
