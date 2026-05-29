using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class MarketplaceSyncEventConfiguration : BaseEntityConfiguration<MarketplaceSyncEvent>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MarketplaceSyncEvent> builder)
    {
        builder.ToTable("RII_B2B_MARKETPLACE_SYNC_EVENT");
        builder.Property(x => x.OperationType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ExternalBatchId).HasMaxLength(160);
        builder.Property(x => x.RequestJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ResponseJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne(x => x.Channel)
            .WithMany()
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.ChannelId, x.Status }).HasDatabaseName("IX_B2B_MarketplaceSyncEvent_ChannelStatus");
        builder.HasIndex(x => new { x.OperationType, x.Status }).HasDatabaseName("IX_B2B_MarketplaceSyncEvent_OperationStatus");
        builder.HasIndex(x => x.ListingId).HasDatabaseName("IX_B2B_MarketplaceSyncEvent_Listing");
        builder.HasIndex(x => x.RequestedDate).HasDatabaseName("IX_B2B_MarketplaceSyncEvent_RequestedDate");
    }
}
