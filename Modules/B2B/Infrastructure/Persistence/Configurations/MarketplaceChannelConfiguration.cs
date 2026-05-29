using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class MarketplaceChannelConfiguration : BaseEntityConfiguration<MarketplaceChannel>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MarketplaceChannel> builder)
    {
        builder.ToTable("RII_B2B_MARKETPLACE_CHANNEL");
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(160).IsRequired();
        builder.Property(x => x.ProviderKey).HasMaxLength(40).IsRequired();
        builder.Property(x => x.SellerId).HasMaxLength(120);
        builder.Property(x => x.ApiBaseUrl).HasMaxLength(500);
        builder.Property(x => x.AuthType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CredentialsJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasMany(x => x.Listings)
            .WithOne(x => x.Channel)
            .HasForeignKey(x => x.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Code).HasDatabaseName("IX_B2B_MarketplaceChannel_Code").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.ProviderKey, x.SellerId }).HasDatabaseName("IX_B2B_MarketplaceChannel_ProviderSeller");
        builder.HasIndex(x => x.IsActive).HasDatabaseName("IX_B2B_MarketplaceChannel_IsActive");
    }
}
