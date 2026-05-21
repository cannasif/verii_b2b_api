using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogCategoryFavoriteConfiguration : BaseEntityConfiguration<CatalogCategoryFavorite>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogCategoryFavorite> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_CATEGORY_FAVORITE");
        builder.Property(x => x.Note).HasMaxLength(500);

        builder.HasOne(x => x.Company)
            .WithMany()
            .HasForeignKey(x => x.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Buyer)
            .WithMany()
            .HasForeignKey(x => x.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CatalogCategory)
            .WithMany()
            .HasForeignKey(x => x.CatalogCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CompanyId, x.BuyerId, x.UserId, x.CatalogCategoryId })
            .HasDatabaseName("IX_B2B_CategoryFavorite_ScopeCategory")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CatalogCategoryId).HasDatabaseName("IX_B2B_CategoryFavorite_CategoryId");
    }
}
