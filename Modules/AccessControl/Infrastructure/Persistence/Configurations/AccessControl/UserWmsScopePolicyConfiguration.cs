using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.AccessControl;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.AccessControl;

public sealed class UserWmsScopePolicyConfiguration : BaseEntityConfiguration<UserWmsScopePolicy>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserWmsScopePolicy> builder)
    {
        builder.ToTable("RII_USER_WMS_SCOPE_POLICIES");
        builder.Property(x => x.BranchCode).HasMaxLength(20);
        builder.HasIndex(x => new { x.UserId, x.WmsScopePolicyId, x.BranchCode, x.WarehouseId })
            .IsUnique()
            .HasDatabaseName("IX_UserWmsScopePolicies_UniqueAssignment");
        builder.HasOne(x => x.WmsScopePolicy)
            .WithMany(x => x.UserAssignments)
            .HasForeignKey(x => x.WmsScopePolicyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
