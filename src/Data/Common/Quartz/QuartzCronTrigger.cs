namespace PlexRipper.Data.Common.Quartz;

public class QuartzCronTrigger
{
    public string SchedulerName { get; set; } = null!;
    public string TriggerName { get; set; } = null!;
    public string TriggerGroup { get; set; } = null!;
    public string CronExpression { get; set; } = null!;
    public string? TimeZoneId { get; set; }

    public QuartzTrigger Trigger { get; set; } = null!;
}