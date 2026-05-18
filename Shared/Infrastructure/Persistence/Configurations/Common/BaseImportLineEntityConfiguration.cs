using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Common;

public abstract class BaseImportLineEntityConfiguration<T> : BaseEntityConfiguration<T> where T : BaseImportLineEntity
{
    public override void Configure(EntityTypeBuilder<T> builder)
    {
        base.Configure(builder);
        builder.Property(x => x.StockId).IsRequired(false);
        builder.Property(x => x.YapKodId).IsRequired(false);
        builder.Property(x => x.Description1).HasMaxLength(100);
        builder.Property(x => x.Description2).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(255);
    }
}
