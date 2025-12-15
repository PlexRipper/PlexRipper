namespace Reaparr.Application.Contracts;

public record JobStatusUpdate
{
    /// <summary>
    /// Get the unique Id that identifies this particular firing instance of the
    /// trigger that triggered this job execution.  It is unique to this
    /// JobExecutionContext instance as well.
    /// </summary>
    ///  <returns>the unique fire instance id</returns>
    public string Id { get; set; }

    public DateTime JobStartTime { get; }

    public JobTypes JobType { get; }

    public JobStatus Status { get; set; }

    public JobStatusUpdate(JobTypes jobType, JobStatus status, string id = "", DateTime jobStartTime = default)
    {
        Id = id != string.Empty ? id : Guid.NewGuid().ToString();
        JobStartTime = jobStartTime != default ? jobStartTime : DateTime.UtcNow;
        JobType = jobType;
        Status = status;
    }
}

public record JobStatusUpdate<T> : JobStatusUpdate
    where T : class
{
    public T Data { get; }

    public JobStatusUpdate(JobStatusUpdate update, T data)
        : base(update.JobType, update.Status, update.Id, update.JobStartTime)
    {
        Data = data;
    }

    public JobStatusUpdate(JobTypes jobType, JobStatus status, T data, string id = "", DateTime jobStartTime = default)
        : base(jobType, status, id, jobStartTime)
    {
        Data = data;
    }
}
