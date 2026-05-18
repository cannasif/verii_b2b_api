using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Identity;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserSessionConfiguration : BaseEntityConfiguration<UserSession>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("RII_USER_SESSION");

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.SessionId).IsRequired();
        builder.Property(x => x.Token).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.IpAddress).HasMaxLength(100);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.DeviceInfo).HasMaxLength(100);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => new { x.UserId, x.RevokedAt });
    }
}
