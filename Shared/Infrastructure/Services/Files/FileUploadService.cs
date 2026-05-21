using Microsoft.AspNetCore.Http;
using Wms.Application.Common;

namespace Wms.Infrastructure.Services.Files;

public sealed class FileUploadService : IFileUploadService
{
    private readonly ILocalizationService _localizationService;

    public FileUploadService(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<string>> UploadProfilePictureAsync(IFormFile file, long userId, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            var message = _localizationService.GetLocalizedString("ProfilePictureFileIsRequired");
            return ApiResponse<string>.ErrorResult(message, message, 400);
        }

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-pictures");
        Directory.CreateDirectory(uploads);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{userId}_{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploads, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        return ApiResponse<string>.SuccessResult(GetProfilePictureUrl(fileName, userId), _localizationService.GetLocalizedString("ProfilePictureUploadedSuccessfully"));
    }

    public async Task<ApiResponse<string>> UploadStockImageAsync(IFormFile file, long stockId, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            var m = _localizationService.GetLocalizedString("StockImageFileIsRequired");
            return ApiResponse<string>.ErrorResult(m, m, 400);
        }

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "stock-images", stockId.ToString());
        Directory.CreateDirectory(uploads);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploads, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        return ApiResponse<string>.SuccessResult(GetStockImageUrl(fileName, stockId), _localizationService.GetLocalizedString("StockImageUploadedSuccessfully"));
    }

    public async Task<ApiResponse<string>> UploadCatalogProductImageAsync(IFormFile file, long catalogProductId, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            var m = _localizationService.GetLocalizedString("StockImageFileIsRequired");
            return ApiResponse<string>.ErrorResult(m, m, 400);
        }

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "catalog-product-images", catalogProductId.ToString());
        Directory.CreateDirectory(uploads);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploads, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        return ApiResponse<string>.SuccessResult(GetCatalogProductImageUrl(fileName, catalogProductId), _localizationService.GetLocalizedString("StockImageUploadedSuccessfully"));
    }

    public async Task<ApiResponse<string>> UploadSteelGoodReciptAcceptanseInspectionPhotoAsync(IFormFile file, long lineId, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            var m = _localizationService.GetLocalizedString("InspectionPhotoFileIsRequired");
            return ApiResponse<string>.ErrorResult(m, m, 400);
        }

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "steel-good-recipt-acceptanse", lineId.ToString());
        Directory.CreateDirectory(uploads);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploads, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        return ApiResponse<string>.SuccessResult(GetSteelGoodReciptAcceptanseInspectionPhotoUrl(fileName, lineId), _localizationService.GetLocalizedString("InspectionPhotoUploadedSuccessfully"));
    }

    public async Task<ApiResponse<string>> UploadVehicleCheckInImageAsync(IFormFile file, long headerId, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            var m = _localizationService.GetLocalizedString("VehicleCheckInImageFileIsRequired");
            return ApiResponse<string>.ErrorResult(m, m, 400);
        }

        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "vehicle-check-in", headerId.ToString());
        Directory.CreateDirectory(uploads);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(uploads, fileName);

        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, cancellationToken);

        return ApiResponse<string>.SuccessResult(GetVehicleCheckInImageUrl(fileName, headerId), _localizationService.GetLocalizedString("VehicleCheckInImageUploadedSuccessfully"));
    }

    public Task<ApiResponse<bool>> DeleteProfilePictureAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Task.FromResult(ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("ProfilePictureDeletedSuccessfully")));
        }

        return DeleteFileAsync(fileUrl, cancellationToken);
    }

    public string GetProfilePictureUrl(string fileName, long userId)
    {
        _ = userId;
        return $"/profile-pictures/{fileName}";
    }

    public Task<ApiResponse<bool>> DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        _ = cancellationToken;
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Task.FromResult(ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("ProfilePictureDeletedSuccessfully")));
        }

        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.FromResult(ApiResponse<bool>.SuccessResult(true, _localizationService.GetLocalizedString("ProfilePictureDeletedSuccessfully")));
    }

    public string GetStockImageUrl(string fileName, long stockId)
    {
        return $"/stock-images/{stockId}/{fileName}";
    }

    public string GetCatalogProductImageUrl(string fileName, long catalogProductId)
    {
        return $"/catalog-product-images/{catalogProductId}/{fileName}";
    }

    public string GetSteelGoodReciptAcceptanseInspectionPhotoUrl(string fileName, long lineId)
    {
        return $"/steel-good-recipt-acceptanse/{lineId}/{fileName}";
    }

    public string GetVehicleCheckInImageUrl(string fileName, long headerId)
    {
        return $"/vehicle-check-in/{headerId}/{fileName}";
    }
}
