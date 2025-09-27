using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Autofac.Extras.Moq;
using Reaparr.FileSystem.Contracts;
using Shouldly;
using Xunit;

namespace Reaparr.FileSystem.UnitTests;

public class MoveFileWithResumeCommandHandlerUnitTests : BaseUnitTest<MoveFileWithResumeCommandHandler>
{
    public MoveFileWithResumeCommandHandlerUnitTests(ITestOutputHelper output)
        : base(output) { }

    private static byte[] CreateBytes(int length)
    {
        var data = new byte[length];
        for (var i = 0; i < length; i++)
            data[i] = (byte)('A' + (i % 26));
        return data;
    }

    [Fact]
    public async Task ExecuteAsync_CopiesSmallFile_SuccessAndContentMatches()
    {
        var sourcePath = "/test/source.bin";
        var targetPath = "/test/target.bin";
        var content = CreateBytes(256 * 1024);

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(Array.Empty<byte>()));
        });

        MoveFileTransferProgressDTO? lastProgress = null;
        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = content.LongLength,
            Progress = p => lastProgress = p,
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        var file = mock.Create<IFile>();
        using var targetStream = file.Open(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var readBack = new byte[targetStream.Length];
        _ = await targetStream.ReadAsync(readBack, 0, readBack.Length, CancellationToken.None);

        readBack.ShouldBe(content);
        lastProgress.ShouldNotBeNull();
        lastProgress!.Transferred.ShouldBe(content.LongLength);
        lastProgress.DataTotal.ShouldBe(content.LongLength);
        lastProgress.FileTransferSpeed.ShouldBeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ExecuteAsync_ReportsProgressMultipleTimes_ForLargeFile()
    {
        var sourcePath = "/test/source-large.bin";
        var targetPath = "/test/target-large.bin";
        var content = CreateBytes(2_500_000); // ~2.5 MB -> 3 iterations with 1MB buffer

        var progressCalls = 0;

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(Array.Empty<byte>()));
        });

        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = content.LongLength,
            Progress = _ => progressCalls++,
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();
        progressCalls.ShouldBeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task ExecuteAsync_ResumeFromOffset_WritesRemainderOnly()
    {
        var sourcePath = "/test/source-resume.bin";
        var targetPath = "/test/target-resume.bin";
        var content = CreateBytes(1_500_000); // 1.5 MB
        var offset = 1_000_000; // 1 MB

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            // Pre-populate target with first offset bytes
            fs.AddFile(targetPath, new MockFileData(content.Take(offset).ToArray()));
        });

        MoveFileTransferProgressDTO? last = null;
        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = offset,
            DataTotal = content.LongLength,
            Progress = p => last = p,
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();

        var file = mock.Create<IFile>();
        using var targetStream = file.Open(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var readBack = new byte[targetStream.Length];
        _ = await targetStream.ReadAsync(readBack, 0, readBack.Length, CancellationToken.None);
        readBack.ShouldBe(content);

        last.ShouldNotBeNull();
        last!.Transferred.ShouldBe(content.LongLength);
        last.DataTotal.ShouldBe(content.LongLength);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationStopsFurtherWrites_WhenCancelledBeforeStart()
    {
        var sourcePath = "/test/source-cancel.bin";
        var targetPath = "/test/target-cancel.bin";
        var content = CreateBytes(2_500_000); // multiple iterations

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(Array.Empty<byte>()));
        });

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = content.LongLength,
            Progress = _ => { },
        };

        var result = await _sut.ExecuteAsync(command, cts.Token);
        result.IsSuccess.ShouldBeTrue(); // handler returns Ok even on cancellation

        var file = mock.Create<IFile>();
        using var targetStream = file.Open(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        // Only first chunk (1MB) should have been written before cancellation check breaks
        targetStream.Length.ShouldBeGreaterThan(0);
        targetStream.Length.ShouldBeLessThan(content.LongLength);
    }

    [Fact]
    public async Task ExecuteAsync_CancellationRequestedInsideProgress_StopsAfterNextCheck()
    {
        var sourcePath = "/test/source-cancel2.bin";
        var targetPath = "/test/target-cancel2.bin";
        var content = CreateBytes(2_500_000);

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(Array.Empty<byte>()));
        });

        var cts = new CancellationTokenSource();

        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = content.LongLength,
            Progress = p =>
            {
                if (p.Transferred >= 1_048_576)
                    cts.Cancel();
            },
        };

        var result = await _sut.ExecuteAsync(command, cts.Token);
        result.IsSuccess.ShouldBeTrue();

        var file = mock.Create<IFile>();
        using var targetStream = file.Open(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        targetStream.Length.ShouldBeLessThan(content.LongLength);
        targetStream.Length.ShouldBeGreaterThanOrEqualTo(1_048_576);
    }

    [Fact]
    public async Task ExecuteAsync_OpenTargetFails_ReturnsFailed()
    {
        var sourcePath = "/test/source-openfail.bin";
        var targetPath = "/test/target-openfail.bin";
        var content = CreateBytes(1024);

        // Do not register a filesystem file; instead, mock IFile to throw on target open
        var fileMock = mock.Mock<IFile>();
        fileMock
            .Setup(f => f.Open(targetPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            .Throws(new UnauthorizedAccessException("no write"));
        // Source open setup not required; handler fails on target open first.

        MoveFileTransferProgressDTO? last = null;
        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = content.LongLength,
            Progress = p => last = p,
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
        last.ShouldBeNull();
    }

    [Fact]
    public async Task ExecuteAsync_OpenSourceFails_ReturnsFailed()
    {
        var sourcePath = "/test/source-openfail2.bin";
        var targetPath = "/test/target-openfail2.bin";

        // Use MockFileSystem: ensure directory exists; do NOT create the source file.
        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(targetPath, new MockFileData(new byte[0]));
        });

        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = 100,
            Progress = _ => { },
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);
        result.IsFailed.ShouldBeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_CurrentOffsetEqualsTotal_NoWritesAndOk()
    {
        var sourcePath = "/test/source-offset-eq.bin";
        var targetPath = "/test/target-offset-eq.bin";
        var content = CreateBytes(128 * 1024);

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(content));
        });

        var progressCalled = false;
        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = content.LongLength,
            DataTotal = content.LongLength,
            Progress = _ => progressCalled = true,
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        progressCalled.ShouldBeFalse();

        var file = mock.Create<IFile>();
        using var targetStream = file.Open(targetPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        targetStream.Length.ShouldBe(content.LongLength);
    }

    [Fact]
    public async Task ExecuteAsync_CurrentOffsetGreaterThanTotal_NoWritesAndOk()
    {
        var sourcePath = "/test/source-offset-gt.bin";
        var targetPath = "/test/target-offset-gt.bin";
        var content = CreateBytes(64 * 1024);

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(content));
        });

        var progressCalled = false;
        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = content.LongLength + 10,
            DataTotal = content.LongLength,
            Progress = _ => progressCalled = true,
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        progressCalled.ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_ProgressSpeedIsNonNegative()
    {
        var sourcePath = "/test/source-speed.bin";
        var targetPath = "/test/target-speed.bin";
        var content = CreateBytes(600 * 1024);

        var speeds = new List<long>();

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(Array.Empty<byte>()));
        });

        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = content.LongLength,
            Progress = p => speeds.Add(p.FileTransferSpeed),
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        speeds.Count.ShouldBeGreaterThan(0);
        speeds.ShouldAllBe(x => x >= 0);
    }

    [Fact]
    public async Task ExecuteAsync_ProgressDataTotalMatchesCommand()
    {
        var sourcePath = "/test/source-datatotal.bin";
        var targetPath = "/test/target-datatotal.bin";
        var content = CreateBytes(123_456);

        var seenTotals = new HashSet<long>();

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(Array.Empty<byte>()));
        });

        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = 999_999, // intentionally different from file size
            Progress = p => seenTotals.Add(p.DataTotal),
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);
        result.IsSuccess.ShouldBeTrue();
        seenTotals.ShouldContain(999_999);
    }

    [Fact]
    public async Task ExecuteAsync_Success_DeletesSourceFileAfterCompletion()
    {
        var sourcePath = "/test/source-delete-success.bin";
        var targetPath = "/test/target-delete-success.bin";
        var content = CreateBytes(256 * 1024);

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(Array.Empty<byte>()));
        });

        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = content.LongLength,
            Progress = _ => { },
        };

        var result = await _sut.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.ShouldBeTrue();

        var file = mock.Create<IFile>();
        file.Exists(sourcePath).ShouldBeFalse();
    }

    [Fact]
    public async Task ExecuteAsync_Cancelled_DoesNotDeleteSourceFile()
    {
        var sourcePath = "/test/source-delete-cancel.bin";
        var targetPath = "/test/target-delete-cancel.bin";
        var content = CreateBytes(2_500_000);

        SetupFileSystem(fs =>
        {
            fs.AddDirectory("/test");
            fs.AddFile(sourcePath, new MockFileData(content));
            fs.AddFile(targetPath, new MockFileData(Array.Empty<byte>()));
        });

        var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new MoveFileWithResumeCommand
        {
            SourcePath = sourcePath,
            TargetPath = targetPath,
            CurrentOffset = 0,
            DataTotal = content.LongLength,
            Progress = _ => { },
        };

        var result = await _sut.ExecuteAsync(command, cts.Token);

        result.IsSuccess.ShouldBeTrue();

        var file = mock.Create<IFile>();
        file.Exists(sourcePath).ShouldBeTrue();
    }
}
