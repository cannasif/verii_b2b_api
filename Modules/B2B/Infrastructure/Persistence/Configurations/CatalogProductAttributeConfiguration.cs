using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogProductAttributeConfiguration : BaseEntityConfiguration<CatalogProductAttribute>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogProductAttribute> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_PRODUCT_ATTRIBUTE");
        builder.Property(x => x.Value).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.NormalizedValue).HasMaxLength(1000);
        builder.Property(x => x.Unit).HasMaxLength(30);

        builder.HasOne(x => x.CatalogProduct)
            .WithMany(x => x.ProductAttributes)
            .HasForeignKey(x => x.CatalogProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.AttributeDefinition)
            .WithMany(x => x.ProductAttributes)
            .HasForeignKey(x => x.AttributeDefinitionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CatalogProductId, x.AttributeDefinitionId })
            .HasDatabaseName("IX_B2B_CatalogProductAttribute_ProductAttribute")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.AttributeDefinitionId, x.NormalizedValue }).HasDatabaseName("IX_B2B_CatalogProductAttribute_Filter");
    }
}
