using System.Text.Json;
using Application.Contracts;

namespace PlexRipper.Application.UnitTests;

public class GetAllBackgroundJobsEndpointUnitTests : BaseUnitTest<GetAllBackgroundJobsEndpoint>
{
    public GetAllBackgroundJobsEndpointUnitTests(ITestOutputHelper output)
        : base(output) { }

    private string ToJsonString<T>(T value) =>
        value is null ? string.Empty : JsonSerializer.Serialize(value, DefaultJsonSerializerOptions.ConfigStandard);

    [Fact]
    public async Task ShouldReturnEmptyList_WhenNoBackgroundJobIsRunning()
    {
        // Arrange
        mock.Mock<ISchedulerService>().Setup(x => x.GetRunningJobUpdates()).ReturnsAsync([]);

        // Act
        var rawResponse = SetupEndpointUnitTest<GetAllBackgroundJobsEndpoint>();
        await rawResponse.HandleAsync(CancellationToken.None);
        var resultDTO = rawResponse.Response as ResultDTO<List<JobStatusUpdateDTO<string>>>;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeTrue();
        var responseValue = resultDTO.Value;
        responseValue.ShouldNotBeNull();
        responseValue.ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldReturnJobStatusUpdate_WhenABackgroundJobIsRunning()
    {
        // Arrange
        var jobUpdate = new JobStatusUpdate<string>(JobTypes.DownloadJob, JobStatus.Started, Guid.NewGuid().ToString());
        var list = new List<JobStatusUpdate<string>> { jobUpdate };
        mock.Mock<ISchedulerService>().Setup(x => x.GetRunningJobUpdates()).ReturnsAsync(list);

        // Act
        var rawResponse = SetupEndpointUnitTest<GetAllBackgroundJobsEndpoint>();
        await rawResponse.HandleAsync(CancellationToken.None);
        var resultDTO = rawResponse.Response as ResultDTO<List<JobStatusUpdateDTO<string>>>;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeTrue();
        var responseValue = resultDTO.Value;
        responseValue.ShouldNotBeNull();
        responseValue.Count.ShouldBe(1);

        foreach (var value in responseValue)
        {
            value.Id.ShouldBe(jobUpdate.Id);
            value.JobType.ShouldBe(jobUpdate.JobType);
            value.Status.ShouldBe(jobUpdate.Status);
            value.JobStartTime.ShouldBe(jobUpdate.JobStartTime);
        }
    }

    [Fact]
    public async Task ShouldReturnTypedJobStatusUpdate_WhenABackgroundJobIsRunning()
    {
        // Arrange
        var downloadJobUpdatePayload = new DownloadTaskKey
        {
            Type = DownloadTaskType.TvShow,
            Id = Guid.NewGuid(),
            PlexServerId = 1,
            PlexLibraryId = 1,
        };
        var downloadJobUpdate = new JobStatusUpdate<string>(
            JobTypes.DownloadJob,
            JobStatus.Started,
            ToJsonString(downloadJobUpdatePayload),
            Guid.NewGuid().ToString()
        );
        List<int> inspectPlexServerJobUpdatePayload = [1, 2, 3, 4, 5];
        var inspectPlexServerJobUpdate = new JobStatusUpdate<string>(
            JobTypes.InspectPlexServerJob,
            JobStatus.Started,
            ToJsonString(inspectPlexServerJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        var list = new List<JobStatusUpdate<string>> { downloadJobUpdate, inspectPlexServerJobUpdate };
        mock.Mock<ISchedulerService>().Setup(x => x.GetRunningJobUpdates()).ReturnsAsync(list);

        // Act
        var rawResponse = SetupEndpointUnitTest<GetAllBackgroundJobsEndpoint>();
        await rawResponse.HandleAsync(CancellationToken.None);
        var resultDTO = rawResponse.Response as ResultDTO<List<JobStatusUpdateDTO<string>>>;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeTrue();
        var responseValue = resultDTO.Value;
        responseValue.ShouldNotBeNull();
        responseValue.Count.ShouldBe(list.Count);

        var downloadJobUpdateDTO = responseValue[0];

        downloadJobUpdateDTO.ShouldNotBeNull();
        downloadJobUpdateDTO.Id.ShouldBe(downloadJobUpdate.Id);
        downloadJobUpdateDTO.JobType.ShouldBe(downloadJobUpdate.JobType);
        downloadJobUpdateDTO.Status.ShouldBe(downloadJobUpdate.Status);
        downloadJobUpdateDTO.JobStartTime.ShouldBe(downloadJobUpdate.JobStartTime);
        downloadJobUpdateDTO.Data.ShouldNotBeNull();
        var downloadJobUpdateDTOValue = JsonSerializer.Deserialize<DownloadTaskKey>(downloadJobUpdateDTO.Data);
        downloadJobUpdateDTOValue.ShouldBe(downloadJobUpdatePayload);

        var inspectPlexServerJobUpdateDTO = responseValue[1];

        inspectPlexServerJobUpdateDTO.ShouldNotBeNull();
        inspectPlexServerJobUpdateDTO.Id.ShouldBe(inspectPlexServerJobUpdate.Id);
        inspectPlexServerJobUpdateDTO.JobType.ShouldBe(inspectPlexServerJobUpdate.JobType);
        inspectPlexServerJobUpdateDTO.Status.ShouldBe(inspectPlexServerJobUpdate.Status);
        inspectPlexServerJobUpdateDTO.JobStartTime.ShouldBe(inspectPlexServerJobUpdate.JobStartTime);
        inspectPlexServerJobUpdateDTO.Data.ShouldNotBeNull();
        var inspectPlexServerJobUpdateDTOValue = JsonSerializer.Deserialize<List<int>>(
            inspectPlexServerJobUpdateDTO.Data
        );

        inspectPlexServerJobUpdateDTOValue.ShouldBe(inspectPlexServerJobUpdatePayload);
    }
}
