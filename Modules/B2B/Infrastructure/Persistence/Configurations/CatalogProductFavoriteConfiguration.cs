using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogProductFavoriteConfiguration : BaseEntityConfiguration<CatalogProductFavorite>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogProductFavorite> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_PRODUCT_FAVORITE");
        builder.Property(x => x.FavoriteKey).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Sku).HasMaxLength(80);
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Buyer)
            .WithMany()
            .HasForeignKey(x => x.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CatalogProduct)
            .WithMany()
            .HasForeignKey(x => x.CatalogProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CatalogVariant)
            .WithMany()
            .HasForeignKey(x => x.CatalogVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CompanyId, x.BuyerId, x.UserId, x.FavoriteKey })
            .HasDatabaseName("IX_B2B_ProductFavorite_ScopeKey")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CatalogProductId).HasDatabaseName("IX_B2B_ProductFavorite_ProductId");
        builder.HasIndex(x => x.CatalogVariantId).HasDatabaseName("IX_B2B_ProductFavorite_VariantId");
        builder.HasIndex(x => x.ErpStockId).HasDatabaseName("IX_B2B_ProductFavorite_ErpStockId");
    }
}
