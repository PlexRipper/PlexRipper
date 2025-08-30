using System.Text.Json;
using Reaparr.Application.Contracts;

namespace Reaparr.Application.UnitTests;

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
        await rawResponse.HandleAsync(new GetAllBackgroundJobsEndpointRequest(), CancellationToken);
        var resultDTO = rawResponse.Response as ResultDTO<List<JobStatusUpdateDTO>>;

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
        await rawResponse.HandleAsync(new GetAllBackgroundJobsEndpointRequest(), CancellationToken);
        var resultDTO = rawResponse.Response as ResultDTO<List<JobStatusUpdateDTO>>;

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
        var downloadJobUpdatePayload = new DownloadJobUpdateDTO
        {
            Id = new DownloadTaskKey
            {
                Type = DownloadTaskType.TvShow,
                Id = Guid.NewGuid(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };
        var downloadJobUpdate = new JobStatusUpdate<string>(
            JobTypes.DownloadJob,
            JobStatus.Started,
            ToJsonString(downloadJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        var inspectPlexServerJobUpdatePayload = new InspectPlexServerJobUpdateDTO
        {
            PlexServerIds = new List<int> { 1, 2, 3, 4, 5 },
        };
        var inspectPlexServerJobUpdate = new JobStatusUpdate<string>(
            JobTypes.InspectPlexServerJob,
            JobStatus.Started,
            ToJsonString(inspectPlexServerJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        var syncServerMediaJobUpdatePayload = new SyncServerMediaJobUpdateDTO { PlexServerId = 1, ForceSync = true };
        var syncServerMediaJobUpdate = new JobStatusUpdate<string>(
            JobTypes.SyncServerMediaJob,
            JobStatus.Started,
            ToJsonString(syncServerMediaJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        var fileMergeJobUpdatePayload = new FileMergeJobUpdateDTO
        {
            DownloadTaskId = new DownloadTaskKey
            {
                Type = DownloadTaskType.TvShow,
                Id = Guid.NewGuid(),
                PlexServerId = 1,
                PlexLibraryId = 1,
            },
        };

        var fileMergeJobUpdate = new JobStatusUpdate<string>(
            JobTypes.FileMergeJob,
            JobStatus.Completed,
            ToJsonString(fileMergeJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        var checkAllConnectionsStatusJobUpdatePayload = new CheckAllConnectionStatusUpdateDTO
        {
            PlexServersWithConnectionIds = new Dictionary<int, List<int>>(),
        };
        var checkAllConnectionsStatusJobUpdate = new JobStatusUpdate<string>(
            JobTypes.CheckAllConnectionsStatusByPlexServerJob,
            JobStatus.Started,
            ToJsonString(checkAllConnectionsStatusJobUpdatePayload),
            Guid.NewGuid().ToString()
        );

        var list = new List<JobStatusUpdate<string>>
        {
            downloadJobUpdate,
            inspectPlexServerJobUpdate,
            syncServerMediaJobUpdate,
            fileMergeJobUpdate,
            checkAllConnectionsStatusJobUpdate,
        };

        mock.Mock<ISchedulerService>().Setup(x => x.GetRunningJobUpdates()).ReturnsAsync(list);

        // Act
        var rawResponse = SetupEndpointUnitTest<GetAllBackgroundJobsEndpoint>();
        await rawResponse.HandleAsync(new GetAllBackgroundJobsEndpointRequest(), CancellationToken);
        var resultDTO = rawResponse.Response as ResultDTO<List<JobStatusUpdateDTO>>;

        // Assert
        resultDTO.ShouldNotBeNull();
        resultDTO.IsSuccess.ShouldBeTrue();
        var responseValue = resultDTO.Value;
        responseValue.ShouldNotBeNull();
        responseValue.Count.ShouldBe(list.Count);

        // Validate each job type
        ValidateJobStatusUpdate(responseValue[0], downloadJobUpdate, downloadJobUpdatePayload);
        ValidateJobStatusUpdate(responseValue[1], inspectPlexServerJobUpdate, inspectPlexServerJobUpdatePayload);
        ValidateJobStatusUpdate(responseValue[2], syncServerMediaJobUpdate, syncServerMediaJobUpdatePayload);
        ValidateJobStatusUpdate(responseValue[3], fileMergeJobUpdate, fileMergeJobUpdatePayload);
        ValidateJobStatusUpdate(
            responseValue[4],
            checkAllConnectionsStatusJobUpdate,
            checkAllConnectionsStatusJobUpdatePayload
        );

        static void ValidateJobStatusUpdate<T>(
            JobStatusUpdateDTO actual,
            JobStatusUpdate<string> expected,
            T expectedPayload
        )
        {
            actual.ShouldNotBeNull();
            actual.Id.ShouldBe(expected.Id);
            actual.JobType.ShouldBe(expected.JobType);
            actual.Status.ShouldBe(expected.Status);
            actual.JobStartTime.ShouldBe(expected.JobStartTime);

            actual.JsonString.ShouldNotBeNullOrEmpty();
            var actualPayload = JsonSerializer.Deserialize<T>(actual.JsonString);
            actualPayload.ShouldBeEquivalentTo(expectedPayload);
        }
    }
}
