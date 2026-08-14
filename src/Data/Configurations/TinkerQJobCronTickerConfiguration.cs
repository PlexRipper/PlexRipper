namespace Reaparr.Data.Configurations;

public class TinkerQJobCronTickerConfiguration : IEntityTypeConfiguration<JobCronTicker>
{
    public void Configure(EntityTypeBuilder<JobCronTicker> builder)
    {
        builder.ToTable("TinkerQ_JobCronTickers");

        builder.Property(x => x.JobKey).HasColumnOrder(2).HasMaxLength(256);
        builder.HasIndex(x => x.JobKey);

        builder
            .Property(e => e.JobType)
            .HasColumnOrder(3)
            .HasMaxLength(100)
            .HasConversion(x => x.ToJobTypesString(), x => x.ToJobTypes())
            .IsUnicode(false)
            .HasDefaultValue(JobTypes.None)
            .HasSentinel(JobTypes.None);
        builder.HasIndex(x => x.JobType);
    }
}
