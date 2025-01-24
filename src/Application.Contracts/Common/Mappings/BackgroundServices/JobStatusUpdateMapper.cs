using System.Text.Json;
using PlexRipper.Domain;

namespace Application.Contracts;

public static class JobStatusUpdateMapper
{
    public static JobStatusUpdateDTO ToDTO<T>(this JobStatusUpdate<T> jobStatusUpdate)
        where T : class =>
        new()
        {
            Id = jobStatusUpdate.Id,
            JobStartTime = jobStatusUpdate.JobStartTime,
            Status = jobStatusUpdate.Status,
            JobType = jobStatusUpdate.JobType,
            JsonString =
                typeof(T) == typeof(string)
                    ? jobStatusUpdate.Data as string ?? string.Empty
                    : JsonSerializer.Serialize(jobStatusUpdate.Data, DefaultJsonSerializerOptions.ConfigStandard),
        };

    public static List<JobStatusUpdateDTO> ToDTO<T>(this List<JobStatusUpdate<T>> jobStatusUpdate)
        where T : class => jobStatusUpdate.Select(ToDTO).ToList();

    public static JobTypes ToJobType(string jobGroup) =>
        Enum.TryParse<JobTypes>(jobGroup, out var jobType)
            ? jobType
            : throw new Exception($"ToJobType => Unknown job type: {jobGroup}");
}
