using Reaparr.Application;
using Reaparr.Data.Contracts;

namespace Reaparr.Application.UnitTests;

public class ScheduleAffectedLibraryComparisonJobsCommandHandlerUnitTests
    : BaseUnitTest<ScheduleAffectedLibraryComparisonJobsCommandHandler>
{
    [Test]
    public async Task ShouldInvalidateDistinctLibrariesOnce_WhenMultipleComparisonJobsAreQueued()
    {
        // Arrange
        await SetupDatabase(79, config =>
        {
            config.PlexServerCount = 3;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibraries = libraries.Skip(1).ToList();
        var ownedServerIds = ownedLibraries.Select(x => x.PlexServerId).ToList();

        await dbContext.PlexServers
            .Where(x => x.Id == remoteLibrary.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, false), CancellationToken);
        await dbContext.PlexServers
            .Where(x => ownedServerIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, true), CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ScheduleLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.InvalidateLibraries(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<string>()));

        var command = new ScheduleAffectedLibraryComparisonJobsCommand(remoteLibrary.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify(
            x => x.Send(It.IsAny<ScheduleLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()),
            Times.Exactly(ownedLibraries.Count)
        );
        Mock.Mock<IMediaQueryCache>().Verify(
            x => x.InvalidateLibraries(
                It.Is<IReadOnlyCollection<int>>(ids =>
                    ids.Count == libraries.Count && libraries.All(library => ids.Contains(library.Id))),
                $"Library comparisons scheduled for {PlexMediaType.Movie}"
            ),
            Times.Once()
        );
    }

    [Test]
    public async Task ShouldInvalidateOnlyLibrariesFromSuccessfulJobs_WhenSomeComparisonJobsFail()
    {
        // Arrange
        await SetupDatabase(80, config =>
        {
            config.PlexServerCount = 3;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var successfulOwnedLibrary = libraries[1];
        var failedOwnedLibrary = libraries[2];
        var ownedServerIds = libraries.Skip(1).Select(x => x.PlexServerId).ToList();

        await dbContext.PlexServers
            .Where(x => x.Id == remoteLibrary.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, false), CancellationToken);
        await dbContext.PlexServers
            .Where(x => ownedServerIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, true), CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(
                It.Is<ScheduleLibraryComparisonJobCommand>(command =>
                    command.OwnedPlexLibraryId == successfulOwnedLibrary.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(
                It.Is<ScheduleLibraryComparisonJobCommand>(command =>
                    command.OwnedPlexLibraryId == failedOwnedLibrary.Id),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("queue failed"));
        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.InvalidateLibraries(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<string>()));

        var command = new ScheduleAffectedLibraryComparisonJobsCommand(remoteLibrary.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IMediaQueryCache>().Verify(
            x => x.InvalidateLibraries(
                It.Is<IReadOnlyCollection<int>>(ids =>
                    ids.Count == 2
                    && ids.Contains(remoteLibrary.Id)
                    && ids.Contains(successfulOwnedLibrary.Id)
                    && !ids.Contains(failedOwnedLibrary.Id)),
                $"Library comparisons scheduled for {PlexMediaType.Movie}"
            ),
            Times.Once()
        );
    }

    [Test]
    public async Task ShouldBatchRemoteLibraries_WhenOwnedLibraryTriggersComparisonQueueing()
    {
        // Arrange
        await SetupDatabase(81, config =>
        {
            config.PlexServerCount = 3;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        var remoteLibraries = libraries.Take(2).ToList();
        var ownedLibrary = libraries[2];
        var remoteServerIds = remoteLibraries.Select(x => x.PlexServerId).ToList();

        await dbContext.PlexServers
            .Where(x => remoteServerIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, false), CancellationToken);
        await dbContext.PlexServers
            .Where(x => x.Id == ownedLibrary.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, true), CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ScheduleLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        Mock.Mock<IMediaQueryCache>()
            .Setup(x => x.InvalidateLibraries(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<string>()));

        var command = new ScheduleAffectedLibraryComparisonJobsCommand(ownedLibrary.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        foreach (var remoteLibrary in remoteLibraries)
        {
            Mock.Mock<ICommandExecutor>().Verify(
                x => x.Send(
                    It.Is<ScheduleLibraryComparisonJobCommand>(scheduledCommand =>
                        scheduledCommand.OwnedPlexLibraryId == ownedLibrary.Id
                        && scheduledCommand.RemotePlexLibraryId == remoteLibrary.Id),
                    It.IsAny<CancellationToken>()),
                Times.Once()
            );
        }
        Mock.Mock<IMediaQueryCache>().Verify(
            x => x.InvalidateLibraries(
                It.Is<IReadOnlyCollection<int>>(ids =>
                    ids.Count == libraries.Count && libraries.All(library => ids.Contains(library.Id))),
                $"Library comparisons scheduled for {PlexMediaType.Movie}"
            ),
            Times.Once()
        );
    }

    [Test]
    public async Task ShouldNotQueueOrInvalidate_WhenNoCompatibleOwnedLibraryExists()
    {
        // Arrange
        await SetupDatabase(82, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries.ToListAsync(CancellationToken);
        var serverIds = libraries.Select(x => x.PlexServerId).ToList();

        await dbContext.PlexServers
            .Where(x => serverIds.Contains(x.Id))
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, false), CancellationToken);

        var command = new ScheduleAffectedLibraryComparisonJobsCommand(libraries[0].Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        Mock.Mock<ICommandExecutor>().Verify(
            x => x.Send(It.IsAny<ScheduleLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()),
            Times.Never()
        );
        Mock.Mock<IMediaQueryCache>().Verify(
            x => x.InvalidateLibraries(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<string>()),
            Times.Never()
        );
    }

    [Test]
    public async Task ShouldNotInvalidateMediaCache_WhenNoComparisonJobIsQueuedSuccessfully()
    {
        // Arrange
        await SetupDatabase(80, config =>
        {
            config.PlexServerCount = 2;
            config.PlexMovieLibraryCount = 1;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;
        var libraries = await dbContext.PlexLibraries
            .OrderBy(x => x.Id)
            .ToListAsync(CancellationToken);
        var remoteLibrary = libraries[0];
        var ownedLibrary = libraries[1];

        await dbContext.PlexServers
            .Where(x => x.Id == remoteLibrary.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, false), CancellationToken);
        await dbContext.PlexServers
            .Where(x => x.Id == ownedLibrary.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.OwnedOverride, true), CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<ScheduleLibraryComparisonJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail("queue failed"));

        var command = new ScheduleAffectedLibraryComparisonJobsCommand(remoteLibrary.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        Mock.Mock<IMediaQueryCache>().Verify(
            x => x.InvalidateLibraries(It.IsAny<IReadOnlyCollection<int>>(), It.IsAny<string>()),
            Times.Never()
        );
    }
}