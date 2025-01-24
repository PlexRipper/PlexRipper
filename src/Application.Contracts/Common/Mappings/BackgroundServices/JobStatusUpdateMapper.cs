namespace Application.Contracts;

public static class JobStatusUpdateMapper
{
    public static JobStatusUpdateDTO ToDTO(this JobStatusUpdate jobStatusUpdate) =>
        new()
        {
            Id = jobStatusUpdate.Id,
            JobStartTime = jobStatusUpdate.JobStartTime,
            Status = jobStatusUpdate.Status,
            JobType = jobStatusUpdate.JobType,
        };

    public static List<JobStatusUpdateDTO> ToDTO(this List<JobStatusUpdate> jobStatusUpdate) =>
        jobStatusUpdate.Select(ToDTO).ToList();

    public static JobStatusUpdateDTO<T> ToDTO<T>(this JobStatusUpdate<T> jobStatusUpdate)
        where T : class =>
        new()
        {
            Id = jobStatusUpdate.Id,
            JobStartTime = jobStatusUpdate.JobStartTime,
            Status = jobStatusUpdate.Status,
            JobType = jobStatusUpdate.JobType,
            Data = jobStatusUpdate.Data,
        };

    public static JobTypes ToJobType(string jobGroup) =>
        Enum.TryParse<JobTypes>(jobGroup, out var jobType)
            ? jobType
            : throw new Exception($"ToJobType => Unknown job type: {jobGroup}");
}
