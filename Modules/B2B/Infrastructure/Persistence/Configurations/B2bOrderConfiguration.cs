using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class B2bOrderConfiguration : BaseEntityConfiguration<B2bOrder>
{
    protected override void ConfigureEntity(EntityTypeBuilder<B2bOrder> builder)
    {
        builder.ToTable("RII_B2B_ORDER");
        builder.Property(x => x.OrderNumber).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ExternalErpOrderNumber).HasMaxLength(80);
        builder.Property(x => x.Subtotal).HasPrecision(18, 4);
        builder.Property(x => x.TaxTotal).HasPrecision(18, 4);
        builder.Property(x => x.GrandTotal).HasPrecision(18, 4);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.OrderNumber).HasDatabaseName("IX_B2B_Order_OrderNumber").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.CustomerId, x.Status }).HasDatabaseName("IX_B2B_Order_CustomerStatus");
    }
}
