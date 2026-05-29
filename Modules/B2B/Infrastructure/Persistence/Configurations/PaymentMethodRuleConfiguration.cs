using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class PaymentMethodRuleConfiguration : BaseEntityConfiguration<PaymentMethodRule>
{
    protected override void ConfigureEntity(EntityTypeBuilder<PaymentMethodRule> builder)
    {
        builder.ToTable("RII_B2B_PAYMENT_METHOD_RULE");
        builder.Property(x => x.CustomerGroupCode).HasMaxLength(80);
        builder.Property(x => x.ProviderKey).HasMaxLength(40).IsRequired();
        builder.Property(x => x.PaymentMethod).HasMaxLength(80).IsRequired();
        builder.Property(x => x.RuleType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.MinAmount).HasPrecision(18, 4);
        builder.Property(x => x.MaxAmount).HasPrecision(18, 4);
        builder.Property(x => x.Notes).HasMaxLength(1000);

        builder.HasIndex(x => new { x.CompanyId, x.CustomerId, x.CustomerGroupCode }).HasDatabaseName("IX_B2B_PaymentMethodRule_Scope");
        builder.HasIndex(x => new { x.ProviderKey, x.PaymentMethod, x.RuleType }).HasDatabaseName("IX_B2B_PaymentMethodRule_Method");
        builder.HasIndex(x => new { x.IsActive, x.ValidFrom, x.ValidTo }).HasDatabaseName("IX_B2B_PaymentMethodRule_ActiveDates");
    }
}
