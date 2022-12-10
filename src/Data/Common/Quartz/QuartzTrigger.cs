namespace PlexRipper.Data.Common.Quartz;

public class QuartzTrigger
{
    public string SchedulerName { get; set; } = null!;
    public string TriggerName { get; set; } = null!;
    public string TriggerGroup { get; set; } = null!;
    public string JobName { get; set; } = null!;
    public string JobGroup { get; set; } = null!;
    public string? Description { get; set; } = null!;
    public long? NextFireTime { get; set; }
    public long? PreviousFireTime { get; set; }
    public int? Priority { get; set; }
    public string TriggerState { get; set; } = null!;
    public string TriggerType { get; set; } = null!;
    public long StartTime { get; set; }
    public long? EndTime { get; set; }
    public string? CalendarName { get; set; } = null!;
    public short? MisfireInstruction { get; set; }
    public byte[]? JobData { get; set; }

    public QuartzJobDetail JobDetail { get; set; } = null!;

    public ICollection<QuartzSimpleTrigger> SimpleTriggers { get; set; } = null!;
    public ICollection<QuartzSimplePropertyTrigger> SimplePropertyTriggers { get; set; } = null!;
    public ICollection<QuartzCronTrigger> CronTriggers { get; set; } = null!;
    public ICollection<QuartzBlobTrigger> BlobTriggers { get; set; } = null!;
}