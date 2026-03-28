using System.Reflection;
using Reaparr.External.Contracts;

namespace Reaparr.External.UnitTests;

public class DashMpdCliWrapperUnitTests : BaseUnitTest
{
    [Test]
    public async Task ShouldEmitSuccessfulCompletion_WhenProcessExitsZero()
    {
        var wrapper = CreateWrapper("/usr/bin/true");
        var completedTcs = new TaskCompletionSource<DashDownloadCompletedEventArgs>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var completedObservableTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var subscription = wrapper.DownloadCompleted.Subscribe(
            args => completedTcs.TrySetResult(args),
            () => completedObservableTcs.TrySetResult(true)
        );

        var result = await wrapper.StartAsync(CreateOptions());
        var completed = await completedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken);
        var observableCompleted = await completedObservableTcs.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            CancellationToken
        );

        result.IsSuccess.ShouldBeTrue();
        completed.IsSuccess.ShouldBeTrue();
        completed.Cancelled.ShouldBeFalse();
        completed.ExitCode.ShouldBe(0);
        completed.Result.IsSuccess.ShouldBeTrue();
        observableCompleted.ShouldBeTrue();

        await wrapper.DisposeAsync();
    }

    [Test]
    public async Task ShouldEmitFailedCompletion_WhenProcessExitsNonZero()
    {
        var wrapper = CreateWrapper("/usr/bin/false");
        var completedTcs = new TaskCompletionSource<DashDownloadCompletedEventArgs>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var completedObservableTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        using var subscription = wrapper.DownloadCompleted.Subscribe(
            args => completedTcs.TrySetResult(args),
            () => completedObservableTcs.TrySetResult(true)
        );

        var result = await wrapper.StartAsync(CreateOptions());
        var completed = await completedTcs.Task.WaitAsync(TimeSpan.FromSeconds(5), CancellationToken);
        var observableCompleted = await completedObservableTcs.Task.WaitAsync(
            TimeSpan.FromSeconds(5),
            CancellationToken
        );

        result.IsFailed.ShouldBeTrue();
        completed.IsSuccess.ShouldBeFalse();
        completed.Cancelled.ShouldBeFalse();
        completed.ExitCode.ShouldNotBeNull();
        completed.Result.IsFailed.ShouldBeTrue();
        observableCompleted.ShouldBeTrue();

        await wrapper.DisposeAsync();
    }

    private static DashMpdCliWrapper CreateWrapper(string binaryPath)
    {
        var fileSystem = new System.IO.Abstractions.FileSystem();
        var logger = new LoggerConfiguration().CreateLogger();
        var wrapper = new DashMpdCliWrapper(logger, fileSystem.File, fileSystem.Directory);

        var binaryPathField = typeof(DashMpdCliWrapper).GetField(
            "_binaryPath",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        binaryPathField.ShouldNotBeNull();
        binaryPathField!.SetValue(wrapper, binaryPath);

        return wrapper;
    }

    private static DashMpdCliOptions CreateOptions()
    {
        var workingDirectory = Path.Combine(Path.GetTempPath(), $"reaparr-dash-wrapper-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(workingDirectory);

        return new DashMpdCliOptions
        {
            MpdUrl = "https://example.com/manifest.mpd",
            Output = Path.Combine(workingDirectory, "output.mkv.reaptemp"),
            WorkingDirectory = workingDirectory,
            Quiet = true,
        };
    }
}
