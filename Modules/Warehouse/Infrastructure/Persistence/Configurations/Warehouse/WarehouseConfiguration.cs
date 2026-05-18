using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WarehouseEntity = Wms.Domain.Entities.Warehouse.Warehouse;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Warehouse;

public sealed class WarehouseConfiguration : BaseEntityConfiguration<WarehouseEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<WarehouseEntity> builder)
    {
        builder.ToTable("RII_WMS_WAREHOUSE");

        builder.Property(x => x.WarehouseCode).IsRequired();
        builder.Property(x => x.WarehouseName).HasMaxLength(200).IsRequired();

        builder.HasIndex(x => x.WarehouseCode)
            .HasDatabaseName("IX_Warehouse_WarehouseCode")
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.WarehouseName).HasDatabaseName("IX_Warehouse_WarehouseName");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_Warehouse_IsDeleted");
    }
}
