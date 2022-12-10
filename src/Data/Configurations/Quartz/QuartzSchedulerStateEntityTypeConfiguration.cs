using Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Data.Common.Quartz;

namespace PlexRipper.Data.Configurations.Quartz;

public class QuartzSchedulerStateEntityTypeConfiguration : IEntityTypeConfiguration<QuartzSchedulerState>
{

    public void Configure(EntityTypeBuilder<QuartzSchedulerState> builder)
    {
        builder.ToTable($"{QuartzDatabaseConfig.Prefix}SCHEDULER_STATE");

        builder.HasKey(x => new { x.SchedulerName, x.InstanceName });

        builder.Property(x => x.SchedulerName)
            .HasColumnName("sched_name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.InstanceName)
            .HasColumnName("instance_name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.LastCheckInTime)
            .HasColumnName("last_checkin_time")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(x => x.CheckInInterval)
            .HasColumnName("checkin_interval")
            .HasColumnType("bigint")
            .IsRequired();
    }
}