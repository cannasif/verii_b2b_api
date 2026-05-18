using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class PurchaseApprovalRuleConfiguration : BaseEntityConfiguration<PurchaseApprovalRule>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PurchaseApprovalRule> builder)
    {
        builder.ToTable("RII_B2B_PURCHASE_APPROVAL_RULE");
        builder.Property(x => x.RuleName).HasMaxLength(180).IsRequired();
        builder.Property(x => x.MinOrderAmount).HasPrecision(18, 4);
        builder.Property(x => x.MaxOrderAmount).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ApproverRoleCode).HasMaxLength(60).IsRequired();

        builder.HasIndex(x => new { x.CompanyId, x.IsActive }).HasDatabaseName("IX_B2B_ApprovalRule_CompanyActive");
    }
}
