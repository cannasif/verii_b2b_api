using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class PaymentTransactionConfiguration : BaseEntityConfiguration<PaymentTransaction>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.ToTable("RII_B2B_PAYMENT_TRANSACTION");
        builder.Property(x => x.ProviderKey).HasMaxLength(80).IsRequired();
        builder.Property(x => x.ExternalTransactionId).HasMaxLength(160);
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.PaymentMethod).HasMaxLength(80);
        builder.Property(x => x.ProviderConversationId).HasMaxLength(120);
        builder.Property(x => x.BinNumber).HasMaxLength(8);
        builder.Property(x => x.CardType).HasMaxLength(40);
        builder.Property(x => x.CardAssociation).HasMaxLength(60);
        builder.Property(x => x.CardFamily).HasMaxLength(120);
        builder.Property(x => x.BankName).HasMaxLength(180);
        builder.Property(x => x.BankCode).HasMaxLength(40);
        builder.Property(x => x.CallbackPayloadJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.InstallmentPlanJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.ProviderPaymentAmount).HasPrecision(18, 4);
        builder.Property(x => x.ProviderCollectedAmount).HasPrecision(18, 4);
        builder.Property(x => x.ProviderRate).HasPrecision(18, 4);
        builder.Property(x => x.ProviderCommissionAmount).HasPrecision(18, 4);

        builder.HasIndex(x => x.OrderId).HasDatabaseName("IX_B2B_Payment_OrderId");
        builder.HasIndex(x => x.PaymentOrderId).HasDatabaseName("IX_B2B_Payment_PaymentOrderId");
        builder.HasIndex(x => x.PaymentInstallmentId).HasDatabaseName("IX_B2B_Payment_InstallmentId");
        builder.HasIndex(x => new { x.ProviderKey, x.ExternalTransactionId }).HasDatabaseName("IX_B2B_Payment_ProviderExternalId");
        builder.HasIndex(x => new { x.ProviderKey, x.BinNumber }).HasDatabaseName("IX_B2B_Payment_ProviderBin");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_B2B_Payment_Status");

        builder.HasOne(x => x.PaymentOrder)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.PaymentOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
