namespace Wms.Domain.Entities.AccessControl;

public static class WmsScopePolicyScopeTypes
{
    public const string AssignedOnly = "AssignedOnly";
    public const string Branch = "Branch";
    public const string Warehouse = "Warehouse";
    public const string BranchAndWarehouse = "BranchAndWarehouse";
    public const string Unrestricted = "Unrestricted";

    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        AssignedOnly,
        Branch,
        Warehouse,
        BranchAndWarehouse,
        Unrestricted
    };
}
