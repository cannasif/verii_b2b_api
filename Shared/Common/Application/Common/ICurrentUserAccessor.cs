namespace Wms.Application.Common;

/// <summary>
/// Application servislerine kullanıcı kimliğini framework bağımsız taşır.
/// </summary>
public interface ICurrentUserAccessor
{
    long? UserId { get; }
    string? UserEmail { get; }
    string? BranchCode { get; }
    bool HasPermission(string permissionCode);
}
