using Wms.Application.Common;
using Wms.Application.Communications.Dtos;

namespace Wms.Application.Communications.Services;

public interface ISmtpSettingsService
{
    Task<ApiResponse<SmtpSettingsDto>> GetAsync(CancellationToken cancellationToken = default);
    Task<ApiResponse<SmtpSettingsDto>> UpdateAsync(UpdateSmtpSettingsDto dto, long userId, CancellationToken cancellationToken = default);
    Task<SmtpSettingsRuntimeDto> GetRuntimeAsync(CancellationToken cancellationToken = default);
    void InvalidateCache();
}
