using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Common;

public sealed class WmsAuditLogConfiguration : BaseEntityConfiguration<WmsAuditLog>
{
    protected override void ConfigureEntity(EntityTypeBuilder<WmsAuditLog> builder)
    {
        builder.ToTable("RII_WMS_AUDIT_LOG");

        builder.Property(x => x.TraceId)
            .HasMaxLength(64);

        builder.Property(x => x.EntityType)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.EntityId)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.ActionType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.PerformedByUserEmail)
            .HasMaxLength(256);

        builder.Property(x => x.Result)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.OldValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.NewValues)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ChangedFields)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Reason)
            .HasMaxLength(1024);

        builder.HasIndex(x => x.TraceId);
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.CreatedDate);
    }
}
