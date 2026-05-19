using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class PaymentOrderConfiguration : BaseEntityConfiguration<PaymentOrder>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.ToTable("RII_B2B_PAYMENT_ORDER");
        builder.Property(x => x.PaymentOrderNumber).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 4);
        builder.Property(x => x.RemainingAmount).HasPrecision(18, 4);
        builder.Property(x => x.PaymentMethod).HasMaxLength(80);
        builder.Property(x => x.ProviderKey).HasMaxLength(80);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => x.PaymentOrderNumber).HasDatabaseName("IX_B2B_PaymentOrder_Number").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.OrderId).HasDatabaseName("IX_B2B_PaymentOrder_OrderId");
        builder.HasIndex(x => new { x.CustomerId, x.Status }).HasDatabaseName("IX_B2B_PaymentOrder_CustomerStatus");

        builder.HasMany(x => x.Installments)
            .WithOne(x => x.PaymentOrder)
            .HasForeignKey(x => x.PaymentOrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
