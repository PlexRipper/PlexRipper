using Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Data.Common.Quartz;

namespace PlexRipper.Data.Configurations.Quartz;

public class QuartzTriggerEntityTypeConfiguration : IEntityTypeConfiguration<QuartzTrigger>
{
    public void Configure(EntityTypeBuilder<QuartzTrigger> builder)
    {
        builder.ToTable($"{QuartzDatabaseConfig.Prefix}TRIGGERS");

        builder.HasKey(x => new { x.SchedulerName, x.TriggerName, x.TriggerGroup });

        builder.Property(x => x.SchedulerName)
            .HasColumnName("sched_name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.TriggerName)
            .HasColumnName("trigger_name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.TriggerGroup)
            .HasColumnName("trigger_group")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.JobName)
            .HasColumnName("job_name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.JobGroup)
            .HasColumnName("job_group")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasColumnType("text");

        builder.Property(x => x.NextFireTime)
            .HasColumnName("next_fire_time")
            .HasColumnType("bigint");

        builder.Property(x => x.PreviousFireTime)
            .HasColumnName("prev_fire_time")
            .HasColumnType("bigint");

        builder.Property(x => x.Priority)
            .HasColumnName("priority")
            .HasColumnType("integer");

        builder.Property(x => x.TriggerState)
            .HasColumnName("trigger_state")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.TriggerType)
            .HasColumnName("trigger_type")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.StartTime)
            .HasColumnName("start_time")
            .HasColumnType("bigint")
            .IsRequired();

        builder.Property(x => x.EndTime)
            .HasColumnName("end_time")
            .HasColumnType("bigint");

        builder.Property(x => x.CalendarName)
            .HasColumnName("calendar_name")
            .HasColumnType("text");

        builder.Property(x => x.MisfireInstruction)
            .HasColumnName("misfire_instr")
            .HasColumnType("smallint");

        builder.Property(x => x.JobData)
            .HasColumnName("job_data")
            .HasColumnType("bytea");

        builder.HasOne(x => x.JobDetail)
            .WithMany(x => x.Triggers)
            .HasForeignKey(x => new { x.SchedulerName, x.JobName, x.JobGroup })
            .IsRequired();

        builder.HasIndex(x => x.NextFireTime)
            .HasDatabaseName($"idx_{QuartzDatabaseConfig.Prefix}t_next_fire_time");

        builder.HasIndex(x => x.TriggerState)
            .HasDatabaseName($"idx_{QuartzDatabaseConfig.Prefix}t_state");

        builder.HasIndex(x => new { x.NextFireTime, x.TriggerState })
            .HasDatabaseName($"idx_{QuartzDatabaseConfig.Prefix}t_nft_st");
    }
}