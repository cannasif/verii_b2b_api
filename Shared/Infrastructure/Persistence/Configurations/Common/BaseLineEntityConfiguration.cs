using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Common;

public abstract class BaseLineEntityConfiguration<T> : BaseEntityConfiguration<T> where T : BaseLineEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.StockId).IsRequired(false);
        builder.Property(x => x.YapKodId).IsRequired(false);
        builder.Property(x => x.Quantity).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(x => x.SiparisMiktar).HasColumnType("decimal(18,6)").IsRequired(false);
        builder.Property(x => x.Unit).HasMaxLength(10);
        builder.Property(x => x.ErpOrderNo).HasMaxLength(50);
        builder.Property(x => x.ErpOrderId).HasMaxLength(30);
        builder.Property(x => x.Description).HasMaxLength(100);
    }
}
