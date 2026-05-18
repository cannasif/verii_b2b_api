using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockEntity = Wms.Domain.Entities.Stock.Stock;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Stock;

public sealed class StockConfiguration : BaseEntityConfiguration<StockEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<StockEntity> builder)
    {
        builder.ToTable("RII_WMS_STOCK");

        builder.Property(x => x.ErpStockCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.StockName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(20);
        builder.Property(x => x.UreticiKodu).HasMaxLength(50);
        builder.Property(x => x.GrupKodu).HasMaxLength(50);
        builder.Property(x => x.GrupAdi).HasMaxLength(250);
        builder.Property(x => x.Kod1).HasMaxLength(50);
        builder.Property(x => x.Kod1Adi).HasMaxLength(250);
        builder.Property(x => x.Kod2).HasMaxLength(50);
        builder.Property(x => x.Kod2Adi).HasMaxLength(250);
        builder.Property(x => x.Kod3).HasMaxLength(50);
        builder.Property(x => x.Kod3Adi).HasMaxLength(250);
        builder.Property(x => x.Kod4).HasMaxLength(50);
        builder.Property(x => x.Kod4Adi).HasMaxLength(250);
        builder.Property(x => x.Kod5).HasMaxLength(50);
        builder.Property(x => x.Kod5Adi).HasMaxLength(250);

        builder.HasIndex(x => x.ErpStockCode)
            .HasDatabaseName("IX_Stock_ErpStockCode")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.StockName).HasDatabaseName("IX_Stock_StockName");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_Stock_IsDeleted");
    }
}
