namespace Application.Contracts;

public record JobStatusUpdateDTO
{
    public string Id { get; init; }

    public DateTime JobStartTime { get; init; }

    public JobTypes JobType { get; init; }

    public JobStatus Status { get; init; }
}

public record JobStatusUpdateDTO<T> : JobStatusUpdateDTO
    where T : class
{
    public T Data { get; init; }
}
