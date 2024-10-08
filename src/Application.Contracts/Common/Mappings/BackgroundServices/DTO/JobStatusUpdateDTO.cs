namespace Application.Contracts;

public record JobStatusUpdateDTO
{
    public required string Id { get; init; }

    public required DateTime JobStartTime { get; init; }

    public required JobTypes JobType { get; init; }

    public required JobStatus Status { get; init; }
}

public record JobStatusUpdateDTO<T> : JobStatusUpdateDTO
    where T : class
{
    public required T? Data { get; init; }
}