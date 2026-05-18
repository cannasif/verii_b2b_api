using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.AccessControl;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.AccessControl;

public sealed class PermissionDefinitionConfiguration : BaseEntityConfiguration<PermissionDefinition>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PermissionDefinition> builder)
    {
        builder.ToTable("RII_PERMISSION_DEFINITIONS");
        builder.Property(x => x.Code).IsRequired().HasMaxLength(120);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.AvailableOnWeb).HasDefaultValue(true);
        builder.Property(x => x.AvailableOnMobile).HasDefaultValue(false);
        builder.HasIndex(x => x.Code).IsUnique().HasDatabaseName("IX_PermissionDefinitions_Code");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_PermissionDefinitions_IsDeleted");
    }
}
