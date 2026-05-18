namespace Wms.Domain.Entities.Common;

public interface IBranchScopedEntity
{
    string BranchCode { get; set; }
}
