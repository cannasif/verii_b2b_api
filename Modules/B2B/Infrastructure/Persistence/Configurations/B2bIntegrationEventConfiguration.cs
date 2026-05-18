using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class B2bIntegrationEventConfiguration : BaseEntityConfiguration<B2bIntegrationEvent>
{
    protected override void ConfigureEntity(EntityTypeBuilder<B2bIntegrationEvent> builder)
    {
        builder.ToTable("RII_B2B_INTEGRATION_EVENT");
        builder.Property(x => x.Direction).HasMaxLength(30).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(120).IsRequired();
        builder.Property(x => x.EntityName).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ExternalReference).HasMaxLength(180);
        builder.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(x => new { x.Status, x.EventType }).HasDatabaseName("IX_B2B_IntegrationEvent_StatusType");
        builder.HasIndex(x => new { x.EntityName, x.EntityId }).HasDatabaseName("IX_B2B_IntegrationEvent_Entity");
    }
}
