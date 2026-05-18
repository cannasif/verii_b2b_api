using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Identity;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserConfiguration : BaseEntityConfiguration<User>
{
    protected override void ConfigureEntity(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("RII_USERS");

        builder.Property(x => x.Username).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Email).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PasswordHash).IsRequired().HasMaxLength(255);
        builder.Property(x => x.FirstName).HasMaxLength(50);
        builder.Property(x => x.LastName).HasMaxLength(50);
        builder.Property(x => x.PhoneNumber).HasMaxLength(20);
        builder.Property(x => x.RoleId).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.IsEmailConfirmed).HasDefaultValue(false);
        builder.Property(x => x.RefreshToken).HasMaxLength(500);

        builder.HasOne(x => x.RoleNavigation)
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Username).IsUnique().HasDatabaseName("IX_RII_USERS_Username");
        builder.HasIndex(x => x.Email).IsUnique().HasDatabaseName("IX_RII_USERS_Email");
    }
}
