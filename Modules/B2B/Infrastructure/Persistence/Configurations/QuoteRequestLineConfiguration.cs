using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class QuoteRequestLineConfiguration : BaseEntityConfiguration<QuoteRequestLine>
{
    protected override void ConfigureEntity(EntityTypeBuilder<QuoteRequestLine> builder)
    {
        builder.ToTable("RII_B2B_QUOTE_REQUEST_LINE");
        builder.Property(x => x.RequestedSku).HasMaxLength(120);
        builder.Property(x => x.RequestedName).HasMaxLength(250);
        builder.Property(x => x.Quantity).HasPrecision(18, 4);
        builder.Property(x => x.TargetUnitPrice).HasPrecision(18, 4);
        builder.Property(x => x.ApprovedUnitPrice).HasPrecision(18, 4);

        builder.HasIndex(x => x.QuoteRequestId).HasDatabaseName("IX_B2B_QuoteLine_QuoteId");
    }
}
