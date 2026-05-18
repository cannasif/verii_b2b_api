using Wms.Application.Common;

namespace Wms.Modules.Customer.Localization;

public sealed class CustomerLocalizationResource : ILocalizationResource
{
    public IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> MessagesByCulture { get; } = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CustomerRetrievedSuccessfully"] = "Customer records retrieved successfully",
            ["CustomerCreatedSuccessfully"] = "Customer created successfully",
            ["CustomerUpdatedSuccessfully"] = "Customer updated successfully",
            ["CustomerDeletedSuccessfully"] = "Customer deleted successfully",
            ["CustomerNotFound"] = "Customer not found",
            ["CustomerAlreadyExists"] = "Customer code already exists",
            ["CustomerSyncCompletedSuccessfully"] = "Customer sync completed successfully"
        },
        ["tr"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["CustomerRetrievedSuccessfully"] = "Müşteri kayıtları başarıyla getirildi",
            ["CustomerCreatedSuccessfully"] = "Müşteri başarıyla oluşturuldu",
            ["CustomerUpdatedSuccessfully"] = "Müşteri başarıyla güncellendi",
            ["CustomerDeletedSuccessfully"] = "Müşteri başarıyla silindi",
            ["CustomerNotFound"] = "Müşteri bulunamadı",
            ["CustomerAlreadyExists"] = "Müşteri kodu zaten mevcut",
            ["CustomerSyncCompletedSuccessfully"] = "Müşteri senkronizasyonu başarıyla tamamlandı"
        }
    };
}
