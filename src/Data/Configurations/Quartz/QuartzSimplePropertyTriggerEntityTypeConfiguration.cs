using Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Data.Common.Quartz;

namespace PlexRipper.Data.Configurations.Quartz;

public class QuartzSimplePropertyTriggerEntityTypeConfiguration
    : IEntityTypeConfiguration<QuartzSimplePropertyTrigger>
{

    public void Configure(EntityTypeBuilder<QuartzSimplePropertyTrigger> builder)
    {
        builder.ToTable($"{QuartzDatabaseConfig.Prefix}SIMPROP_TRIGGERS");

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

        builder.Property(x => x.StringProperty1)
            .HasColumnName("str_prop_1")
            .HasColumnType("text");

        builder.Property(x => x.StringProperty2)
            .HasColumnName("str_prop_2")
            .HasColumnType("text");

        builder.Property(x => x.StringProperty3)
            .HasColumnName("str_prop_3")
            .HasColumnType("text");

        builder.Property(x => x.IntegerProperty1)
            .HasColumnName("int_prop_1")
            .HasColumnType("integer");

        builder.Property(x => x.IntegerProperty2)
            .HasColumnName("int_prop_2")
            .HasColumnType("integer");

        builder.Property(x => x.LongProperty1)
            .HasColumnName("long_prop_1")
            .HasColumnType("bigint");

        builder.Property(x => x.LongProperty2)
            .HasColumnName("long_prop_2")
            .HasColumnType("bigint");

        builder.Property(x => x.DecimalProperty1)
            .HasColumnName("dec_prop_1")
            .HasColumnType("numeric");

        builder.Property(x => x.DecimalProperty2)
            .HasColumnName("dec_prop_2")
            .HasColumnType("numeric");

        builder.Property(x => x.BooleanProperty1)
            .HasColumnName("bool_prop_1")
            .HasColumnType("bool");

        builder.Property(x => x.BooleanProperty2)
            .HasColumnName("bool_prop_2")
            .HasColumnType("bool");

        builder.Property(x => x.TimeZoneId)
            .HasColumnName("time_zone_id")
            .HasColumnType("text");

        builder.HasOne(x => x.Trigger)
            .WithMany(x => x.SimplePropertyTriggers)
            .HasForeignKey(x => new { x.SchedulerName, x.TriggerName, x.TriggerGroup })
            .OnDelete(DeleteBehavior.Cascade);
    }
}