using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Stock;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Stock;

public sealed class StockDetailConfiguration : BaseEntityConfiguration<StockDetail>
{
    protected override void ConfigureEntity(EntityTypeBuilder<StockDetail> builder)
    {
        builder.ToTable("RII_WMS_STOCK_DETAIL");

        builder.Property(x => x.HtmlDescription).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.TechnicalSpecsJson).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.Stock)
            .WithMany()
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.StockId)
            .IsUnique()
            .HasDatabaseName("IX_StockDetail_StockId")
            .HasFilter("[IsDeleted] = 0");
    }
}
