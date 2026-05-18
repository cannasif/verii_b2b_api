using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CustomerPriceListConfiguration : BaseEntityConfiguration<CustomerPriceList>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CustomerPriceList> builder)
    {
        builder.ToTable("RII_B2B_CUSTOMER_PRICE_LIST");
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(180).IsRequired();
        builder.Property(x => x.CustomerGroupCode).HasMaxLength(80);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();

        builder.HasMany(x => x.Items)
            .WithOne(x => x.PriceList)
            .HasForeignKey(x => x.PriceListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Code).HasDatabaseName("IX_B2B_PriceList_Code").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.CustomerId, x.CustomerGroupCode, x.IsActive }).HasDatabaseName("IX_B2B_PriceList_CustomerScope");
    }
}
