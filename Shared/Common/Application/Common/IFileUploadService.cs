using Microsoft.AspNetCore.Http;

namespace Wms.Application.Common;

public interface IFileUploadService
{
    Task<ApiResponse<string>> UploadProfilePictureAsync(IFormFile file, long userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> UploadStockImageAsync(IFormFile file, long stockId, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> UploadCatalogProductImageAsync(IFormFile file, long catalogProductId, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> UploadSteelGoodReciptAcceptanseInspectionPhotoAsync(IFormFile file, long lineId, CancellationToken cancellationToken = default);
    Task<ApiResponse<string>> UploadVehicleCheckInImageAsync(IFormFile file, long headerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteProfilePictureAsync(string fileUrl, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
    string GetProfilePictureUrl(string fileName, long userId);
    string GetStockImageUrl(string fileName, long stockId);
    string GetCatalogProductImageUrl(string fileName, long catalogProductId);
    string GetSteelGoodReciptAcceptanseInspectionPhotoUrl(string fileName, long lineId);
    string GetVehicleCheckInImageUrl(string fileName, long headerId);
}
