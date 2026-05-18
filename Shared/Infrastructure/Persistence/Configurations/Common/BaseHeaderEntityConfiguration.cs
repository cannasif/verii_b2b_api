using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Common;

public abstract class BaseHeaderEntityConfiguration<T> : BaseEntityConfiguration<T> where T : BaseHeaderEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);
        builder.Property(h => h.DocumentNo).HasMaxLength(50);
        builder.Property(h => h.DocumentDate).IsRequired(false);
        builder.Property(h => h.YearCode).HasMaxLength(4).IsRequired();
        builder.Property(h => h.BranchCode).HasMaxLength(10).IsRequired().HasDefaultValue("0");
        builder.Property(h => h.ProjectCode).HasMaxLength(20);
        builder.Property(h => h.OrderId).HasMaxLength(50);
        builder.Property(h => h.PlannedDate).IsRequired(false);
        builder.Property(h => h.IsPlanned).HasDefaultValue(false);
        builder.Property(h => h.DocumentType).HasMaxLength(10).IsRequired();
        builder.Property(h => h.PriorityLevel).IsRequired(false);
        builder.Property(h => h.Description1).HasMaxLength(100);
        builder.Property(h => h.Description2).HasMaxLength(100);
        builder.Property(h => h.CompletionDate).IsRequired(false);
        builder.Property(h => h.IsCompleted).HasDefaultValue(false);
        builder.Property(h => h.IsPendingApproval).HasDefaultValue(false);
        builder.Property(h => h.ApprovalStatus).IsRequired(false);
        builder.Property(h => h.ApprovedByUserId).IsRequired(false);
        builder.Property(h => h.ApprovalDate).IsRequired(false);
        builder.Property(h => h.IsERPIntegrated).HasDefaultValue(false);
        builder.Property(h => h.ERPIntegrationDate).IsRequired(false);
        builder.Property(h => h.ERPReferenceNumber).IsRequired(false);
        builder.Property(h => h.ERPIntegrationStatus).IsRequired(false);
        builder.Property(h => h.ERPErrorMessage).IsRequired(false);
        builder.Property(h => h.AllowLessQuantityBasedOnOrder).IsRequired(false);
        builder.Property(h => h.AllowMoreQuantityBasedOnOrder).IsRequired(false);
    }
}
