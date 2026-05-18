using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class B2bCompanyAddressConfiguration : BaseEntityConfiguration<B2bCompanyAddress>
{
    protected override void ConfigureEntity(EntityTypeBuilder<B2bCompanyAddress> builder)
    {
        builder.ToTable("RII_B2B_COMPANY_ADDRESS");
        builder.Property(x => x.AddressType).HasMaxLength(40).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(160).IsRequired();
        builder.Property(x => x.AddressLine).HasMaxLength(600).IsRequired();
        builder.Property(x => x.District).HasMaxLength(120);
        builder.Property(x => x.City).HasMaxLength(120);
        builder.Property(x => x.CountryCode).HasMaxLength(20);
        builder.Property(x => x.PostalCode).HasMaxLength(30);

        builder.HasIndex(x => new { x.CompanyId, x.AddressType, x.IsDefault }).HasDatabaseName("IX_B2B_CompanyAddress_Default");
    }
}
