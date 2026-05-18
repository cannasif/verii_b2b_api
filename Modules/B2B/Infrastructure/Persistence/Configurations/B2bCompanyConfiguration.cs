using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class B2bCompanyConfiguration : BaseEntityConfiguration<B2bCompany>
{
    protected override void ConfigureEntity(EntityTypeBuilder<B2bCompany> builder)
    {
        builder.ToTable("RII_B2B_COMPANY");
        builder.Property(x => x.CompanyCode).HasMaxLength(80).IsRequired();
        builder.Property(x => x.CompanyName).HasMaxLength(220).IsRequired();
        builder.Property(x => x.CustomerGroupCode).HasMaxLength(80);
        builder.Property(x => x.CreditLimit).HasPrecision(18, 4);
        builder.Property(x => x.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(40).IsRequired();

        builder.HasMany(x => x.Buyers).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(x => x.Addresses).WithOne(x => x.Company).HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CompanyCode).HasDatabaseName("IX_B2B_Company_Code").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => new { x.CustomerId, x.ParentCompanyId }).HasDatabaseName("IX_B2B_Company_CustomerHierarchy");
    }
}
