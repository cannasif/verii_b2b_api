using Wms.Application.Common;

namespace Wms.Application.AccessControl.Services;

public interface IWmsOperationScopeEnforcer
{
    Task<ApiResponse<WmsOperationScopeContext>> GetCurrentScopeAsync(string entityType, CancellationToken cancellationToken = default);
    ApiResponse<T>? EnsureAccess<T>(
        WmsOperationScopeContext scope,
        string? branchCode,
        bool? isAssignedToCurrentUser = null,
        long? createdByUserId = null,
        params long?[] warehouseIds);
}
