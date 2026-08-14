using FlexQuery.NET.Models;
using Reaparr.Application.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Data.UnitTests;

public class MediaQueryCacheUnitTests : BaseUnitTest<MediaQueryCache>
{
    // ──────────────────────────────────────────────────────────────
    // Invalidation fast paths
    // ──────────────────────────────────────────────────────────────

    [Test]
    public void ShouldNotStartCacheBuild_WhenInvalidatingAnEmptyCache()
    {
        // Act
        Sut.InvalidateLibraries([1, 2, 2], "comparison batch queued");

        // Assert
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Test]
    public void ShouldIgnoreInvalidLibraryIds_WhenInvalidatingCache()
    {
        // Act
        Sut.InvalidateLibraries([0, -1], "invalid library IDs");

        // Assert
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    [Test]
    public void ShouldSkipInvalidation_WhenInvalidationIsSuppressed()
    {
        // Arrange
        Sut.SuppressInvalidation = true;

        // Act
        Sut.InvalidateLibraries([1, 2], "sync storm");

        // Assert
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()), Times.Never());
    }

    // ──────────────────────────────────────────────────────────────
    // Cache miss → 503 (no synchronous blocking)
    // ──────────────────────────────────────────────────────────────

    [Test]
    public async Task ShouldReturn503ServiceUnavailable_WhenCacheIsCold()
    {
        // Arrange
        await SetupDatabase(
            70307,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 5;
            }
        );

        var filter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 10);

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("warming up"));
        result.Has503ServiceUnavailableError().ShouldBeTrue();
    }

    [Test]
    public async Task ShouldReturn503_WhenConcurrentRequestsMissSameSnapshot()
    {
        // Arrange
        await SetupDatabase(
            70315,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 30;
            }
        );

        var firstFilter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 5);
        var secondFilter = CreateFilter(sort: "sortIndex:asc", page: 2, pageSize: 5);

        // Act
        var results = await Task.WhenAll(
            Sut.GetMediaAsync(firstFilter, CancellationToken),
            Sut.GetMediaAsync(secondFilter, CancellationToken)
        );

        // Assert
        results.ShouldAllBe(x => x.IsFailed);
        results.ShouldAllBe(x => x.Has503ServiceUnavailableError());
    }

    // ──────────────────────────────────────────────────────────────
    // Cache bypass (query/filter/multi-sort still work directly)
    // ──────────────────────────────────────────────────────────────

    [Test]
    public async Task ShouldBypassCache_WhenQueryParameterIsSet()
    {
        // Arrange
        await SetupDatabase(
            70304,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 5;
            }
        );

        var filter = CreateFilter(query: "alpha", sort: "sortIndex:asc", page: 2, pageSize: 2);
        GetMediaByTypeCommand? capturedCommand = null;

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) =>
                {
                    capturedCommand = command;
                    return Task.FromResult(Result.Ok(CreateResult([101, 102])));
                }
            )
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand!.Filter.Parameters.Query.ShouldBe("alpha");
        capturedCommand.Filter.Parameters.Page.ShouldBe(2);
        result.Value.Items.Select(x => x.Id).ShouldBe([101, 102]);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldBypassCache_WhenFilterParameterIsSet()
    {
        // Arrange
        await SetupDatabase(
            70305,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 5;
            }
        );

        var filter = CreateFilter(filter: "Genres:any:Id:eq:1", sort: "sortIndex:asc", page: 3, pageSize: 2);
        GetMediaByTypeCommand? capturedCommand = null;

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) =>
                {
                    capturedCommand = command;
                    return Task.FromResult(Result.Ok(CreateResult([201, 202])));
                }
            )
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand!.Filter.Parameters.Filter.ShouldBe("Genres:any:Id:eq:1");
        capturedCommand.Filter.Parameters.Page.ShouldBe(3);
        result.Value.Items.Select(x => x.Id).ShouldBe([201, 202]);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldBypassCache_WhenSortContainsMultipleFields()
    {
        // Arrange
        await SetupDatabase(
            70306,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 5;
            }
        );

        var filter = CreateFilter(sort: "year:asc,title:desc", page: 4, pageSize: 2);
        GetMediaByTypeCommand? capturedCommand = null;

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) =>
                {
                    capturedCommand = command;
                    return Task.FromResult(Result.Ok(CreateResult([301, 302])));
                }
            )
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand!.Filter.Parameters.Sort.ShouldBe("year:asc,title:desc");
        capturedCommand.Filter.Parameters.Page.ShouldBe(4);
        result.Value.Items.Select(x => x.Id).ShouldBe([301, 302]);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    // ──────────────────────────────────────────────────────────────
    // BuildCache warms all-library snapshots synchronously.
    // Subsequent all-library requests hit the cache.
    // ──────────────────────────────────────────────────────────────

    [Test]
    public async Task ShouldWarmAllLibraryMovieAndTvShowSnapshots_WhenBuildCacheRuns()
    {
        // Arrange
        await SetupDatabase(
            70316,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.PlexTvShowLibraryCount = 1;
                cfg.MovieCount = 2;
                cfg.TvShowCount = 2;
            }
        );

        var capturedCommands = new List<GetMediaByTypeCommand>();

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) =>
                {
                    capturedCommands.Add(command);
                    return Task.FromResult(Result.Ok(CreateResult([1, 2], command.Filter.MediaType)));
                }
            )
            .Verifiable(Times.Exactly(14));

        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOfflineServers).Returns(true);
        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOwnedServers).Returns(true);

        // Act
        await Sut.BuildCache();

        // Assert
        capturedCommands.Count.ShouldBe(14);
        capturedCommands.Select(x => x.Filter.MediaType).Count(x => x == PlexMediaType.Movie).ShouldBe(7);
        capturedCommands.Select(x => x.Filter.MediaType).Count(x => x == PlexMediaType.TvShow).ShouldBe(7);
        capturedCommands.ShouldAllBe(x => x.Filter.PlexLibraryId == 0);
        capturedCommands.ShouldAllBe(x => x.Filter.Parameters.Page == null);
        capturedCommands.ShouldAllBe(x => x.Filter.Parameters.PageSize == null);
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldHitWarmedCache_WhenRequestingAllLibraries()
    {
        // Arrange
        await SetupDatabase(
            70302,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 30;
            }
        );

        // Setup mocks for warmup
        var allMovieIds = await IDbContext
            .PlexMovies.OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOfflineServers).Returns(false);
        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOwnedServers).Returns(false);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) => Task.FromResult(Result.Ok(CreateResult(allMovieIds, command.Filter.MediaType)))
            );

        await Sut.BuildCache();

        // Reset and verify no more executor calls needed (cache hits)
        Mock.Mock<ICommandExecutor>().Reset();

        var firstPageFilter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 10);
        var secondPageFilter = CreateFilter(sort: "sortIndex:asc", page: 2, pageSize: 10);

        // Act
        var firstResult = await Sut.GetMediaAsync(firstPageFilter, CancellationToken);
        var secondResult = await Sut.GetMediaAsync(secondPageFilter, CancellationToken);

        // Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        firstResult.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.Take(10));
        secondResult.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.Skip(10).Take(10));
    }

    [Test]
    public async Task ShouldReuseAscendingSnapshotAndReverseItems_WhenDescendingSortRequested()
    {
        // Arrange
        await SetupDatabase(
            70303,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 25;
            }
        );

        var allMovieIds = await IDbContext
            .PlexMovies.OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOfflineServers).Returns(false);
        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOwnedServers).Returns(false);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) => Task.FromResult(Result.Ok(CreateResult(allMovieIds, command.Filter.MediaType)))
            );

        await Sut.BuildCache();

        Mock.Mock<ICommandExecutor>().Reset();

        var ascendingFilter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 5);
        var descendingFilter = CreateFilter(sort: "sortIndex:desc", page: 1, pageSize: 5);

        // Act
        var ascendingResult = await Sut.GetMediaAsync(ascendingFilter, CancellationToken);
        var descendingResult = await Sut.GetMediaAsync(descendingFilter, CancellationToken);

        // Assert
        ascendingResult.IsSuccess.ShouldBeTrue();
        descendingResult.IsSuccess.ShouldBeTrue();
        ascendingResult.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.Take(5));
        descendingResult.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.AsEnumerable().Reverse().Take(5));
        descendingResult.Value.Items.Select(x => x.SortIndex).ShouldBe(Enumerable.Range(1, 5));
    }

    [Test]
    public async Task ShouldReturnEmptyItemsWithMetadata_WhenRequestedPageIsBeyondSnapshotRange()
    {
        // Arrange
        await SetupDatabase(
            70313,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 12;
            }
        );

        var allMovieIds = await IDbContext
            .PlexMovies.OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOfflineServers).Returns(false);
        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOwnedServers).Returns(false);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) =>
                    Task.FromResult(
                        Result.Ok(CreateResult(allMovieIds, command.Filter.MediaType, totalMediaSize: 12345))
                    )
            );

        await Sut.BuildCache();
        Mock.Mock<ICommandExecutor>().Reset();

        var filter = CreateFilter(sort: "sortIndex:asc", page: 99, pageSize: 10);

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Page.ShouldBe(99);
        result.Value.PageSize.ShouldBe(10);
        result.Value.TotalCount.ShouldBe(allMovieIds.Count);
        result.Value.MediaSize.ShouldBe(12345);
        result.Value.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldReturnAllItems_WhenRequestedPageSizeIsNull()
    {
        // Arrange
        await SetupDatabase(
            70312,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 12;
            }
        );

        var allMovieIds = await IDbContext
            .PlexMovies.OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOfflineServers).Returns(false);
        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOwnedServers).Returns(false);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) => Task.FromResult(Result.Ok(CreateResult(allMovieIds, command.Filter.MediaType)))
            );

        await Sut.BuildCache();
        Mock.Mock<ICommandExecutor>().Reset();

        var filter = CreateFilter(sort: "sortIndex:asc", page: null, pageSize: null);

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Page.ShouldBe(1);
        result.Value.PageSize.ShouldBe(allMovieIds.Count);
        result.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds);
    }

    [Test]
    public async Task ShouldReturnEmptyFilterArrays_InPageResponse()
    {
        // Arrange
        await SetupDatabase(
            70320,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 10;
            }
        );

        var allMovieIds = await IDbContext
            .PlexMovies.OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOfflineServers).Returns(false);
        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOwnedServers).Returns(false);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) => Task.FromResult(Result.Ok(CreateResult(allMovieIds, command.Filter.MediaType)))
            );

        await Sut.BuildCache();
        Mock.Mock<ICommandExecutor>().Reset();

        var filter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 5);

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Roles.ShouldBeEmpty();
        result.Value.Countries.ShouldBeEmpty();
        result.Value.Genres.ShouldBeEmpty();
        result.Value.Qualities.ShouldBeEmpty();
    }

    // ──────────────────────────────────────────────────────────────
    // Invalidation: marks dirty, returns stale, queues background
    // ──────────────────────────────────────────────────────────────

    [Test]
    public async Task ShouldReturnStaleData_WhenInvalidationMarksSnapshotDirty()
    {
        // Arrange
        await SetupDatabase(
            70317,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 20;
            }
        );

        var libraryId = await IDbContext
            .PlexLibraries.Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);
        var allMovieIds = await IDbContext
            .PlexMovies.OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOfflineServers).Returns(false);
        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOwnedServers).Returns(false);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) => Task.FromResult(Result.Ok(CreateResult(allMovieIds, command.Filter.MediaType)))
            );

        await Sut.BuildCache();
        Mock.Mock<ICommandExecutor>().Reset();

        var filter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 5);

        // Verify warm cache hit
        var firstResult = await Sut.GetMediaAsync(filter, CancellationToken);
        firstResult.IsSuccess.ShouldBeTrue();
        firstResult.Value.Items.ShouldNotBeEmpty();

        // Act — invalidate library (marks dirty, queues background rebuild)
        Sut.InvalidateLibraries([libraryId], "test invalidation");

        // Second call should still return stale cached data (not 503, not empty)
        var secondResult = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        secondResult.IsSuccess.ShouldBeTrue();
        secondResult.Value.Items.Count.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task ShouldNotEmptyState_WhenInvalidationHitsAndSubsequentRequestUsesStaleSnapshot()
    {
        // Arrange
        await SetupDatabase(
            70323,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 1;
                cfg.MovieCount = 15;
            }
        );

        var libraryId = await IDbContext
            .PlexLibraries.Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);
        var allMovieIds = await IDbContext
            .PlexMovies.OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOfflineServers).Returns(false);
        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOwnedServers).Returns(false);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) => Task.FromResult(Result.Ok(CreateResult(allMovieIds, command.Filter.MediaType)))
            );

        await Sut.BuildCache();
        Mock.Mock<ICommandExecutor>().Reset();

        var filter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 10);

        // Act — invalidate first (simulates library refresh), then request
        Sut.InvalidateLibraries([libraryId], "simulated library refresh");
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert — stale data available, not empty
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.Count.ShouldBe(10);
    }

    [Test]
    public async Task ShouldNotRebuild_WhenInvalidatingUnrelatedLibrary()
    {
        // Arrange
        await SetupDatabase(
            70318,
            cfg =>
            {
                cfg.PlexServerCount = 1;
                cfg.PlexMovieLibraryCount = 2;
                cfg.MovieCount = 10;
            }
        );

        var libraryIds = await IDbContext
            .PlexLibraries.Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .OrderBy(x => x)
            .ToListAsync(CancellationToken);
        var unrelatedId = libraryIds[0] + libraryIds[1] + 999;
        var allMovieIds = await IDbContext
            .PlexMovies.OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOfflineServers).Returns(false);
        Mock.Mock<IGeneralSettings>().Setup(x => x.HideMediaFromOwnedServers).Returns(false);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>(
                (command, _) => Task.FromResult(Result.Ok(CreateResult(allMovieIds, command.Filter.MediaType)))
            );

        await Sut.BuildCache();
        Mock.Mock<ICommandExecutor>().Reset();

        var filter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 5);

        // Act
        var firstResult = await Sut.GetMediaAsync(filter, CancellationToken);
        Sut.InvalidateLibraries([unrelatedId], "unrelated invalidation");
        var secondResult = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        secondResult.Value.Items.Select(x => x.Id).ShouldBe(firstResult.Value.Items.Select(x => x.Id));
    }

    // ──────────────────────────────────────────────────────────────
    // Helpers
    // ──────────────────────────────────────────────────────────────

    private static MediaQueryFilter CreateFilter(
        PlexMediaType mediaType = PlexMediaType.Movie,
        int plexLibraryId = 0,
        bool filterOfflineMedia = false,
        bool filterOwnedMedia = false,
        string? query = null,
        string? filter = null,
        string? sort = "sortIndex:asc",
        int? page = 1,
        int? pageSize = 10
    ) =>
        new()
        {
            MediaType = mediaType,
            PlexLibraryId = plexLibraryId,
            FilterOfflineMedia = filterOfflineMedia,
            FilterOwnedMedia = filterOwnedMedia,
            Parameters = new FlexQueryParameters
            {
                Query = query,
                Filter = filter,
                Sort = sort,
                Page = page,
                PageSize = pageSize,
            },
        };

    private static PagedMediaQueryResult CreateResult(
        IReadOnlyCollection<int> ids,
        PlexMediaType mediaType = PlexMediaType.Movie,
        long totalMediaSize = 0
    ) =>
        new()
        {
            TotalCount = ids.Count,
            MediaCount = ids.Count,
            MovieCount = mediaType == PlexMediaType.Movie ? ids.Count : 0,
            TvShowCount = mediaType == PlexMediaType.TvShow ? ids.Count : 0,
            MediaSize = totalMediaSize,
            TotalMediaSize = totalMediaSize,
            Roles = [1, 2],
            Countries = [3],
            Genres = [4, 5],
            Qualities = [6],
            NavigationIndexes = [new MediaNavigationIndexDTO { Label = "A", Index = 0 }],
            Items = ids.Select((id, index) => CreateItem(id, index, mediaType)).ToList(),
        };

    private static PlexMediaSlimDTO CreateItem(int id, int index, PlexMediaType mediaType) =>
        new()
        {
            Id = id,
            PlexApiRatingKey = id,
            PlexApiMetaDataKey = id,
            Title = $"Media {index:D3}",
            SearchTitle = $"Media {index:D3}",
            SortIndex = index + 1,
            Year = 2000 + index,
            Duration = 100 + index,
            MediaSize = 1000 + index,
            ChildCount = mediaType == PlexMediaType.TvShow ? 1 : 0,
            GrandChildCount = mediaType == PlexMediaType.TvShow ? 2 : 0,
            AddedAt = DateTime.UnixEpoch.AddDays(index),
            UpdatedAt = DateTime.UnixEpoch.AddDays(index + 1),
            PlexLibraryId = 1,
            PlexServerId = 1,
            Type = mediaType,
            HasThumb = false,
            Qualities = new List<PlexMediaQualityDTO>(),
        };
}
