using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Common;

public abstract class BaseRouteEntityConfiguration<T> : BaseEntityConfiguration<T> where T : BaseRouteEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.ScannedBarcode).HasMaxLength(75);
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.SerialNo).HasMaxLength(50);
        builder.Property(x => x.SerialNo2).HasMaxLength(50);
        builder.Property(x => x.SerialNo3).HasMaxLength(50);
        builder.Property(x => x.SerialNo4).HasMaxLength(50);
        builder.Property(x => x.SourceWarehouseId).IsRequired(false);
        builder.Property(x => x.TargetWarehouseId).IsRequired(false);
        builder.Property(x => x.SourceCellCode).HasMaxLength(20);
        builder.Property(x => x.TargetCellCode).HasMaxLength(20);
    }
}
