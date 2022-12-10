using Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Data.Common.Quartz;

namespace PlexRipper.Data.Configurations.Quartz;

public class QuartzLockEntityTypeConfiguration : IEntityTypeConfiguration<QuartzLock>
{

    public void Configure(EntityTypeBuilder<QuartzLock> builder)
    {
        builder.ToTable($"{QuartzDatabaseConfig.Prefix}LOCKS");

        builder.HasKey(x => new { x.SchedulerName, x.LockName });

        builder.Property(x => x.SchedulerName)
            .HasColumnName("sched_name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.LockName)
            .HasColumnName("lock_name")
            .HasColumnType("text")
            .IsRequired();
    }
}