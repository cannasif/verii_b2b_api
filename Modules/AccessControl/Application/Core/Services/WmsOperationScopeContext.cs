namespace Wms.Application.AccessControl.Services;

public sealed class WmsOperationScopeContext
{
    public long CurrentUserId { get; init; }
    public string CurrentBranchCode { get; init; } = "0";
    public string EntityType { get; init; } = string.Empty;
    public bool HasExplicitPolicy { get; init; }
    public bool IsUnrestricted { get; init; }
    public bool RequiresAssignedRecords { get; init; }
    public bool IncludeSelf { get; init; }
    public IReadOnlySet<string> AllowedBranchCodes { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public IReadOnlySet<long> AllowedWarehouseIds { get; init; } = new HashSet<long>();

    public bool HasBranchRestrictions => AllowedBranchCodes.Count > 0;
    public bool HasWarehouseRestrictions => AllowedWarehouseIds.Count > 0;

    public bool AllowsBranch(string? branchCode)
    {
        var normalizedBranchCode = NormalizeBranchCode(branchCode);
        if (HasBranchRestrictions)
        {
            return AllowedBranchCodes.Contains(normalizedBranchCode);
        }

        return string.Equals(normalizedBranchCode, CurrentBranchCode, StringComparison.OrdinalIgnoreCase);
    }

    public bool AllowsAnyWarehouse(params long?[] warehouseIds)
    {
        if (!HasWarehouseRestrictions)
        {
            return true;
        }

        return warehouseIds.Any(warehouseId => warehouseId.HasValue && AllowedWarehouseIds.Contains(warehouseId.Value));
    }

    public bool AllowsAssignedRecord(bool isAssignedToCurrentUser, long? createdByUserId = null)
    {
        if (!RequiresAssignedRecords)
        {
            return true;
        }

        if (isAssignedToCurrentUser)
        {
            return true;
        }

        return IncludeSelf && createdByUserId.HasValue && createdByUserId.Value == CurrentUserId;
    }

    private static string NormalizeBranchCode(string? branchCode)
    {
        return string.IsNullOrWhiteSpace(branchCode) ? "0" : branchCode.Trim();
    }
}
