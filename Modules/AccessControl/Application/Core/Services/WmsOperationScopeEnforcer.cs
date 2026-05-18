using Wms.Application.Common;

namespace Wms.Application.AccessControl.Services;

public sealed class WmsOperationScopeEnforcer : IWmsOperationScopeEnforcer
{
    private readonly ICurrentUserAccessor _currentUserAccessor;
    private readonly IWmsScopePolicyService _wmsScopePolicyService;
    private readonly ILocalizationService _localizationService;

    public WmsOperationScopeEnforcer(
        ICurrentUserAccessor currentUserAccessor,
        IWmsScopePolicyService wmsScopePolicyService,
        ILocalizationService localizationService)
    {
        _currentUserAccessor = currentUserAccessor;
        _wmsScopePolicyService = wmsScopePolicyService;
        _localizationService = localizationService;
    }

    public async Task<ApiResponse<WmsOperationScopeContext>> GetCurrentScopeAsync(string entityType, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserAccessor.UserId;
        if (!userId.HasValue)
        {
            var unauthorized = _localizationService.GetLocalizedString("Unauthorized");
            return ApiResponse<WmsOperationScopeContext>.ErrorResult(unauthorized, unauthorized, 401);
        }

        var resolutionResponse = await _wmsScopePolicyService.ResolveForUserAsync(userId.Value, entityType, cancellationToken);
        if (!resolutionResponse.Success || resolutionResponse.Data == null)
        {
            return ApiResponse<WmsOperationScopeContext>.ErrorResult(
                resolutionResponse.Message,
                resolutionResponse.ExceptionMessage,
                resolutionResponse.StatusCode,
                errorCode: resolutionResponse.ErrorCode,
                details: resolutionResponse.Details);
        }

        var currentBranchCode = string.IsNullOrWhiteSpace(_currentUserAccessor.BranchCode) ? "0" : _currentUserAccessor.BranchCode.Trim();
        var context = new WmsOperationScopeContext
        {
            CurrentUserId = userId.Value,
            CurrentBranchCode = currentBranchCode,
            EntityType = entityType,
            HasExplicitPolicy = resolutionResponse.Data.HasExplicitPolicy,
            IsUnrestricted = resolutionResponse.Data.IsUnrestricted,
            RequiresAssignedRecords = resolutionResponse.Data.RequiresAssignedRecords,
            IncludeSelf = resolutionResponse.Data.IncludeSelf,
            AllowedBranchCodes = resolutionResponse.Data.BranchCodes
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase),
            AllowedWarehouseIds = resolutionResponse.Data.WarehouseIds.ToHashSet()
        };

        return ApiResponse<WmsOperationScopeContext>.SuccessResult(context, _localizationService.GetLocalizedString("OperationSuccessful"));
    }

    public ApiResponse<T>? EnsureAccess<T>(
        WmsOperationScopeContext scope,
        string? branchCode,
        bool? isAssignedToCurrentUser = null,
        long? createdByUserId = null,
        params long?[] warehouseIds)
    {
        if (!scope.AllowsBranch(branchCode))
        {
            return CreateForbiddenResponse<T>();
        }

        if (!scope.AllowsAnyWarehouse(warehouseIds))
        {
            return CreateForbiddenResponse<T>();
        }

        if (isAssignedToCurrentUser.HasValue && !scope.AllowsAssignedRecord(isAssignedToCurrentUser.Value, createdByUserId))
        {
            return CreateForbiddenResponse<T>();
        }

        return null;
    }

    private ApiResponse<T> CreateForbiddenResponse<T>()
    {
        var unauthorized = _localizationService.GetLocalizedString("UnauthorizedAccess");
        return ApiResponse<T>.ErrorResult(unauthorized, unauthorized, 403);
    }
}
