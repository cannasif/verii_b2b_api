using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Identity;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserAuthorityConfiguration : BaseEntityConfiguration<UserAuthority>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserAuthority> builder)
    {
        builder.ToTable("RII_USER_AUTHORITY");

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(30);
    }
}
