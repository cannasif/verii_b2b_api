using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class PaymentProviderOperationConfiguration : IEntityTypeConfiguration<PaymentProviderOperation>
{
    public void Configure(EntityTypeBuilder<PaymentProviderOperation> builder)
    {
        builder.ToTable("RII_B2B_PAYMENT_PROVIDER_OPERATION");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProviderKey).HasMaxLength(40).IsRequired();
        builder.Property(x => x.OperationType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.ExternalOperationId).HasMaxLength(160);
        builder.Property(x => x.IdempotencyKey).HasMaxLength(120);
        builder.Property(x => x.Reason).HasMaxLength(1000);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2000);
        builder.Property(x => x.Amount).HasPrecision(18, 4);
        builder.Property(x => x.BranchCode).HasMaxLength(10).HasDefaultValue("0");
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.HasIndex(x => new { x.PaymentTransactionId, x.OperationType, x.Status }).HasDatabaseName("IX_B2B_PaymentProviderOperation_Transaction");
        builder.HasIndex(x => new { x.ProviderKey, x.ExternalOperationId }).HasDatabaseName("IX_B2B_PaymentProviderOperation_External");
        builder.HasIndex(x => x.IdempotencyKey).HasDatabaseName("IX_B2B_PaymentProviderOperation_Idempotency");
    }
}
