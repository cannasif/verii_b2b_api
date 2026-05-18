using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class ShoppingListLineConfiguration : BaseEntityConfiguration<ShoppingListLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ShoppingListLine> builder)
    {
        builder.ToTable("RII_B2B_SHOPPING_LIST_LINE");
        builder.Property(x => x.Sku).HasMaxLength(120);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);

        builder.HasIndex(x => x.ShoppingListId).HasDatabaseName("IX_B2B_ShoppingListLine_ListId");
        builder.HasIndex(x => x.ErpStockId).HasDatabaseName("IX_B2B_ShoppingListLine_ErpStockId");
    }
}
