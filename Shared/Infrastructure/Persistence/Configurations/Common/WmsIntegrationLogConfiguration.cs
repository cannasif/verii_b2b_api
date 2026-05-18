using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Common;

public sealed class WmsIntegrationLogConfiguration : BaseEntityConfiguration<WmsIntegrationLog>
{
    protected override void ConfigureEntity(EntityTypeBuilder<WmsIntegrationLog> builder)
    {
        builder.ToTable("RII_WMS_INTEGRATION_LOG");

        builder.Property(x => x.TraceId)
            .HasMaxLength(64);

        builder.Property(x => x.IntegrationType)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.TargetSystem)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Operation)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Source)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(x => x.RequestMetadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ResponseMetadata)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2048);

        builder.Property(x => x.ErrorType)
            .HasMaxLength(256);

        builder.HasIndex(x => x.TraceId);
        builder.HasIndex(x => new { x.TargetSystem, x.Operation });
        builder.HasIndex(x => x.CreatedDate);
    }
}
