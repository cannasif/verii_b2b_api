using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class PaymentProviderInquiryLogConfiguration : BaseEntityConfiguration<PaymentProviderInquiryLog>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentProviderInquiryLog> builder)
    {
        builder.ToTable("RII_B2B_PAYMENT_PROVIDER_INQUIRY_LOG");
        builder.Property(x => x.ProviderKey).HasMaxLength(40).IsRequired();
        builder.Property(x => x.InquiryType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.BinNumber).HasMaxLength(8);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.ConversationId).HasMaxLength(120);
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.Property(x => x.RequestJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.ResponseJson).HasColumnType("nvarchar(max)");
        builder.Property(x => x.Amount).HasPrecision(18, 4);

        builder.HasIndex(x => new { x.ProviderKey, x.InquiryType, x.RequestedDate }).HasDatabaseName("IX_B2B_ProviderInquiry_ProviderTypeDate");
        builder.HasIndex(x => x.BinNumber).HasDatabaseName("IX_B2B_ProviderInquiry_BinNumber");
        builder.HasIndex(x => x.Status).HasDatabaseName("IX_B2B_ProviderInquiry_Status");
    }
}
