using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class JobStatusUpdateMapper
{
    #region ToDTO

    public static JobStatusUpdateDTO ToDTO(this JobStatusUpdate jobStatusUpdate)
    {
        var dto = jobStatusUpdate.ToDTOMapper();
        dto.JobType = ToJobType(jobStatusUpdate.JobGroup);
        return dto;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapperIgnoreTarget(nameof(JobStatusUpdateDTO.JobType))]
    private static partial JobStatusUpdateDTO ToDTOMapper(this JobStatusUpdate jobStatusUpdate);

    #endregion

    private static JobTypes ToJobType(string jobGroup) => Enum.TryParse<JobTypes>(jobGroup, out var jobType) ? jobType : JobTypes.Unknown;
}