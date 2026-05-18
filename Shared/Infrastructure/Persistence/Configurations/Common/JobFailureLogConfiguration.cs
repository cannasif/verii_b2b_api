using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Wms.Domain.Entities.Common;

namespace Wms.Infrastructure.Persistence.Configurations.Common;

public sealed class JobFailureLogConfiguration : BaseEntityConfiguration<JobFailureLog>
{
    protected override void ConfigureEntity(EntityTypeBuilder<JobFailureLog> builder)
    {
        builder.ToTable("RII_JOB_FAILURE_LOG");
        builder.Property(x => x.TraceId).HasMaxLength(64);
        builder.Property(x => x.JobId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.JobName).HasMaxLength(500).IsRequired();
        builder.Property(x => x.FailedAt).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(2000);
        builder.Property(x => x.ExceptionType).HasMaxLength(500);
        builder.Property(x => x.ExceptionMessage).HasMaxLength(4000);
        builder.Property(x => x.StackTrace).HasMaxLength(4000);
        builder.Property(x => x.Queue).HasMaxLength(100);

        builder.HasIndex(x => x.TraceId).HasDatabaseName("IX_JobFailureLog_TraceId");
    }
}
