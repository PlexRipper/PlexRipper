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
        var dbContext = IDbContext;

        await dbContext.PlexServers
            .IgnoreQueryFilters()
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, true), CancellationToken);

        var serverIds = await dbContext.PlexServers.Select(x => x.Id).ToListAsync(CancellationToken);
        var accountId = await dbContext.PlexAccounts.IgnoreQueryFilters().Select(x => x.Id).FirstAsync(CancellationToken);

        await dbContext.PlexAccountServers.IgnoreQueryFilters().ExecuteDeleteAsync(CancellationToken);
        dbContext.PlexAccountServers.AddRange(
            serverIds.Select(serverId => new PlexAccountServer
            {
                PlexAccountId = accountId,
                PlexServerId = serverId,
                AuthToken = $"token-{serverId}",
                AuthTokenCreationDate = DateTime.UtcNow,
                IsServerOwned = true,
            })
        );
        await dbContext.SaveChangesNewAsync(CancellationToken);

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
    public async Task ShouldSkipServerWithoutTokenMapping_WhenRefreshingLibraryAccess()
    {
        // Arrange
        await SetupDatabase(4503, config =>
        {
            config.PlexServerCount = 2;
            config.PlexAccountCount = 1;
        });
        var dbContext = IDbContext;

        await dbContext.PlexServers
            .IgnoreQueryFilters()
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, true), CancellationToken);

        var serverIds = await dbContext.PlexServers
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        var accountId = await dbContext.PlexAccounts.IgnoreQueryFilters().Select(x => x.Id).FirstAsync(CancellationToken);

        await dbContext.PlexAccountServers.IgnoreQueryFilters().ExecuteDeleteAsync(CancellationToken);
        dbContext.PlexAccountServers.Add(
            new PlexAccountServer
            {
                PlexAccountId = accountId,
                PlexServerId = serverIds[0],
                AuthToken = "token-only-first-server",
                AuthTokenCreationDate = DateTime.UtcNow,
                IsServerOwned = true,
            }
        );
        await dbContext.SaveChangesNewAsync(CancellationToken);

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<RefreshLibraryAccessCommand>(cmd =>
                        cmd.PlexAccountId == accountId && cmd.PlexServerId == serverIds[0]
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new PlexLibraryAccessRefreshResponse { Reports = [], OfflineServers = [] }))
            .Verifiable(Times.Once());

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
    public async Task ShouldQueueOnlyOutdatedLibraries_WhenMarkedByRefresh()
    {
        // Arrange
        var now = DateTime.UtcNow;
        await SetupDatabase(4502, config =>
        {
            config.PlexServerCount = 1;
            config.PlexAccountCount = 1;
            config.PlexMovieLibraryCount = 4;
            config.PlexTvShowLibraryCount = 0;
        });
        var dbContext = IDbContext;

        await dbContext.PlexServers
            .IgnoreQueryFilters()
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, true), CancellationToken);

        var serverId = await dbContext.PlexServers.Select(x => x.Id).FirstAsync(CancellationToken);
        var libraries = await dbContext.PlexLibraries.AsTracking().OrderBy(x => x.Id).ToListAsync(CancellationToken);
        libraries[0].PlexServerId = serverId;
        libraries[0].UpdatedAt = now;
        libraries[0].ContentChangedAt = 10;
        libraries[0].SyncedAt = now.AddHours(-2);
        libraries[0].Outdated = false;
        libraries[1].PlexServerId = serverId;
        libraries[1].UpdatedAt = now;
        libraries[1].ContentChangedAt = 20;
        libraries[1].SyncedAt = now.AddHours(-1);
        libraries[1].Outdated = false;
        libraries[2].PlexServerId = serverId;
        libraries[2].UpdatedAt = now;
        libraries[2].ContentChangedAt = 30;
        libraries[2].SyncedAt = now.AddHours(-4);
        libraries[2].Outdated = true;
        libraries[3].PlexServerId = serverId;
        libraries[3].UpdatedAt = now;
        libraries[3].ContentChangedAt = 40;
        libraries[3].SyncedAt = now.AddHours(-3);
        libraries[3].Outdated = true;
        await dbContext.SaveChangesNewAsync(CancellationToken);

        var expectedLibraryIds = new[] { libraries[2].Id, libraries[3].Id };
        var accountId = await dbContext.PlexAccounts.IgnoreQueryFilters().Select(x => x.Id).FirstAsync(CancellationToken);

        await dbContext.PlexAccountServers.IgnoreQueryFilters().ExecuteDeleteAsync(CancellationToken);
        dbContext.PlexAccountServers.Add(
            new PlexAccountServer
            {
                PlexAccountId = accountId,
                PlexServerId = serverId,
                AuthToken = "token-outdated",
                AuthTokenCreationDate = DateTime.UtcNow,
                IsServerOwned = true,
            }
        );
        await dbContext.SaveChangesNewAsync(CancellationToken);
        dbContext.ClearChangeTracker();

        var mappingExists = await dbContext.PlexAccountServers
            .IgnoreQueryFilters()
            .AnyAsync(x => x.PlexAccountId == accountId && x.PlexServerId == serverId, CancellationToken);
        mappingExists.ShouldBeTrue();

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
                        cmd.PlexLibraryIds.OrderBy(id => id).ToList().SequenceEqual(expectedLibraryIds.OrderBy(id => id))
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
