using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockEntity = Wms.Domain.Entities.Stock.Stock;
using Wms.Infrastructure.Persistence.Configurations.Common;
using YapKodEntity = Wms.Domain.Entities.YapKod.YapKod;

namespace Wms.Infrastructure.Persistence.Configurations.YapKod;

public sealed class YapKodConfiguration : BaseEntityConfiguration<YapKodEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<YapKodEntity> builder)
    {
        builder.ToTable("RII_WMS_YAPKOD");

        builder.Property(x => x.YapKodCode).HasMaxLength(15).IsRequired();
        builder.Property(x => x.YapAcik).HasMaxLength(255).IsRequired();
        builder.Property(x => x.YplndrStokKod).HasMaxLength(35);

        builder.HasOne<StockEntity>()
            .WithMany()
            .HasForeignKey(x => x.StockId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.YapKodCode)
            .HasDatabaseName("IX_YapKod_YapKodCode")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.YapAcik).HasDatabaseName("IX_YapKod_YapAcik");
        builder.HasIndex(x => x.StockId).HasDatabaseName("IX_YapKod_StockId");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_YapKod_IsDeleted");
    }
}
