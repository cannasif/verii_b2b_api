using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogCategoryConfiguration : BaseEntityConfiguration<CatalogCategory>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogCategory> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_CATEGORY");
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.FullPath).HasMaxLength(600);
        builder.Property(x => x.ImageUrl).HasMaxLength(500);
        builder.Property(x => x.IconName).HasMaxLength(80);
        builder.Property(x => x.ColorHex).HasMaxLength(20);

        builder.HasOne(x => x.ParentCategory)
            .WithMany(x => x.Children)
            .HasForeignKey(x => x.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).HasDatabaseName("IX_B2B_CatalogCategory_Code").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.ParentCategoryId).HasDatabaseName("IX_B2B_CatalogCategory_ParentId");
        builder.HasIndex(x => x.FullPath).HasDatabaseName("IX_B2B_CatalogCategory_FullPath");
        builder.HasIndex(x => new { x.IsActive, x.SortOrder }).HasDatabaseName("IX_B2B_CatalogCategory_ActiveSort");
    }
}
