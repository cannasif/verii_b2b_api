using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CustomerPriceListItemConfiguration : BaseEntityConfiguration<CustomerPriceListItem>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CustomerPriceListItem> builder)
    {
        builder.ToTable("RII_B2B_CUSTOMER_PRICE_LIST_ITEM");
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.MinQuantity).HasPrecision(18, 4);
        builder.Property(x => x.DiscountRate).HasPrecision(5, 2);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();

        builder.HasIndex(x => x.PriceListId).HasDatabaseName("IX_B2B_PriceListItem_ListId");
        builder.HasIndex(x => new { x.CustomerId, x.ErpStockId, x.CatalogProductId }).HasDatabaseName("IX_B2B_PriceListItem_ProductScope");
    }
}
