using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.B2B;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.B2B;

public sealed class CatalogVisibilityRuleConfiguration : BaseEntityConfiguration<CatalogVisibilityRule>
{
    protected override void ConfigureEntity(EntityTypeBuilder<CatalogVisibilityRule> builder)
    {
        builder.ToTable("RII_B2B_CATALOG_VISIBILITY_RULE");
        builder.Property(x => x.CustomerGroupCode).HasMaxLength(80);
        builder.Property(x => x.CategoryPath).HasMaxLength(500);
        builder.Property(x => x.RuleType).HasMaxLength(40).IsRequired();

        builder.HasIndex(x => new { x.CompanyId, x.CustomerId, x.CustomerGroupCode }).HasDatabaseName("IX_B2B_CatalogVisibility_Scope");
        builder.HasIndex(x => new { x.CatalogProductId, x.CategoryPath, x.RuleType }).HasDatabaseName("IX_B2B_CatalogVisibility_Target");
    }
}
