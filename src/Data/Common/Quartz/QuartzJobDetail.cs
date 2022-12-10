namespace PlexRipper.Data.Common.Quartz;

public class QuartzJobDetail
{
    public string SchedulerName { get; set; } = null!;
    public string JobName { get; set; } = null!;
    public string JobGroup { get; set; } = null!;
    public string? Description { get; set; }
    public string JobClassName { get; set; } = null!;
    public bool IsDurable { get; set; }
    public bool IsNonConcurrent { get; set; }
    public bool IsUpdateData { get; set; }
    public bool RequestsRecovery { get; set; }
    public byte[]? JobData { get; set; } = null!;

    public ICollection<QuartzTrigger> Triggers { get; set; } = null!;
}