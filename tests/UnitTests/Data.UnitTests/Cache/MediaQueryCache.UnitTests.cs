using FlexQuery.NET.Models;
using Reaparr.Application.Contracts;
using Reaparr.Settings.Contracts;

namespace Reaparr.Data.UnitTests;

public class MediaQueryCacheUnitTests : BaseUnitTest<MediaQueryCache>
{
    [Test]
    public async Task ShouldReturnSecondPageItems_WhenUsingRealQueryHandlerSnapshot()
    {
        // Arrange
        await SetupDatabase(70320, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 250;
        });

        var dbContext = IDbContext;
        var allMovieIds = await dbContext.PlexMovies
            .OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var filter = CreateFilter(sort: "sortIndex:asc", page: 2, pageSize: 100);
        var realHandler = new GetMediaByTypeCommandHandler(dbContext);

        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, token) => realHandler.ExecuteAsync(command, token))
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(allMovieIds.Count);
        result.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.Skip(100).Take(100));
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public async Task ShouldReturnDeepPageItems_WhenSnapshotContainsFullResult()
    {
        // Arrange
        await SetupDatabase(70301, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 250;
        });

        var dbContext = IDbContext;
        var allMovieIds = await dbContext.PlexMovies
            .OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var expectedPageIds = allMovieIds.Skip(200).Take(50).ToList();

        GetMediaByTypeCommand? capturedCommand = null;
        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, _) =>
            {
                capturedCommand = command;
                return Task.FromResult(Result.Ok(CreateResult(allMovieIds)));
            })
            .Verifiable(Times.Once());

        var filter = CreateFilter(sort: "sortIndex:asc", page: 5, pageSize: 50);

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(allMovieIds.Count);
        capturedCommand.ShouldNotBeNull();
        capturedCommand!.Filter.Parameters.Page.ShouldBeNull();
        capturedCommand.Filter.Parameters.PageSize.ShouldBeNull();
        result.Value.Items.Count.ShouldBe(50);
        result.Value.Items.Select(x => x.Id).ShouldBe(expectedPageIds);
        result.Value.Items.Select(x => x.SortIndex).ShouldBe(Enumerable.Range(201, 50));
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldReuseCachedSnapshot_WhenOnlyPageChanges()
    {
        // Arrange
        await SetupDatabase(70302, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 30;
        });

        var allMovieIds = await IDbContext.PlexMovies
            .OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var firstPageFilter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 10);
        var secondPageFilter = CreateFilter(sort: "sortIndex:asc", page: 2, pageSize: 10);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateResult(allMovieIds)))
            .Verifiable(Times.Once());

        // Act
        var firstResult = await Sut.GetMediaAsync(firstPageFilter, CancellationToken);
        var secondResult = await Sut.GetMediaAsync(secondPageFilter, CancellationToken);

        // Assert
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        firstResult.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.Take(10));
        secondResult.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.Skip(10).Take(10));
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldReuseAscendingSnapshotAndReverseItems_WhenDescendingSortRequested()
    {
        // Arrange
        await SetupDatabase(70303, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 25;
        });

        var allMovieIds = await IDbContext.PlexMovies
            .OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var ascendingFilter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 5);
        var descendingFilter = CreateFilter(sort: "sortIndex:desc", page: 1, pageSize: 5);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateResult(allMovieIds)))
            .Verifiable(Times.Once());

        // Act
        var ascendingResult = await Sut.GetMediaAsync(ascendingFilter, CancellationToken);
        var descendingResult = await Sut.GetMediaAsync(descendingFilter, CancellationToken);

        // Assert
        ascendingResult.IsSuccess.ShouldBeTrue();
        descendingResult.IsSuccess.ShouldBeTrue();
        ascendingResult.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.Take(5));
        descendingResult.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.AsEnumerable().Reverse().Take(5));
        descendingResult.Value.Items.Select(x => x.SortIndex).ShouldBe(Enumerable.Range(1, 5));
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldBypassCache_WhenQueryParameterIsSet()
    {
        // Arrange
        await SetupDatabase(70304, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 5;
        });

        var filter = CreateFilter(query: "alpha", sort: "sortIndex:asc", page: 2, pageSize: 2);
        GetMediaByTypeCommand? capturedCommand = null;

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, _) =>
            {
                capturedCommand = command;
                return Task.FromResult(Result.Ok(CreateResult([101, 102])));
            })
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand.ShouldNotBeNull();
        capturedCommand!.Filter.Parameters.Query.ShouldBe("alpha");
        capturedCommand.Filter.Parameters.Page.ShouldBe(2);
        capturedCommand.Filter.Parameters.PageSize.ShouldBe(2);
        result.Value.Items.Select(x => x.Id).ShouldBe([101, 102]);
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldBypassCache_WhenFilterParameterIsSet()
    {
        // Arrange
        await SetupDatabase(70305, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 5;
        });

        var filter = CreateFilter(filter: "Genres:any:Id:eq:1", sort: "sortIndex:asc", page: 3, pageSize: 2);
        GetMediaByTypeCommand? capturedCommand = null;

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, _) =>
            {
                capturedCommand = command;
                return Task.FromResult(Result.Ok(CreateResult([201, 202])));
            })
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand.ShouldNotBeNull();
        capturedCommand!.Filter.Parameters.Filter.ShouldBe("Genres:any:Id:eq:1");
        capturedCommand.Filter.Parameters.Page.ShouldBe(3);
        capturedCommand.Filter.Parameters.PageSize.ShouldBe(2);
        result.Value.Items.Select(x => x.Id).ShouldBe([201, 202]);
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldBypassCache_WhenSortContainsMultipleFields()
    {
        // Arrange
        await SetupDatabase(70306, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 5;
        });

        var filter = CreateFilter(sort: "year:asc,title:desc", page: 4, pageSize: 2);
        GetMediaByTypeCommand? capturedCommand = null;

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, _) =>
            {
                capturedCommand = command;
                return Task.FromResult(Result.Ok(CreateResult([301, 302])));
            })
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand.ShouldNotBeNull();
        capturedCommand!.Filter.Parameters.Sort.ShouldBe("year:asc,title:desc");
        capturedCommand.Filter.Parameters.Page.ShouldBe(4);
        capturedCommand.Filter.Parameters.PageSize.ShouldBe(2);
        result.Value.Items.Select(x => x.Id).ShouldBe([301, 302]);
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldReturnFailure_WhenSnapshotBuildCommandFails()
    {
        // Arrange
        await SetupDatabase(70307, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 5;
        });

        var filter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 10);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail<PagedMediaQueryResult>("snapshot build failed"))
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message == "snapshot build failed");
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldNormalizeAllLibrarySortIndexToSearchTitle_WhenMultipleLibrariesAreInScope()
    {
        // Arrange
        await SetupDatabase(70308, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 5;
        });

        var filter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 5);
        GetMediaByTypeCommand? capturedCommand = null;

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, _) =>
            {
                capturedCommand = command;
                return Task.FromResult(Result.Ok(CreateResult([1, 2, 3, 4, 5])));
            })
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand.ShouldNotBeNull();
        capturedCommand!.Filter.Parameters.Sort.ShouldBe("SearchTitle:asc");
        capturedCommand.Filter.Parameters.Page.ShouldBeNull();
        capturedCommand.Filter.Parameters.PageSize.ShouldBeNull();
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldKeepSortIndexSort_WhenSingleSpecificLibraryIsInScope()
    {
        // Arrange
        await SetupDatabase(70309, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 5;
        });

        var libraryId = await IDbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);
        var filter = CreateFilter(plexLibraryId: libraryId, sort: "sortIndex:asc", page: 1, pageSize: 5);
        GetMediaByTypeCommand? capturedCommand = null;

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, _) =>
            {
                capturedCommand = command;
                return Task.FromResult(Result.Ok(CreateResult([1, 2, 3, 4, 5])));
            })
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand.ShouldNotBeNull();
        capturedCommand!.Filter.Parameters.Sort.ShouldBe("sortIndex:asc");
        capturedCommand.Filter.Parameters.Page.ShouldBeNull();
        capturedCommand.Filter.Parameters.PageSize.ShouldBeNull();
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldUseSearchTitleDefaultSort_WhenAllLibraryModeHasMultipleLibraries()
    {
        // Arrange
        await SetupDatabase(70310, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 5;
        });

        var filter = CreateFilter(sort: null, page: 1, pageSize: 5);
        GetMediaByTypeCommand? capturedCommand = null;

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, _) =>
            {
                capturedCommand = command;
                return Task.FromResult(Result.Ok(CreateResult([1, 2, 3, 4, 5])));
            })
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand.ShouldNotBeNull();
        capturedCommand!.Filter.Parameters.Sort.ShouldBe("SearchTitle:asc");
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldUseSortIndexDefaultSort_WhenSingleLibraryIsInScope()
    {
        // Arrange
        await SetupDatabase(70311, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 5;
        });

        var filter = CreateFilter(sort: null, page: 1, pageSize: 5);
        GetMediaByTypeCommand? capturedCommand = null;

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, _) =>
            {
                capturedCommand = command;
                return Task.FromResult(Result.Ok(CreateResult([1, 2, 3, 4, 5])));
            })
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        capturedCommand.ShouldNotBeNull();
        capturedCommand!.Filter.Parameters.Sort.ShouldBe("sortIndex:asc");
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldReturnAllItems_WhenRequestedPageSizeIsNull()
    {
        // Arrange
        await SetupDatabase(70312, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 12;
        });

        var allMovieIds = await IDbContext.PlexMovies
            .OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var filter = CreateFilter(sort: "sortIndex:asc", page: null, pageSize: null);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateResult(allMovieIds)))
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Page.ShouldBe(1);
        result.Value.PageSize.ShouldBe(allMovieIds.Count);
        result.Value.Items.Select(x => x.Id).ShouldBe(allMovieIds);
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldReturnEmptyItemsWithMetadata_WhenRequestedPageIsBeyondSnapshotRange()
    {
        // Arrange
        await SetupDatabase(70313, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 12;
        });

        var allMovieIds = await IDbContext.PlexMovies
            .OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var filter = CreateFilter(sort: "sortIndex:asc", page: 99, pageSize: 10);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateResult(allMovieIds, totalMediaSize: 12345)))
            .Verifiable(Times.Once());

        // Act
        var result = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Page.ShouldBe(99);
        result.Value.PageSize.ShouldBe(10);
        result.Value.TotalCount.ShouldBe(allMovieIds.Count);
        result.Value.MediaSize.ShouldBe(12345);
        result.Value.Items.ShouldBeEmpty();
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldUseSeparateSnapshots_WhenVisibilityFiltersChange()
    {
        // Arrange
        await SetupDatabase(70314, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 10;
        });

        var unfiltered = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 5);
        var filtered = CreateFilter(filterOfflineMedia: true, filterOwnedMedia: true, sort: "sortIndex:asc", page: 1,
            pageSize: 5);
        var buildResults = new Queue<Result<PagedMediaQueryResult>>([
            Result.Ok(CreateResult([1, 2, 3, 4, 5])),
            Result.Ok(CreateResult([6, 7, 8, 9, 10])),
        ]);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => buildResults.Dequeue())
            .Verifiable(Times.Exactly(2));

        // Act
        var unfilteredResult = await Sut.GetMediaAsync(unfiltered, CancellationToken);
        var filteredResult = await Sut.GetMediaAsync(filtered, CancellationToken);

        // Assert
        unfilteredResult.IsSuccess.ShouldBeTrue();
        filteredResult.IsSuccess.ShouldBeTrue();
        unfilteredResult.Value.Items.Select(x => x.Id).ShouldBe([1, 2, 3, 4, 5]);
        filteredResult.Value.Items.Select(x => x.Id).ShouldBe([6, 7, 8, 9, 10]);
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldShareSingleBuild_WhenConcurrentRequestsMissSameSnapshot()
    {
        // Arrange
        await SetupDatabase(70315, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 30;
        });

        var allMovieIds = await IDbContext.PlexMovies
            .OrderBy(x => x.SearchTitle)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var firstFilter = CreateFilter(sort: "sortIndex:asc", page: 1, pageSize: 5);
        var secondFilter = CreateFilter(sort: "sortIndex:asc", page: 2, pageSize: 5);
        var buildCompletion =
            new TaskCompletionSource<Result<PagedMediaQueryResult>>(TaskCreationOptions.RunContinuationsAsynchronously);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(buildCompletion.Task)
            .Verifiable(Times.Once());

        // Act
        var firstTask = Sut.GetMediaAsync(firstFilter, CancellationToken);
        var secondTask = Sut.GetMediaAsync(secondFilter, CancellationToken);
        buildCompletion.SetResult(Result.Ok(CreateResult(allMovieIds)));
        var results = await Task.WhenAll(firstTask, secondTask);

        // Assert
        results.ShouldAllBe(x => x.IsSuccess);
        results[0].Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.Take(5));
        results[1].Value.Items.Select(x => x.Id).ShouldBe(allMovieIds.Skip(5).Take(5));
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldWarmAllLibraryMovieAndTvShowSnapshots_WhenBuildCacheRuns()
    {
        // Arrange
        await SetupDatabase(70316, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.PlexTvShowLibraryCount = 1;
            cfg.MovieCount = 2;
            cfg.TvShowCount = 2;
        });

        var capturedCommands = new List<GetMediaByTypeCommand>();
        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns<GetMediaByTypeCommand, CancellationToken>((command, _) =>
            {
                capturedCommands.Add(command);
                return Task.FromResult(Result.Ok(CreateResult([1, 2], command.Filter.MediaType)));
            })
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
        foreach (var command in capturedCommands)
        {
            command.Filter.Parameters.Page.ShouldBeNull();
            command.Filter.Parameters.PageSize.ShouldBeNull();
        }

        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldTriggerFreshBuild_WhenInvalidationClearsCachedSnapshot()
    {
        // Arrange
        await SetupDatabase(70317, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 20;
        });

        var libraryId = await IDbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);
        var freshIds = new List<int> { 50, 51, 52, 53, 54 };

        var filter = CreateFilter(plexLibraryId: libraryId, sort: "year:asc", page: 1, pageSize: 5);
        var buildResults = new Queue<Result<PagedMediaQueryResult>>([
            Result.Ok(CreateResult([1, 2, 3, 4, 5])),
            Result.Ok(CreateResult(freshIds)),
        ]);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => buildResults.Dequeue())
            .Verifiable(Times.Exactly(2));

        // Act — first call builds the snapshot
        var firstResult = await Sut.GetMediaAsync(filter, CancellationToken);
        firstResult.Value.Items.Select(x => x.Id).ShouldBe([1, 2, 3, 4, 5]);

        // Invalidate and verify second call builds fresh
        Sut.InvalidateLibraries([libraryId], "test invalidation");
        var secondResult = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        secondResult.IsSuccess.ShouldBeTrue();
        secondResult.Value.Items.Select(x => x.Id).ShouldBe(freshIds);
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldNotRebuild_WhenInvalidatingUnrelatedLibrary()
    {
        // Arrange
        await SetupDatabase(70318, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 10;
        });

        var libraryIds = await IDbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .OrderBy(x => x)
            .ToListAsync(CancellationToken);
        var unrelatedId = libraryIds[0] + libraryIds[1] + 999; // non-existent library

        var filter = CreateFilter(sort: "year:asc", page: 1, pageSize: 5);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(CreateResult([10, 20, 30, 40, 50])))
            .Verifiable(Times.Once());

        // Act
        var firstResult = await Sut.GetMediaAsync(filter, CancellationToken);
        Sut.InvalidateLibraries([unrelatedId], "unrelated invalidation");
        var secondResult = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert — should reuse cached snapshot (only one build total)
        firstResult.IsSuccess.ShouldBeTrue();
        secondResult.IsSuccess.ShouldBeTrue();
        secondResult.Value.Items.Select(x => x.Id).ShouldBe([10, 20, 30, 40, 50]);
        commandExecutor.Verify();
    }

    [Test]
    public async Task ShouldDiscardInFlightBuild_WhenInvalidationOccursDuringBuild()
    {
        // Arrange
        await SetupDatabase(70319, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 5;
        });

        var libraryId = await IDbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        // Build that will be invalidated mid-flight
        var staleBuild = new TaskCompletionSource<Result<PagedMediaQueryResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
        // Build after invalidation
        var freshBuild = new TaskCompletionSource<Result<PagedMediaQueryResult>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var buildQueue = new Queue<TaskCompletionSource<Result<PagedMediaQueryResult>>>([staleBuild, freshBuild]);

        var filter = CreateFilter(plexLibraryId: libraryId, sort: "sortIndex:asc", page: 1, pageSize: 5);

        var commandExecutor = Mock.Mock<ICommandExecutor>();
        commandExecutor
            .Setup(x => x.Send(It.IsAny<GetMediaByTypeCommand>(), It.IsAny<CancellationToken>()))
            .Returns(() => buildQueue.Dequeue().Task)
            .Verifiable(Times.Exactly(2));

        // Act — start a build, invalidate, then complete the stale build
        var firstRequest = Sut.GetMediaAsync(filter, CancellationToken);
        Sut.InvalidateLibraries([libraryId], "mid-build invalidation");
        staleBuild.SetResult(Result.Ok(CreateResult([100, 101, 102, 103, 104])));
        var staleResult = await firstRequest;

        // The stale build was discarded; next request triggers a fresh one
        freshBuild.SetResult(Result.Ok(CreateResult([200, 201, 202, 203, 204])));
        var freshResult = await Sut.GetMediaAsync(filter, CancellationToken);

        // Assert
        staleResult.IsFailed.ShouldBeTrue();
        staleResult.Errors.ShouldContain(x => x.Message.Contains("invalidated"));
        freshResult.IsSuccess.ShouldBeTrue();
        freshResult.Value.Items.Select(x => x.Id).ShouldBe([200, 201, 202, 203, 204]);
        commandExecutor.Verify();
    }

    private static MediaQueryFilter CreateFilter(
        PlexMediaType mediaType = PlexMediaType.Movie,
        int plexLibraryId = 0,
        bool filterOfflineMedia = false,
        bool filterOwnedMedia = false,
        string? query = null,
        string? filter = null,
        string? sort = "sortIndex:asc",
        int? page = 1,
        int? pageSize = 10) => new()
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
        long totalMediaSize = 0) => new()
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

    private static PlexMediaSlimDTO CreateItem(int id, int index, PlexMediaType mediaType) => new()
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