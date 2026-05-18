using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.AccessControl;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.AccessControl;

public sealed class WmsScopePolicyConfiguration : BaseEntityConfiguration<WmsScopePolicy>
{
    protected override void ConfigureEntity(EntityTypeBuilder<WmsScopePolicy> builder)
    {
        builder.ToTable("RII_WMS_SCOPE_POLICIES");
        builder.Property(x => x.Code).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.ScopeType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.IncludeSelf).HasDefaultValue(true);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_WmsScopePolicies_Code");
        builder.HasIndex(x => new { x.EntityType, x.IsActive }).HasDatabaseName("IX_WmsScopePolicies_EntityType_IsActive");
    }
}
