using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class B2bCartConfiguration : BaseEntityConfiguration<B2bCart>
{
    protected override void ConfigureEntity(EntityTypeBuilder<B2bCart> builder)
    {
        builder.ToTable("RII_B2B_CART");
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.Cart)
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.CustomerId, x.Status }).HasDatabaseName("IX_B2B_Cart_CustomerStatus");
    }
}
