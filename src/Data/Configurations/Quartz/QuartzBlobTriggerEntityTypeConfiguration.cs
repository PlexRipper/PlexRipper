using Environment;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlexRipper.Data.Common.Quartz;

namespace PlexRipper.Data.Configurations.Quartz;

public class QuartzBlobTriggerEntityTypeConfiguration : IEntityTypeConfiguration<QuartzBlobTrigger>
{
    public void Configure(EntityTypeBuilder<QuartzBlobTrigger> builder)
    {
        builder.ToTable($"{QuartzDatabaseConfig.Prefix}BLOB_TRIGGERS", "quartz");

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

        builder.Property(x => x.BlobData)
            .HasColumnName("blob_data")
            .HasColumnType("bytea");

        builder.HasOne(x => x.Trigger)
            .WithMany(x => x.BlobTriggers)
            .HasForeignKey(x => new { x.SchedulerName, x.TriggerName, x.TriggerGroup })
            .OnDelete(DeleteBehavior.Cascade);
    }
}