namespace Wms.Application.Common;

public static class BranchCodeDefaults
{
    public const string Fallback = "0";

    public static string Normalize(string? branchCode)
    {
        return string.IsNullOrWhiteSpace(branchCode) ? Fallback : branchCode.Trim();
    }
}
