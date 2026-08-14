using System.Reflection;
using TickerQ.Utilities.Base;
using TickerQ.Utilities.Interfaces;
using TickerQ.Utilities.Interfaces.Managers;
using TickerQ.Utilities.Models;

namespace Reaparr.Application.UnitTests;

public class BackgroundJobSchedulerUnitTests
{
    [Test]
    public async Task ShouldNotFlowCallerExecutionContext_WhenExecutingJobImmediately()
    {
        // Arrange
        var ambientValue = new AsyncLocal<string?> { Value = "request-scope" };
        string? capturedValue = null;
        var tickerManager = new Mock<ITimeTickerManager<JobTimeTicker>>();
        tickerManager
            .Setup(x => x.AddAsync(It.IsAny<JobTimeTicker>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                // Match TickerQ's immediate-dispatch path, which queues work after awaiting persistence.
                await Task.Yield();

                using var signal = new ManualResetEventSlim();
                var thread = new Thread(() =>
                {
                    capturedValue = ambientValue.Value;
                    signal.Set();
                });
                thread.Start();
                signal.Wait();
                thread.Join();

                return CreateSuccessfulTickerResult();
            });

        var scheduler = new BackgroundJobScheduler(
            Mock.Of<ILogger>(),
            Mock.Of<IReaparrDbContextFactory>(),
            tickerManager.Object,
            Mock.Of<ICronTickerManager<JobCronTicker>>(),
            Mock.Of<ITickerQHostScheduler>(),
            Mock.Of<IAppRuntimeInfo>(),
            Mock.Of<ICommandExecutor>()
        );

        // Act
        await scheduler.ExecuteJob<TestTickerFunction, TestTickerPayload>(
            new JobKey("execution-context-test", JobTypes.LibrarySyncJob),
            new TestTickerPayload(),
            CancellationToken.None
        );

        // Assert
        capturedValue.ShouldBeNull();
        ambientValue.Value.ShouldBe("request-scope");
    }

