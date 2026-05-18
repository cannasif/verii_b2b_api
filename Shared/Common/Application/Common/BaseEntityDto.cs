namespace Wms.Application.Common;

/// <summary>
/// `_old/reference/verii_wms_api.Application/DTOs/Base/BaseEntityDto.cs` taban DTO davranışını taşır.
/// </summary>
public class BaseEntityDto
{
    public long Id { get; set; }
    public string BranchCode { get; set; } = "0";
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }
    public bool IsDeleted { get; set; }
    public long? CreatedBy { get; set; }
    public long? UpdatedBy { get; set; }
    public long? DeletedBy { get; set; }
    public string? CreatedByFullUser { get; set; }
    public string? UpdatedByFullUser { get; set; }
    public string? DeletedByFullUser { get; set; }
}
