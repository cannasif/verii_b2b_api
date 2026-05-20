using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogProductMediaConfiguration : BaseEntityConfiguration<CatalogProductMedia>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogProductMedia> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_PRODUCT_MEDIA");
        builder.Property(x => x.Url).HasMaxLength(500).IsRequired();
        builder.Property(x => x.MediaType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.AltText).HasMaxLength(250);

        builder.HasOne(x => x.CatalogProduct)
            .WithMany(x => x.MediaItems)
            .HasForeignKey(x => x.CatalogProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CatalogProductId, x.IsPrimary, x.SortOrder }).HasDatabaseName("IX_B2B_CatalogProductMedia_ProductSort");
    }
}
