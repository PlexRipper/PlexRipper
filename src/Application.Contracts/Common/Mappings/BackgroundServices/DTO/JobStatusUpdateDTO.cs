namespace Application.Contracts;

public class JobStatusUpdateDTO
{
    public required string Id { get; set; }

    public required string JobName { get; set; }

    public required string JobGroup { get; set; }

    public required JobTypes JobType { get; set; }

    public required TimeSpan JobRuntime { get; set; }

    public required DateTime JobStartTime { get; set; }

    public required JobStatus Status { get; set; }

    public required string PrimaryKey { get; set; }

    public required string PrimaryKeyValue { get; set; }
}
