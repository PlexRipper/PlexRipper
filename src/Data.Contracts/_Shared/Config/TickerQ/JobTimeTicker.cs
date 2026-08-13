using TickerQ.Utilities.Entities;

namespace Reaparr.Data.Contracts;

public class JobTimeTicker : TimeTickerEntity<JobTimeTicker>
{
    public string JobKey { get; init; } = string.Empty;

    public JobTypes JobType { get; init; } = JobTypes.Unknown;

    public JobTimeTickerRequestProperties? RequestJson { get; init; }
}