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
            x => x.AddBatchAsync(
                It.Is<List<JobTimeTicker>>(tickers => tickers.Count == 2),
                It.IsAny<CancellationToken>()
            ),
            Times.Once
        );
        tickerManager.Verify(
            x => x.AddAsync(It.IsAny<JobTimeTicker>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    private static TickerResult<JobTimeTicker> CreateSuccessfulTickerResult() =>
        (TickerResult<JobTimeTicker>)Activator.CreateInstance(
            typeof(TickerResult<JobTimeTicker>),
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            args: [new JobTimeTicker()],
            culture: null
        )!;

    private static TickerResult<List<JobTimeTicker>> CreateSuccessfulTickerBatchResult() =>
        (TickerResult<List<JobTimeTicker>>)Activator.CreateInstance(
            typeof(TickerResult<List<JobTimeTicker>>),
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            args: [new List<JobTimeTicker>()],
            culture: null
        )!;

    private sealed record TestTickerPayload;

    private sealed class TestTickerFunction : ITickerFunction<TestTickerPayload>
    {
        public Task ExecuteAsync(
            TickerFunctionContext<TestTickerPayload> context,
            CancellationToken cancellationToken = default
        ) => Task.CompletedTask;
    }
}