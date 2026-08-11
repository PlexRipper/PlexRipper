
using TickerQ.Utilities.Entities;

namespace Reaparr.Data.Contracts;

public class JobTimeTicker : TimeTickerEntity<JobTimeTicker>
{
    public string JobKey { get; set; } = string.Empty;

    public JobTypes JobType { get; set; } = JobTypes.Unknown;
}