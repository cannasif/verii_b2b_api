using Wms.Modules.NetsisIntegrations.Application.Dtos;

namespace Wms.Modules.NetsisIntegrations.Application.Services;

public interface INetsisItemSlipService
{
    Task<NetsisItemSlipCreateResponseDto> CreateAsync(
        NetsisItemSlipCreateRequestDto request,
        string operation,
        int? configuredDocumentType,
        CancellationToken cancellationToken = default);
}
