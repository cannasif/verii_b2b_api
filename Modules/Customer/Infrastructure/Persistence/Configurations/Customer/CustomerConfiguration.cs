using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CustomerEntity = Wms.Domain.Entities.Customer.Customer;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Customer;

public sealed class CustomerConfiguration : BaseEntityConfiguration<CustomerEntity>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CustomerEntity> builder)
    {
        builder.ToTable("RII_WMS_CUSTOMER");

        builder.Property(x => x.CustomerCode).HasMaxLength(50).IsRequired();
        builder.Property(x => x.CustomerName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TaxOffice).HasMaxLength(100);
        builder.Property(x => x.TaxNumber).HasMaxLength(50);
        builder.Property(x => x.TcknNumber).HasMaxLength(11);
        builder.Property(x => x.SalesRepCode).HasMaxLength(50);
        builder.Property(x => x.GroupCode).HasMaxLength(50);
        builder.Property(x => x.CreditLimit).HasColumnType("decimal(18,6)");
        builder.Property(x => x.Email).HasMaxLength(100);
        builder.Property(x => x.Website).HasMaxLength(100);
        builder.Property(x => x.Phone1).HasMaxLength(100);
        builder.Property(x => x.Phone2).HasMaxLength(100);
        builder.Property(x => x.Address).HasMaxLength(500);
        builder.Property(x => x.City).HasMaxLength(100);
        builder.Property(x => x.District).HasMaxLength(100);
        builder.Property(x => x.CountryCode).HasMaxLength(50);
        builder.Property(x => x.ErpIntegrationNumber).HasMaxLength(50);
        builder.Property(x => x.IsErpIntegrated).HasDefaultValue(true);

        builder.HasIndex(x => x.CustomerCode)
            .IsUnique()
            .HasDatabaseName("IX_Customer_CustomerCode")
            .HasFilter("[IsDeleted] = 0");
        builder.HasIndex(x => x.CustomerName).HasDatabaseName("IX_Customer_CustomerName");
        builder.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_Customer_IsDeleted");
    }
}
