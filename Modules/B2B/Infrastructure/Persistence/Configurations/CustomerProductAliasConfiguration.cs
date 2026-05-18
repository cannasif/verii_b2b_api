using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CustomerProductAliasConfiguration : BaseEntityConfiguration<CustomerProductAlias>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CustomerProductAlias> builder)
    {
        builder.ToTable("RII_B2B_CUSTOMER_PRODUCT_ALIAS");
        builder.Property(x => x.CustomerSku).HasMaxLength(120).IsRequired();
        builder.Property(x => x.CustomerProductName).HasMaxLength(250);
        builder.Property(x => x.MatchStatus).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ConfidenceScore).HasPrecision(5, 2);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => new { x.CustomerId, x.CustomerSku })
            .HasDatabaseName("IX_B2B_CustomerAlias_CustomerSku")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.MatchStatus).HasDatabaseName("IX_B2B_CustomerAlias_MatchStatus");
        builder.HasIndex(x => x.ErpStockId).HasDatabaseName("IX_B2B_CustomerAlias_ErpStockId");
    }
}
