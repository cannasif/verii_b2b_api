using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class B2bOrderLineConfiguration : BaseEntityConfiguration<B2bOrderLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<B2bOrderLine> builder)
    {
        builder.ToTable("RII_B2B_ORDER_LINE");
        builder.Property(x => x.ProductSku).HasMaxLength(120);
        builder.Property(x => x.ProductName).HasMaxLength(250);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.UnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.LineTotal).HasPrecision(18, 4);

        builder.HasIndex(x => x.OrderId).HasDatabaseName("IX_B2B_OrderLine_OrderId");
        builder.HasIndex(x => new { x.ErpStockId, x.WarehouseCode }).HasDatabaseName("IX_B2B_OrderLine_StockWarehouse");
    }
}
