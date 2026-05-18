using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Stock;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Stock;

public sealed class StockImageConfiguration : BaseEntityConfiguration<StockImage>
{
    protected override void ConfigureEntity(EntityTypeBuilder<StockImage> builder)
    {
        builder.ToTable("RII_WMS_STOCK_IMAGE");

        builder.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
        builder.Property(x => x.AltText).HasMaxLength(250);
        builder.Property(x => x.IsPrimary).HasDefaultValue(false);

        builder.HasOne(x => x.Stock)
            .WithMany()
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.StockId, x.IsPrimary })
            .HasDatabaseName("IX_StockImage_Stock_Primary");
        builder.HasIndex(x => new { x.StockId, x.SortOrder })
            .HasDatabaseName("IX_StockImage_Stock_SortOrder");
    }
}
