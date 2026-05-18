using Reaparr.Application.Contracts;

namespace Reaparr.BackgroundJobs.UnitTests;

public class CheckPlexLibrariesForUpdatesCommandHandlerUnitTests
    : BaseUnitTest<CheckPlexLibrariesForUpdatesCommandHandler>
{
    [Test]
    public async Task ShouldRefreshLibraryAccessForEnabledServers_WhenAutoSyncIsEnabled()
    {
        // Arrange
        await SetupDatabase(4501, config =>
        {
            config.PlexServerCount = 2;
            config.PlexAccountCount = 1;
        });

        var serverIds = await IDbContext.PlexServers.AsNoTracking().Select(x => x.Id).ToListAsync(CancellationToken);
        var accountId = await IDbContext.PlexAccounts.Select(x => x.Id).FirstAsync(CancellationToken);


        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<RefreshLibraryAccessCommand>(cmd =>
                        cmd.PlexAccountId == accountId && serverIds.Contains(cmd.PlexServerId)
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new PlexLibraryAccessRefreshResponse { Reports = [], OfflineServers = [] }))
            .Verifiable(Times.Exactly(2));

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<QueueLibrarySyncJobCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Never());

        // Act
        var result = await Sut.ExecuteAsync(new CheckPlexLibrariesForUpdatesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldQueueOnlyOutdatedLibraries_WhenUpdatedAtIsNewerThanSyncedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        await SetupDatabase(4502, config =>
        {
            config.PlexServerCount = 1;
            config.PlexAccountCount = 1;
            config.PlexMovieLibraryCount = 3;
            config.PlexTvShowLibraryCount = 0;
        });

        var serverId = await IDbContext.PlexServers.Select(x => x.Id).FirstAsync(CancellationToken);
        var libraries = await IDbContext.PlexLibraries.AsTracking().OrderBy(x => x.Id).ToListAsync(CancellationToken);
        libraries[0].PlexServerId = serverId;
        libraries[0].UpdatedAt = now.AddHours(-1);
        libraries[0].SyncedAt = now.AddHours(-2);
        libraries[1].PlexServerId = serverId;
        libraries[1].UpdatedAt = now.AddHours(-2);
        libraries[1].SyncedAt = now.AddHours(-1);
        libraries[2].PlexServerId = serverId;
        libraries[2].UpdatedAt = now.AddHours(-1);
        libraries[2].SyncedAt = null;
        await IDbContext.SaveChangesAsync(CancellationToken);

        var expectedLibraryIds = new[] { libraries[0].Id, libraries[2].Id };
        var accountId = await IDbContext.PlexAccounts.Select(x => x.Id).FirstAsync(CancellationToken);


        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<RefreshLibraryAccessCommand>(cmd => cmd.PlexAccountId == accountId && cmd.PlexServerId == serverId),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new PlexLibraryAccessRefreshResponse { Reports = [], OfflineServers = [] }))
            .Verifiable(Times.Once());

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<QueueLibrarySyncJobCommand>(cmd =>
                        cmd.PlexLibraryIds.OrderBy(id => id).SequenceEqual(expectedLibraryIds.OrderBy(id => id))
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok())
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.ExecuteAsync(new CheckPlexLibrariesForUpdatesCommand(), CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        Mock.Mock<ICommandExecutor>().Verify();
    }

}
