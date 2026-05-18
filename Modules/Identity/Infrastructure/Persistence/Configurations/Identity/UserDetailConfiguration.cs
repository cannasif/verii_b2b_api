using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Identity;
using Wms.Infrastructure.Persistence.Configurations.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Identity;

public sealed class UserDetailConfiguration : BaseEntityConfiguration<UserDetail>
{
    protected override void ConfigureEntity(EntityTypeBuilder<UserDetail> builder)
    {
        builder.ToTable("RII_USER_DETAIL");
        builder.Property(x => x.ProfilePictureUrl).HasMaxLength(500);
        builder.Property(x => x.Height).HasColumnType("decimal(5,2)");
        builder.Property(x => x.Weight).HasColumnType("decimal(5,2)");
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Gender).HasConversion<byte?>().IsRequired(false);
        builder.HasIndex(x => x.UserId).IsUnique().HasDatabaseName("IX_RII_USER_DETAIL_UserId");
        builder.HasOne(x => x.User)
            .WithOne(x => x.UserDetail)
            .HasForeignKey<UserDetail>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
