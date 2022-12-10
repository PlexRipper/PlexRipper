using Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Data.Common.Quartz;

namespace PlexRipper.Data.Configurations.Quartz;

public class QuartzPausedTriggerGroupEntityTypeConfiguration : IEntityTypeConfiguration<QuartzPausedTriggerGroup>
{
    public void Configure(EntityTypeBuilder<QuartzPausedTriggerGroup> builder)
    {
        builder.ToTable($"{QuartzDatabaseConfig.Prefix}PAUSED_TRIGGER_GRPS");

        builder.HasKey(x => new { x.SchedulerName, x.TriggerGroup });

        builder.Property(x => x.SchedulerName)
            .HasColumnName("sched_name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.TriggerGroup)
            .HasColumnName("trigger_group")
            .HasColumnType("text")
            .IsRequired();
    }
}