using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogAttributeDefinitionConfiguration : BaseEntityConfiguration<CatalogAttributeDefinition>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogAttributeDefinition> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_ATTRIBUTE_DEFINITION");
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.DataType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Unit).HasMaxLength(30);
        builder.Property(x => x.AllowedValuesJson).HasColumnType("nvarchar(max)");

        builder.HasOne(x => x.CatalogCategory)
            .WithMany(x => x.AttributeDefinitions)
            .HasForeignKey(x => x.CatalogCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.CatalogCategoryId, x.Code })
            .HasDatabaseName("IX_B2B_CatalogAttributeDefinition_CategoryCode")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.IsActive, x.IsRequired, x.SortOrder }).HasDatabaseName("IX_B2B_CatalogAttributeDefinition_ActiveRequiredSort");
    }
}
