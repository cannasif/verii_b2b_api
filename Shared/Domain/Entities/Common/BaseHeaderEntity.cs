using Wms.Domain.Common;

namespace Wms.Domain.Entities.Common;

/// <summary>
/// `_old/reference/verii_wms_api.Domain/Entities/Common/BaseHeaderEntity.cs` içindeki doküman ve approval davranışını korur.
/// </summary>
public abstract class BaseHeaderEntity : BaseEntity
{
    public string? DocumentNo { get; set; }
    public DateTime? DocumentDate { get; set; }
    public string YearCode { get; set; } = DateTime.Now.Year.ToString();
    public string? OrderId { get; set; }
    public string? ProjectCode { get; set; }
    public DateTime? PlannedDate { get; set; }
    public bool IsPlanned { get; set; }
    public string DocumentType { get; set; } = null!;
    public byte? PriorityLevel { get; set; }
    public DateTime? CompletionDate { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsPendingApproval { get; set; }
    public bool? ApprovalStatus { get; set; }
    public long? ApprovedByUserId { get; set; }
    public DateTime? ApprovalDate { get; set; }
    public bool IsERPIntegrated { get; set; }
    public string? ERPReferenceNumber { get; set; }
    public DateTime? ERPIntegrationDate { get; set; }
    public string? ERPIntegrationStatus { get; set; }
    public string? ERPErrorMessage { get; set; }
    public bool? AllowLessQuantityBasedOnOrder { get; set; }
    public bool? AllowMoreQuantityBasedOnOrder { get; set; }
    public string? Description1 { get; set; }
    public string? Description2 { get; set; }

    public void MarkAsCompleted()
    {
        IsCompleted = true;
        CompletionDate = DateTimeProvider.Now;
    }

    public void UndoCompletion()
    {
        IsCompleted = false;
        CompletionDate = null;
    }

    public void SetPendingApproval(bool pending)
    {
        IsPendingApproval = pending;
    }

    public void Approve(long approvedByUserId)
    {
        ApprovalStatus = true;
        ApprovedByUserId = approvedByUserId;
        ApprovalDate = DateTimeProvider.Now;
    }

    public void Reject(long rejectedByUserId)
    {
        ApprovalStatus = false;
        ApprovedByUserId = rejectedByUserId;
        ApprovalDate = DateTimeProvider.Now;
    }

    public void MarkErpIntegrated(string referenceNumber)
    {
        IsERPIntegrated = true;
        ERPReferenceNumber = referenceNumber;
        ERPIntegrationDate = DateTimeProvider.Now;
        ERPIntegrationStatus = "Success";
    }

    public void SetErpError(string errorMessage)
    {
        ERPIntegrationStatus = "Failed";
        ERPErrorMessage = errorMessage;
    }
}
