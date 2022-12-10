using Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Data.Common.Quartz;

namespace PlexRipper.Data.Configurations.Quartz;

public class QuartzCalendarEntityTypeConfiguration : IEntityTypeConfiguration<QuartzCalendar>
{
    public void Configure(EntityTypeBuilder<QuartzCalendar> builder)
    {
        builder.ToTable($"{QuartzDatabaseConfig.Prefix}CALENDARS");

        builder.HasKey(x => new { x.SchedulerName, x.CalendarName });

        builder.Property(x => x.SchedulerName)
            .HasColumnName("sched_name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.CalendarName)
            .HasColumnName("calendar_name")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(x => x.Calendar)
            .HasColumnName("calendar")
            .HasColumnType("bytea")
            .IsRequired();
    }
}