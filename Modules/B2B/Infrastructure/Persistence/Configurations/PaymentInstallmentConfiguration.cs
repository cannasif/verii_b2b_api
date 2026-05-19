using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class PaymentInstallmentConfiguration : BaseEntityConfiguration<PaymentInstallment>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentInstallment> builder)
    {
        builder.ToTable("RII_B2B_PAYMENT_INSTALLMENT");
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.PaidAmount).HasPrecision(18, 4);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => x.PaymentOrderId).HasDatabaseName("IX_B2B_PaymentInstallment_OrderId");
        builder.HasIndex(x => new { x.PaymentOrderId, x.InstallmentNumber }).HasDatabaseName("IX_B2B_PaymentInstallment_Number").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.DueDate).HasDatabaseName("IX_B2B_PaymentInstallment_DueDate");
    }
}
