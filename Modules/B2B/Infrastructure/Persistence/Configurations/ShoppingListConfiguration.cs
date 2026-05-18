using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class ShoppingListConfiguration : BaseEntityConfiguration<ShoppingList>
{
    protected override void ConfigureEntity(EntityTypeBuilder<ShoppingList> builder)
    {
        builder.ToTable("RII_B2B_SHOPPING_LIST");
        builder.Property(x => x.Name).HasMaxLength(180).IsRequired();
        builder.Property(x => x.ListType).HasMaxLength(40).IsRequired();

        builder.HasMany(x => x.Lines).WithOne(x => x.ShoppingList).HasForeignKey(x => x.ShoppingListId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.CompanyId, x.BuyerId, x.ListType }).HasDatabaseName("IX_B2B_ShoppingList_Scope");
    }
}
