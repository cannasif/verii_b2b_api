using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class QuoteRequestConfiguration : BaseEntityConfiguration<QuoteRequest>
{
    protected override void ConfigureEntity(EntityTypeBuilder<QuoteRequest> builder)
    {
        builder.ToTable("RII_B2B_QUOTATION");
        builder.Property(x => x.QuoteNumber).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.OfferType).HasMaxLength(40);
        builder.Property(x => x.OfferNo).HasMaxLength(60);
        builder.Property(x => x.RevisionNo).HasMaxLength(30);
        builder.Property(x => x.DeliveryMethod).HasMaxLength(120);
        builder.Property(x => x.ErpProjectCode).HasMaxLength(50);
        builder.Property(x => x.GeneralDiscountRate).HasPrecision(18, 4);
        builder.Property(x => x.GeneralDiscountAmount).HasPrecision(18, 4);
        builder.Property(x => x.Total).HasPrecision(18, 4);
        builder.Property(x => x.EstimatedTotal).HasPrecision(18, 4);
        builder.Property(x => x.CustomerNote).HasMaxLength(1000);
        builder.Property(x => x.SalesNote).HasMaxLength(1000);

        builder.HasMany(x => x.Lines)
            .WithOne(x => x.QuoteRequest)
            .HasForeignKey(x => x.QuoteRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.QuoteNumber).HasDatabaseName("IX_B2B_Quotation_QuoteNumber").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.OfferNo).HasDatabaseName("IX_B2B_Quotation_OfferNo");
        builder.HasIndex(x => x.OfferDate).HasDatabaseName("IX_B2B_Quotation_OfferDate");
        builder.HasIndex(x => new { x.CustomerId, x.Status }).HasDatabaseName("IX_B2B_Quotation_CustomerStatus");
        builder.HasIndex(x => new { x.CustomerId, x.BuyerId, x.Status }).HasDatabaseName("IX_B2B_Quotation_CustomerBuyerStatus");
    }
}
