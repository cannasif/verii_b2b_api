using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.AccessControl;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.AccessControl;

public sealed class PermissionGroupConfiguration : BaseEntityConfiguration<PermissionGroup>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PermissionGroup> builder)
    {
        builder.ToTable("RII_PERMISSION_GROUPS");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsSystemAdmin).HasDefaultValue(false);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.HasIndex(x => x.Name).IsUnique().HasDatabaseName("IX_PermissionGroups_Name");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_PermissionGroups_IsDeleted");
    }
}
