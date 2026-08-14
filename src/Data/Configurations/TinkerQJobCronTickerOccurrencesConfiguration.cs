using TickerQ.Utilities.Entities;

namespace Reaparr.Data.Configurations;

public class TinkerQJobCronTickerOccurrencesConfiguration
    : IEntityTypeConfiguration<CronTickerOccurrenceEntity<JobCronTicker>>
{
    public void Configure(EntityTypeBuilder<CronTickerOccurrenceEntity<JobCronTicker>> builder)
    {
        builder.ToTable("TinkerQ_JobCronTickerOccurrences");
    }
}
