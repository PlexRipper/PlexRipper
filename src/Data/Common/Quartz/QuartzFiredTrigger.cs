namespace PlexRipper.Data.Common.Quartz;

public class QuartzFiredTrigger
{
    public string SchedulerName { get; set; } = null!;
    public string EntryId { get; set; } = null!;
    public string TriggerName { get; set; } = null!;
    public string TriggerGroup { get; set; } = null!;
    public string InstanceName { get; set; } = null!;
    public long FiredTime { get; set; }
    public long ScheduledTime { get; set; }
    public int Priority { get; set; }
    public string State { get; set; } = null!;
    public string? JobName { get; set; }
    public string? JobGroup { get; set; }
    public bool IsNonConcurrent { get; set; }
    public bool? RequestsRecovery { get; set; }
}