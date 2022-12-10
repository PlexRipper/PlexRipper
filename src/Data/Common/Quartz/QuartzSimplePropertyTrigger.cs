namespace PlexRipper.Data.Common.Quartz;

public class QuartzSimplePropertyTrigger
{
    public string SchedulerName { get; set; } = null!;
    public string TriggerName { get; set; } = null!;
    public string TriggerGroup { get; set; } = null!;
    public string? StringProperty1 { get; set; }
    public string? StringProperty2 { get; set; }
    public string? StringProperty3 { get; set; }
    public int? IntegerProperty1 { get; set; }
    public int? IntegerProperty2 { get; set; }
    public long? LongProperty1 { get; set; }
    public long? LongProperty2 { get; set; }
    public decimal? DecimalProperty1 { get; set; }
    public decimal? DecimalProperty2 { get; set; }
    public bool? BooleanProperty1 { get; set; }
    public bool? BooleanProperty2 { get; set; }
    public string? TimeZoneId { get; set; }

    public QuartzTrigger Trigger { get; set; } = null!;
}