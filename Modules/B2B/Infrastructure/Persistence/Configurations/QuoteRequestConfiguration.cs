using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class QuoteRequestConfiguration : BaseEntityConfiguration<QuoteRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<QuoteRequest> builder)
    {
        builder.ToTable("RII_B2B_QUOTE_REQUEST");
        builder.Property(x => x.QuoteNumber).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.EstimatedTotal).HasPrecision(18, 4);
        builder.Property(x => x.CustomerNote).HasMaxLength(1000);
        builder.Property(x => x.SalesNote).HasMaxLength(1000);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.QuoteRequest)
            .HasForeignKey(x => x.QuoteRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.QuoteNumber).HasDatabaseName("IX_B2B_Quote_QuoteNumber").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.CustomerId, x.Status }).HasDatabaseName("IX_B2B_Quote_CustomerStatus");
    }
}
