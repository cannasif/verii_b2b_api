namespace Wms.Application.Common;

public interface ICurrentUserService
{
    long? UserId { get; }
    string? UserEmail { get; }
    string? BranchCode { get; }
}
