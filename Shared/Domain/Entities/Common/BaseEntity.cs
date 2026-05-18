using Wms.Domain.Common;

namespace Wms.Domain.Entities.Common;

/// <summary>
/// `_old/reference/verii_wms_api.Domain/Entities/Common/BaseEntity.cs` temel audit ve soft delete davranışını taşır.
/// </summary>
public abstract class BaseEntity : IBranchScopedEntity
{
    public long Id { get; set; }
    public string BranchCode { get; set; } = "0";
    public DateTime? CreatedDate { get; set; } = DateTimeProvider.Now;
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }

    public void MarkAsDeleted(long? deletedByUserId = null)
    {
        IsDeleted = true;
        DeletedDate = DateTimeProvider.Now;
        DeletedBy = deletedByUserId;
    }

    public void SetUpdatedInfo(long? updatedByUserId = null)
    {
        UpdatedDate = DateTimeProvider.Now;
        UpdatedBy = updatedByUserId;
    }
}
