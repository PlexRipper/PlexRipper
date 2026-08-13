using TickerQ.Utilities.Entities;

namespace Reaparr.Data.Contracts;

public class JobCronTicker : CronTickerEntity
{
    public string JobKey { get; set; } = string.Empty;
    
    public JobTypes JobType { get; set; } = JobTypes.Unknown;
}