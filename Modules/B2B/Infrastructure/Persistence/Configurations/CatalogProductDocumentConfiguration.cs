using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogProductDocumentConfiguration : BaseEntityConfiguration<CatalogProductDocument>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogProductDocument> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_PRODUCT_DOCUMENT");
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.Url).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DocumentType).HasMaxLength(60).IsRequired();
        builder.Property(x => x.LanguageCode).HasMaxLength(10);

        builder.HasOne(x => x.CatalogProduct)
            .WithMany(x => x.Documents)
            .HasForeignKey(x => x.CatalogProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CatalogProductId, x.IsActive, x.SortOrder }).HasDatabaseName("IX_B2B_CatalogProductDocument_ProductSort");
    }
}
