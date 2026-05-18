using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogProductConfiguration : BaseEntityConfiguration<CatalogProduct>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogProduct> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_PRODUCT");
        builder.Property(x => x.Sku).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Brand).HasMaxLength(120);
        builder.Property(x => x.CategoryPath).HasMaxLength(500);
        builder.Property(x => x.PrimaryImageUrl).HasMaxLength(500);
        builder.Property(x => x.Description).HasColumnType("nvarchar(max)");
        builder.Property(x => x.SearchText).HasMaxLength(1000);

        builder.HasMany(x => x.Variants)
            .WithOne(x => x.CatalogProduct)
            .HasForeignKey(x => x.CatalogProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Sku).HasDatabaseName("IX_B2B_CatalogProduct_Sku").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.Slug).HasDatabaseName("IX_B2B_CatalogProduct_Slug").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.IsPublished).HasDatabaseName("IX_B2B_CatalogProduct_IsPublished");
    }
}
