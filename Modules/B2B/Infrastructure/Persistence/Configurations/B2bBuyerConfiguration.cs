using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class B2bBuyerConfiguration : BaseEntityConfiguration<B2bBuyer>
{
    protected override void ConfigureEntity(EntityTypeBuilder<B2bBuyer> builder)
    {
        builder.ToTable("RII_B2B_BUYER");
        builder.Property(x => x.Email).HasMaxLength(180).IsRequired();
        builder.Property(x => x.FullName).HasMaxLength(180).IsRequired();
        builder.Property(x => x.RoleCode).HasMaxLength(60).IsRequired();
        builder.Property(x => x.OrderLimit).HasPrecision(18, 4);

        builder.HasIndex(x => new { x.CompanyId, x.Email }).HasDatabaseName("IX_B2B_Buyer_CompanyEmail").IsUnique().HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.UserId).HasDatabaseName("IX_B2B_Buyer_UserId");
    }
}
