using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Pragmatik dikey slice için ortak audit ve soft-delete kolonlarını tanımlar.
/// `_old` yapıdaki kullanıcı navigation bağımlılıklarını bilerek taşımaz.
/// </summary>
public abstract class BaseEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.BranchCode)
            .HasMaxLength(10)
            .IsRequired()
            .HasDefaultValue("0");

        builder.Property(x => x.CreatedDate)
            .IsRequired(false);

        builder.Property(x => x.UpdatedDate)
            .IsRequired(false);

        builder.Property(x => x.DeletedDate)
            .IsRequired(false);

        builder.Property(x => x.CreatedBy)
            .IsRequired(false);

        builder.Property(x => x.UpdatedBy)
            .IsRequired(false);

        builder.Property(x => x.DeletedBy)
            .IsRequired(false);

        builder.Property(x => x.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        ConfigureEntity(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> builder);
}
