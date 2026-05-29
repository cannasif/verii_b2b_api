using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class MarketplaceListingConfiguration : BaseEntityConfiguration<MarketplaceListing>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MarketplaceListing> builder)
    {
        builder.ToTable("RII_B2B_MARKETPLACE_LISTING");
        builder.Property(x => x.Sku).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Barcode).HasMaxLength(80);
        builder.Property(x => x.MarketplaceProductId).HasMaxLength(160);
        builder.Property(x => x.MarketplaceListingId).HasMaxLength(160);
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.LastPushedPrice).HasColumnType("decimal(18,4)");
        builder.Property(x => x.LastPushedQuantity).HasColumnType("decimal(18,4)");
        builder.Property(x => x.CurrencyCode).HasMaxLength(10).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);

        builder.HasOne(x => x.CatalogProduct)
            .WithMany()
            .HasForeignKey(x => x.CatalogProductId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.SyncEvents)
            .WithOne(x => x.Listing)
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => new { x.ChannelId, x.Sku }).HasDatabaseName("IX_B2B_MarketplaceListing_ChannelSku").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.ChannelId, x.MarketplaceListingId }).HasDatabaseName("IX_B2B_MarketplaceListing_ChannelExternalId");
        builder.HasIndex(x => x.CatalogProductId).HasDatabaseName("IX_B2B_MarketplaceListing_CatalogProduct");
        builder.HasIndex(x => x.ErpStockId).HasDatabaseName("IX_B2B_MarketplaceListing_ErpStock");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_B2B_MarketplaceListing_Status");
    }
}