    [Test]
    public async Task ShouldAddScheduledJobsInSingleBatch()
    {
        // Arrange
        var tickerManager = new Mock<ITimeTickerManager<JobTimeTicker>>();
        tickerManager
            .Setup(x => x.AddBatchAsync(It.IsAny<List<JobTimeTicker>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateSuccessfulTickerBatchResult());

        var scheduler = new BackgroundJobScheduler(
            Mock.Of<ILogger>(),
            Mock.Of<IReaparrDbContextFactory>(),
            tickerManager.Object,
            Mock.Of<ICronTickerManager<JobCronTicker>>(),
            Mock.Of<ITickerQHostScheduler>(),
            Mock.Of<IAppRuntimeInfo>(),
            Mock.Of<ICommandExecutor>()
        );
        var jobs = new List<(JobKey JobKey, TestTickerPayload Request)>
        {
            (new JobKey("batch-1", JobTypes.LibraryComparisonJob), new TestTickerPayload()),
            (new JobKey("batch-2", JobTypes.LibraryComparisonJob), new TestTickerPayload()),
        };

        // Act
        await scheduler.ScheduleJobs<TestTickerFunction, TestTickerPayload>(jobs, DateTime.UtcNow.AddMinutes(2));

        // Assert
        tickerManager.Verify(
            x =>
                x.AddBatchAsync(
                    It.Is<List<JobTimeTicker>>(tickers => tickers.Count == 2),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
        tickerManager.Verify(x => x.AddAsync(It.IsAny<JobTimeTicker>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldFailWithoutQueueing_WhenRequestIsNull()
    {
        // Arrange
        var tickerManager = new Mock<ITimeTickerManager<JobTimeTicker>>();
        var scheduler = CreateScheduler(tickerManager.Object);

        // Act
        var result = await scheduler.ExecuteJob<TestTickerFunction, TestTickerPayload>(
            new JobKey("null-request-test", JobTypes.LibrarySyncJob),
            null!,
            CancellationToken.None
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Single().Message.ShouldBe("Background job null-request-test cannot be queued without a request");
        tickerManager.Verify(x => x.AddAsync(It.IsAny<JobTimeTicker>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldFailWithoutQueueing_WhenSerializedRequestIsEmpty()
    {
        // Arrange
        var tickerManager = new Mock<ITimeTickerManager<JobTimeTicker>>();
        var scheduler = CreateScheduler(tickerManager.Object);

        // Act
        var result = await scheduler.ScheduleJob<TestTickerFunction, EmptyTickerPayload>(
            new JobKey("empty-request-test", JobTypes.LibrarySyncJob),
            new EmptyTickerPayload(),
            cancellationToken: CancellationToken.None
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result
            .Errors.Single()
            .Message.ShouldBe("Background job empty-request-test cannot be queued without a serialized request");
        tickerManager.Verify(x => x.AddAsync(It.IsAny<JobTimeTicker>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task ShouldAlwaysPersistSerializedRequest_WhenExecutingJobImmediately()
    {
        // Arrange
        JobTimeTicker? queuedTicker = null;
        var tickerManager = new Mock<ITimeTickerManager<JobTimeTicker>>();
        tickerManager
            .Setup(x => x.AddAsync(It.IsAny<JobTimeTicker>(), It.IsAny<CancellationToken>()))
            .Callback<JobTimeTicker, CancellationToken>((ticker, _) => queuedTicker = ticker)
            .ReturnsAsync(CreateSuccessfulTickerResult());
        var scheduler = CreateScheduler(tickerManager.Object);

        // Act
        await scheduler.ExecuteJob<TestTickerFunction, TestTickerPayload>(
            new JobKey("execute-request-test", JobTypes.LibrarySyncJob),
            new TestTickerPayload(),
            CancellationToken.None
        );

        // Assert
        queuedTicker.ShouldNotBeNull();
        queuedTicker.Request.ShouldNotBeNull();
        queuedTicker.Request.Length.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task ShouldAlwaysPersistSerializedRequest_WhenSchedulingSingleJob()
    {
        // Arrange
        JobTimeTicker? queuedTicker = null;
        var tickerManager = new Mock<ITimeTickerManager<JobTimeTicker>>();
        tickerManager
            .Setup(x => x.AddAsync(It.IsAny<JobTimeTicker>(), It.IsAny<CancellationToken>()))
            .Callback<JobTimeTicker, CancellationToken>((ticker, _) => queuedTicker = ticker)
            .ReturnsAsync(CreateSuccessfulTickerResult());
        var scheduler = CreateScheduler(tickerManager.Object);

        // Act
        await scheduler.ScheduleJob<TestTickerFunction, TestTickerPayload>(
            new JobKey("schedule-request-test", JobTypes.LibrarySyncJob),
            new TestTickerPayload(),
            DateTime.UtcNow.AddMinutes(2),
            CancellationToken.None
        );

        // Assert
        queuedTicker.ShouldNotBeNull();
        queuedTicker.Request.ShouldNotBeNull();
        queuedTicker.Request.Length.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task ShouldAlwaysPersistSerializedRequest_WhenSchedulingJobsInBatch()
    {
        // Arrange
        List<JobTimeTicker>? queuedTickers = null;
        var tickerManager = new Mock<ITimeTickerManager<JobTimeTicker>>();
        tickerManager
            .Setup(x => x.AddBatchAsync(It.IsAny<List<JobTimeTicker>>(), It.IsAny<CancellationToken>()))
            .Callback<List<JobTimeTicker>, CancellationToken>((tickers, _) => queuedTickers = tickers)
            .ReturnsAsync(CreateSuccessfulTickerBatchResult());
        var scheduler = CreateScheduler(tickerManager.Object);
        var jobs = new List<(JobKey JobKey, TestTickerPayload Request)>
        {
            (new JobKey("batch-request-1", JobTypes.LibraryComparisonJob), new TestTickerPayload()),
            (new JobKey("batch-request-2", JobTypes.LibraryComparisonJob), new TestTickerPayload()),
        };

        // Act
        await scheduler.ScheduleJobs<TestTickerFunction, TestTickerPayload>(jobs, DateTime.UtcNow.AddMinutes(2));

        // Assert
        queuedTickers.ShouldNotBeNull();
        queuedTickers.Count.ShouldBe(2);
        queuedTickers.ShouldAllBe(x => x.Request != null && x.Request.Length > 0);
    }

    [Test]
    public async Task ShouldPersistLibraryComparisonRequestProperties_WhenSchedulingJobsInBatch()
    {
        // Arrange
        List<JobTimeTicker>? queuedTickers = null;
        var tickerManager = new Mock<ITimeTickerManager<JobTimeTicker>>();
        tickerManager
            .Setup(x => x.AddBatchAsync(It.IsAny<List<JobTimeTicker>>(), It.IsAny<CancellationToken>()))
            .Callback<List<JobTimeTicker>, CancellationToken>((tickers, _) => queuedTickers = tickers)
            .ReturnsAsync(CreateSuccessfulTickerBatchResult());
        var scheduler = CreateScheduler(tickerManager.Object);
        var jobs = new List<(JobKey JobKey, PlexLibraryComparisonJobPayload Request)>
        {
            (
                PlexLibraryComparisonJob.GetJobKey(11, 21),
                new PlexLibraryComparisonJobPayload { OwnedPlexLibraryId = 11, RemotePlexLibraryId = 21 }
            ),
            (
                PlexLibraryComparisonJob.GetJobKey(12, 22),
                new PlexLibraryComparisonJobPayload { OwnedPlexLibraryId = 12, RemotePlexLibraryId = 22 }
            ),
        };

        // Act
        await scheduler.ScheduleJobs<PlexLibraryComparisonJob, PlexLibraryComparisonJobPayload>(
            jobs,
            DateTime.UtcNow.AddMinutes(2)
        );

        // Assert
        queuedTickers.ShouldNotBeNull();
        var tickers = queuedTickers;
        tickers.Count.ShouldBe(2);
        tickers[0].Request.ShouldNotBeNull();
        tickers[0].Request!.Length.ShouldBeGreaterThan(0);
        tickers[0].RequestJson.ShouldNotBeNull();
        var firstRequestJson = tickers[0].RequestJson!;
        firstRequestJson.OwnedPlexLibraryId.ShouldBe(11);
        firstRequestJson.RemotePlexLibraryId.ShouldBe(21);
        tickers[1].Request.ShouldNotBeNull();
        tickers[1].Request!.Length.ShouldBeGreaterThan(0);
        tickers[1].RequestJson.ShouldNotBeNull();
        var secondRequestJson = tickers[1].RequestJson!;
        secondRequestJson.OwnedPlexLibraryId.ShouldBe(12);
        secondRequestJson.RemotePlexLibraryId.ShouldBe(22);
    }

    private static BackgroundJobScheduler CreateScheduler(ITimeTickerManager<JobTimeTicker> tickerManager) =>
        new(
            Mock.Of<ILogger>(),
            Mock.Of<IReaparrDbContextFactory>(),
            tickerManager,
            Mock.Of<ICronTickerManager<JobCronTicker>>(),
            Mock.Of<ITickerQHostScheduler>(),
            Mock.Of<IAppRuntimeInfo>(),
            Mock.Of<ICommandExecutor>()
        );

    private static TickerResult<JobTimeTicker> CreateSuccessfulTickerResult() =>
        (TickerResult<JobTimeTicker>)
            Activator.CreateInstance(
                typeof(TickerResult<JobTimeTicker>),
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: [new JobTimeTicker()],
                culture: null
            )!;

    private static TickerResult<List<JobTimeTicker>> CreateSuccessfulTickerBatchResult() =>
        (TickerResult<List<JobTimeTicker>>)
            Activator.CreateInstance(
                typeof(TickerResult<List<JobTimeTicker>>),
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: [new List<JobTimeTicker>()],
                culture: null
            )!;

    private sealed record TestTickerPayload;

    private sealed class EmptyTickerPayload
    {
        [System.Text.Json.Serialization.JsonIgnore]
        public string Ignored { get; init; } = string.Empty;
    }

    private sealed class TestTickerFunction : ITickerFunction<TestTickerPayload>, ITickerFunction<EmptyTickerPayload>
    {
        public Task ExecuteAsync(
            TickerFunctionContext<TestTickerPayload> context,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;

        public Task ExecuteAsync(
            TickerFunctionContext<EmptyTickerPayload> context,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;
    }
}
