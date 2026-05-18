using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class QuoteRequestLineConfiguration : BaseEntityConfiguration<QuoteRequestLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<QuoteRequestLine> builder)
    {
        builder.ToTable("RII_B2B_QUOTATION_LINE");
        builder.Property(x => x.QuoteRequestId).HasColumnName("QuotationId");
        builder.Property(x => x.RequestedSku).HasMaxLength(120);
        builder.Property(x => x.RequestedName).HasMaxLength(250);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.TargetUnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.ApprovedUnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.DiscountRate1).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount1).HasPrecision(18, 4);
        builder.Property(x => x.DiscountRate2).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount2).HasPrecision(18, 4);
        builder.Property(x => x.DiscountRate3).HasPrecision(18, 4);
        builder.Property(x => x.DiscountAmount3).HasPrecision(18, 4);
        builder.Property(x => x.VatRate).HasPrecision(18, 4);
        builder.Property(x => x.VatAmount).HasPrecision(18, 4);
        builder.Property(x => x.LineTotal).HasPrecision(18, 4);
        builder.Property(x => x.LineGrandTotal).HasPrecision(18, 4);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.Description1).HasMaxLength(500);
        builder.Property(x => x.Description2).HasMaxLength(500);
        builder.Property(x => x.Description3).HasMaxLength(500);
        builder.Property(x => x.RelatedProductKey).HasMaxLength(120);
        builder.Property(x => x.ErpProjectCode).HasMaxLength(50);

        builder.HasIndex(x => x.QuoteRequestId).HasDatabaseName("IX_B2B_QuotationLine_QuotationId");
    }
}
