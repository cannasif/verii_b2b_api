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
        builder.Property(x => x.DiscountRate1).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount1).HasPrecision(18, 4);
        builder.Property(x => x.DiscountRate2).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount2).HasPrecision(18, 4);
        builder.Property(x => x.DiscountRate3).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount3).HasPrecision(18, 4);
        builder.Property(x => x.VatRate).HasPrecision(18, 4);
        builder.Property(x => x.VatAmount).HasPrecision(18, 4);
        builder.Property(x => x.LineTotal).HasPrecision(18, 4);
        builder.Property(x => x.LineGrandTotal).HasPrecision(18, 4);
        builder.Property(x => x.PriceSource).HasMaxLength(80);
        builder.Property(x => x.ExchangeRate).HasPrecision(18, 8).HasDefaultValue(1m);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Description1).HasMaxLength(500);
        builder.Property(x => x.Description2).HasMaxLength(500);
        builder.Property(x => x.Description3).HasMaxLength(500);
        builder.Property(x => x.RelatedProductKey).HasMaxLength(120);
        builder.Property(x => x.ErpProjectCode).HasMaxLength(50);

        builder.HasIndex(x => x.OrderId).HasDatabaseName("IX_B2B_OrderLine_OrderId");
        builder.HasIndex(x => new { x.ErpStockId, x.WarehouseCode }).HasDatabaseName("IX_B2B_OrderLine_StockWarehouse");
    }
}
