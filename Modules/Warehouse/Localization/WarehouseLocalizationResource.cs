using Wms.Application.Common;

namespace Wms.Modules.Warehouse.Localization;

public sealed class WarehouseLocalizationResource : ILocalizationResource
{
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MessagesByCulture { get; } = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["WarehouseRetrievedSuccessfully"] = "Warehouse records retrieved successfully",
            ["WarehouseCreatedSuccessfully"] = "Warehouse created successfully",
            ["WarehouseUpdatedSuccessfully"] = "Warehouse updated successfully",
            ["WarehouseDeletedSuccessfully"] = "Warehouse deleted successfully",
            ["WarehouseNotFound"] = "Warehouse not found",
            ["WarehouseAlreadyExists"] = "Warehouse code already exists",
            ["WarehouseSyncCompletedSuccessfully"] = "Warehouse sync completed successfully"
        },
        ["tr"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["WarehouseRetrievedSuccessfully"] = "Depo kayıtları başarıyla getirildi",
            ["WarehouseCreatedSuccessfully"] = "Depo başarıyla oluşturuldu",
            ["WarehouseUpdatedSuccessfully"] = "Depo başarıyla güncellendi",
            ["WarehouseDeletedSuccessfully"] = "Depo başarıyla silindi",
            ["WarehouseNotFound"] = "Depo bulunamadı",
            ["WarehouseAlreadyExists"] = "Depo kodu zaten mevcut",
            ["WarehouseSyncCompletedSuccessfully"] = "Depo senkronizasyonu başarıyla tamamlandı"
        }
    };
}
