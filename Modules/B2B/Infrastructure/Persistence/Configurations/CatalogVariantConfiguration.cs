using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogVariantConfiguration : BaseEntityConfiguration<CatalogVariant>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogVariant> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_VARIANT");
        builder.Property(x => x.VariantSku).HasMaxLength(80).IsRequired();
        builder.Property(x => x.VariantName).HasMaxLength(250).IsRequired();
        builder.Property(x => x.AttributesJson).HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.CatalogProductId).HasDatabaseName("IX_B2B_CatalogVariant_ProductId");
        builder.HasIndex(x => x.VariantSku).HasDatabaseName("IX_B2B_CatalogVariant_Sku").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.ErpStockId).HasDatabaseName("IX_B2B_CatalogVariant_ErpStockId");
    }
}
