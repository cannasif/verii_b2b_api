using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class InventorySnapshotConfiguration : BaseEntityConfiguration<InventorySnapshot>
{
    protected override void ConfigureEntity(EntityTypeBuilder<InventorySnapshot> builder)
    {
        builder.ToTable("RII_B2B_INVENTORY_SNAPSHOT");
        builder.Property(x => x.ErpStockCode).HasMaxLength(80);
        builder.Property(x => x.WarehouseName).HasMaxLength(180);
        builder.Property(x => x.Unit).HasMaxLength(30);
        builder.Property(x => x.AvailableQuantity).HasPrecision(18, 4);
        builder.Property(x => x.ReservedQuantity).HasPrecision(18, 4);

        builder.HasIndex(x => new { x.ErpStockId, x.WarehouseCode }).HasDatabaseName("IX_B2B_Inventory_ErpStockWarehouse");
        builder.HasIndex(x => x.SnapshotDate).HasDatabaseName("IX_B2B_Inventory_SnapshotDate");
    }
}
