using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogProductCategoryConfiguration : BaseEntityConfiguration<CatalogProductCategory>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogProductCategory> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_PRODUCT_CATEGORY");
        builder.Property(x => x.AssignmentSource).HasMaxLength(40);

        builder.HasOne(x => x.CatalogProduct)
            .WithMany(x => x.ProductCategories)
            .HasForeignKey(x => x.CatalogProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.CatalogCategory)
            .WithMany(x => x.ProductCategories)
            .HasForeignKey(x => x.CatalogCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CatalogProductId, x.CatalogCategoryId })
            .HasDatabaseName("IX_B2B_CatalogProductCategory_ProductCategory")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.CatalogCategoryId, x.IsPrimary, x.SortOrder }).HasDatabaseName("IX_B2B_CatalogProductCategory_CategorySort");
    }
}
