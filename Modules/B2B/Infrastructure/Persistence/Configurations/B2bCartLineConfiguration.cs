using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class B2bCartLineConfiguration : BaseEntityConfiguration<B2bCartLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<B2bCartLine> builder)
    {
        builder.ToTable("RII_B2B_CART_LINE");
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();

        builder.HasIndex(x => x.CartId).HasDatabaseName("IX_B2B_CartLine_CartId");
        builder.HasIndex(x => new { x.ErpStockId, x.WarehouseCode }).HasDatabaseName("IX_B2B_CartLine_StockWarehouse");
    }
}
