namespace Reaparr.Data.Configurations;

public class TinkerQTimeTickerConfiguration : IEntityTypeConfiguration<JobTimeTicker>
{
    public void Configure(EntityTypeBuilder<JobTimeTicker> builder)
    {
        builder.ToTable("TinkerQ_JobTimeTickers");
        
        builder.Property(x => x.JobKey).HasColumnOrder(2).HasMaxLength(256);
        builder.HasIndex(x => x.JobKey);
        builder.Property(e => e.JobType)
            .HasColumnOrder(3)
            .HasMaxLength(100)
            .HasConversion(x => x.ToJobTypesString(), x => x.ToJobTypes())
            .IsUnicode(false)
            .HasDefaultValue(JobTypes.None)
            .HasSentinel(JobTypes.None);
        builder.HasIndex(x => x.JobType);

        builder.OwnsOne(x => x.RequestJson, request => request.ToJson());
    }
}
