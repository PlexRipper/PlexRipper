using FlexQuery.NET.Models;

namespace Reaparr.Data.UnitTests;

public class GetMediaByTypeCommandHandlerUnitTests : BaseUnitTest<GetMediaByTypeCommandHandler>
{
    [Test]
    public async Task ShouldReturnOnlyMoviesFromSpecificLibrary_WhenPlexLibraryIdIsSet()
    {
        // Arrange
        await SetupDatabase(70001, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 5;
            cfg.MovieCount = 6;
        });

        var dbContext = IDbContext;
        var targetLibraryId = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, targetLibraryId);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.ShouldAllBe(x => x.PlexLibraryId == targetLibraryId);
    }

    [Test]
    public async Task ShouldReturnOnlyTvShowsFromSpecificLibrary_WhenPlexLibraryIdIsSet()
    {
        // Arrange
        await SetupDatabase(70002, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexTvShowLibraryCount = 2;
            cfg.TvShowCount = 6;
        });

        var dbContext = IDbContext;
        var targetLibraryId = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.TvShow)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        var command = CreateCommand(PlexMediaType.TvShow, targetLibraryId);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.ShouldAllBe(x => x.PlexLibraryId == targetLibraryId);
    }

    [Test]
    public async Task ShouldReturnMoviesFromMultipleLibraries_WhenPlexLibraryIdIsZero()
    {
        // Arrange
        await SetupDatabase(70003, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 4;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.Select(x => x.PlexLibraryId).Distinct().Count().ShouldBe(2);
    }

    [Test]
    public async Task ShouldExcludeOwnedLibraries_WhenFilterOwnedMediaIsTrueForAllLibraries()
    {
        // Arrange
        await SetupDatabase(70004, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 4;
            cfg.PlexAccountCount = 1;
        });

        var dbContext = IDbContext;
        var libraryToKeep = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        await dbContext.PlexAccountLibraries
            .Where(x => x.PlexLibraryId == libraryToKeep)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsLibraryOwned, false), CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, 0, filterOwnedMedia: true);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.ShouldAllBe(x => x.PlexLibraryId == libraryToKeep);
    }

    [Test]
    public async Task ShouldReturnEmpty_WhenAllLibrariesAreOwnedAndFilterOwnedMediaIsTrueForAllLibraries()
    {
        // Arrange
        await SetupDatabase(70005, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 4;
            cfg.PlexAccountCount = 1;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0, filterOwnedMedia: true);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldExcludeOfflineServerLibraries_WhenFilterOfflineMediaIsTrueForAllLibraries()
    {
        // Arrange
        await SetupDatabase(70006, cfg =>
        {
            cfg.PlexServerCount = 2;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 3;
        });

        var dbContext = IDbContext;
        var serverIds = await dbContext.PlexServers
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);
        var offlineServerId = serverIds.Last();

        await dbContext.PlexServerStatuses
            .Where(x => x.PlexServerId == offlineServerId)
            .ExecuteDeleteAsync(CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, 0, filterOfflineMedia: true);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.ShouldAllBe(x => x.PlexServerId != offlineServerId);
    }

    [Test]
    public async Task ShouldReturnEmpty_WhenAllServersAreOfflineAndFilterOfflineMediaIsTrueForAllLibraries()
    {
        // Arrange
        await SetupDatabase(70007, cfg =>
        {
            cfg.PlexServerCount = 2;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 3;
        });

        var dbContext = IDbContext;
        await dbContext.PlexServerStatuses.ExecuteDeleteAsync(CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, 0, filterOfflineMedia: true);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldIgnoreOwnedFilter_WhenSpecificPlexLibraryIdIsSet()
    {
        // Arrange
        await SetupDatabase(70008, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 4;
            cfg.PlexAccountCount = 1;
        });

        var dbContext = IDbContext;
        var targetLibraryId = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, targetLibraryId, filterOwnedMedia: true);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.ShouldAllBe(x => x.PlexLibraryId == targetLibraryId);
    }

    [Test]
    public async Task ShouldIgnoreOfflineFilter_WhenSpecificPlexLibraryIdIsSet()
    {
        // Arrange
        await SetupDatabase(70009, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 4;
        });

        var dbContext = IDbContext;
        await dbContext.PlexServerStatuses.ExecuteDeleteAsync(CancellationToken);

        var targetLibraryId = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, targetLibraryId, filterOfflineMedia: true);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.ShouldAllBe(x => x.PlexLibraryId == targetLibraryId);
    }

    [Test]
    public async Task ShouldFail_WhenUnsupportedMediaTypeIsRequested()
    {
        // Arrange
        await SetupDatabase(70010, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 2;
        });

        var command = CreateCommand(PlexMediaType.Season, 0);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(x => x.Message.Contains("not supported"));
    }

    [Test]
    public async Task ShouldReturnEmpty_WhenNoAllowedLibrariesExistInAllLibraryMode()
    {
        // Arrange
        await SetupDatabase(70011);

        var command = CreateCommand(PlexMediaType.Movie, 0);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldAssignSortIndexesSequentiallyForMovies_WhenResultsReturned()
    {
        // Arrange
        await SetupDatabase(70012, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 6;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.Select(x => x.SortIndex).ShouldBe(Enumerable.Range(1, result.Value.Items.Count));
    }

    [Test]
    public async Task ShouldAssignSortIndexesSequentiallyForTvShows_WhenResultsReturned()
    {
        // Arrange
        await SetupDatabase(70013, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexTvShowLibraryCount = 1;
            cfg.TvShowCount = 6;
        });

        var command = CreateCommand(PlexMediaType.TvShow, 0);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.Select(x => x.SortIndex).ShouldBe(Enumerable.Range(1, result.Value.Items.Count));
    }

    [Test]
    public async Task ShouldRespectPaging_WhenPageAndPageSizeAreProvided()
    {
        // Arrange
        await SetupDatabase(70014, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 8;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0, page: 1, pageSize: 3);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(3);
    }

    [Test]
    public async Task ShouldReturnPageAndPageSize_WhenPageAndPageSizeAreProvided()
    {
        // Arrange
        await SetupDatabase(70101, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 8;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0, page: 2, pageSize: 3);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Page.ShouldBe(2);
        result.Value.PageSize.ShouldBe(3);
    }

    [Test]
    public async Task ShouldReturnTotalMatchingCount_WhenPagingIsApplied()
    {
        // Arrange
        await SetupDatabase(70102, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 8;
        });

        var expectedMovieCount = await IDbContext.PlexMovies.CountAsync(CancellationToken);
        var command = CreateCommand(PlexMediaType.Movie, 0, page: 1, pageSize: 3);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(3);
        result.Value.TotalCount.ShouldBe(expectedMovieCount);
        result.Value.MediaCount.ShouldBe(expectedMovieCount);
    }

    [Test]
    public async Task ShouldCountOnlyRequestedMediaType_WhenAllLibraryModeHasMultipleServersAndMixedLibraries()
    {
        // Arrange
        await SetupDatabase(70103, cfg =>
        {
            cfg.PlexServerCount = 2;
            cfg.PlexMovieLibraryCount = 2;
            cfg.PlexTvShowLibraryCount = 2;
            cfg.MovieCount = 3;
            cfg.TvShowCount = 4;
        });

        var expectedMovieCount = await IDbContext.PlexMovies.CountAsync(CancellationToken);
        var command = CreateCommand(PlexMediaType.Movie, 0, page: 1, pageSize: 2);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.Items.ShouldAllBe(x => x.Type == PlexMediaType.Movie);
        result.Value.TotalCount.ShouldBe(expectedMovieCount);
        result.Value.MediaCount.ShouldBe(expectedMovieCount);
        result.Value.MovieCount.ShouldBe(expectedMovieCount);
        result.Value.TvShowCount.ShouldBe(0);
    }

    [Test]
    public async Task ShouldAssignGlobalSortIndexes_WhenPageChanges()
    {
        // Arrange
        await SetupDatabase(70104, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 10;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0, page: 2, pageSize: 3);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Select(x => x.SortIndex).ShouldBe([4, 5, 6]);
    }

    [Test]
    public async Task ShouldBuildNavigationIndexesFromFullSortedResult_WhenPageIsPartial()
    {
        // Arrange
        await SetupDatabase(70105, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 10;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0, sort: "Year:asc", page: 1, pageSize: 2);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBe(2);
        result.Value.NavigationIndexes.ShouldNotBeEmpty();
        result.Value.NavigationIndexes.Max(x => x.Index).ShouldBeLessThan(result.Value.TotalCount);
    }

    [Test]
    public async Task ShouldReturnDifferentPageData_WhenPageChanges()
    {
        // Arrange
        await SetupDatabase(70015, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 10;
        });

        var page1 = CreateCommand(PlexMediaType.Movie, 0, page: 1, pageSize: 3);
        var page2 = CreateCommand(PlexMediaType.Movie, 0, page: 2, pageSize: 3);

        // Act
        var sutForPage1 = Mock.Create<GetMediaByTypeCommandHandler>();
        var resultPage1 = await sutForPage1.ExecuteAsync(page1, CancellationToken);
        var page1Ids = resultPage1.Value.Items.Select(x => x.Id).ToList();

        var sutForPage2 = Mock.Create<GetMediaByTypeCommandHandler>();
        var resultPage2 = await sutForPage2.ExecuteAsync(page2, CancellationToken);
        var page2Ids = resultPage2.Value.Items.Select(x => x.Id).ToList();

        // Assert
        resultPage1.IsSuccess.ShouldBeTrue();
        resultPage2.IsSuccess.ShouldBeTrue();
        page1Ids.ShouldNotBe(page2Ids);
    }

    [Test]
    public async Task ShouldReturnEmpty_WhenSpecificLibraryIdDoesNotExist()
    {
        // Arrange
        await SetupDatabase(70016, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 4;
        });

        var command = CreateCommand(PlexMediaType.Movie, 999999);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(0);
        result.Value.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldReturnOnlyMovies_WhenMovieTypeRequestedAndBothMediaTypesExist()
    {
        // Arrange
        await SetupDatabase(70017, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.PlexTvShowLibraryCount = 1;
            cfg.MovieCount = 5;
            cfg.TvShowCount = 5;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.ShouldAllBe(x => x.Type == PlexMediaType.Movie);
    }

    [Test]
    public async Task ShouldReturnOnlyTvShows_WhenTvShowTypeRequestedAndBothMediaTypesExist()
    {
        // Arrange
        await SetupDatabase(70018, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.PlexTvShowLibraryCount = 1;
            cfg.MovieCount = 5;
            cfg.TvShowCount = 5;
        });

        var command = CreateCommand(PlexMediaType.TvShow, 0);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.ShouldAllBe(x => x.Type == PlexMediaType.TvShow);
    }

    [Test]
    public async Task ShouldNotExcludeOwnedLibraries_WhenFilterOwnedMediaIsFalse()
    {
        // Arrange
        await SetupDatabase(70019, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 4;
            cfg.PlexAccountCount = 1;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0, filterOwnedMedia: false);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.Select(x => x.PlexLibraryId).Distinct().Count().ShouldBe(2);
    }

    [Test]
    public async Task ShouldNotExcludeOfflineLibraries_WhenFilterOfflineMediaIsFalse()
    {
        // Arrange
        await SetupDatabase(70020, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 4;
        });

        var dbContext = IDbContext;
        await dbContext.PlexServerStatuses.ExecuteDeleteAsync(CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, 0, filterOfflineMedia: false);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
    }

    [Test]
    public async Task ShouldReturnEmpty_WhenSpecificLibraryExistsButForDifferentMediaType()
    {
        // Arrange
        await SetupDatabase(70021, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.PlexTvShowLibraryCount = 1;
            cfg.MovieCount = 4;
            cfg.TvShowCount = 4;
        });

        var dbContext = IDbContext;
        var movieLibraryId = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        var command = CreateCommand(PlexMediaType.TvShow, movieLibraryId);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldReturnEmpty_WhenRequestedPageIsBeyondAvailableRange()
    {
        // Arrange
        await SetupDatabase(70022, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 3;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0, page: 5, pageSize: 10);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
        result.Value.TotalCount.ShouldBe(3);
        result.Value.MediaCount.ShouldBe(3);
        result.Value.MovieCount.ShouldBe(3);
    }

    [Test]
    public async Task ShouldTreatServerAsOffline_WhenOnlyUnsuccessfulStatusesExistAndFilterOfflineMediaIsTrue()
    {
        // Arrange
        await SetupDatabase(70023, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 5;
        });

        var dbContext = IDbContext;
        await dbContext.PlexServerStatuses
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsSuccessful, false), CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, 0, filterOfflineMedia: true);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldApplyProvidedSort_WhenSortExpressionIsSet()
    {
        // Arrange
        await SetupDatabase(70024, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 10;
        });

        var command = CreateCommand(PlexMediaType.Movie, 0, sort: "Year:desc", pageSize: 10);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Count.ShouldBeGreaterThanOrEqualTo(2);
        result.Value.Items.Zip(result.Value.Items.Skip(1))
            .ShouldAllBe(x => x.First.Year >= x.Second.Year);
    }

    [Test]
    public async Task ShouldSortAllLibraryMoviesBySearchTitle_WhenSortIndexSortIsRequested()
    {
        // Arrange
        await SetupDatabase(70233, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 4;
        });

        var dbContext = IDbContext;
        var movieIds = await dbContext.PlexMovies
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        await SetAllLibrarySortIndexRegressionDataAsync(dbContext, movieIds);

        var command = CreateCommand(PlexMediaType.Movie, 0, sort: "sortIndex:asc", pageSize: 10);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Select(x => x.Title).ShouldBe([
            "Alpha",
            "Bravo",
            "Charlie",
            "Delta",
            "Whiskey",
            "Xray",
            "Yankee",
            "Zulu",
        ]);
        result.Value.NavigationIndexes.Select(x => x.Label).ShouldBe(["A", "B", "C", "D", "W", "X", "Y", "Z"]);
    }

    [Test]
    public async Task ShouldSortMoviesByHighestQuality_WhenLegacyQualitySortFieldIsUsed()
    {
        // Arrange
        await SetupDatabase(70030, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 3;
        });

        var dbContext = IDbContext;
        var movieIds = await dbContext.PlexMovies
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        await SetMovieQualitySortTestDataAsync(dbContext, movieIds);

        var command = CreateCommand(PlexMediaType.Movie, 0, sort: "quality:asc", pageSize: 10);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Select(x => x.Title).ShouldBe(["SD Movie", "HD Movie", "4K Movie"]);
    }

    [Test]
    public async Task ShouldSortMoviesByHighestQualityDescending_WhenCanonicalHighestQualitySortFieldIsUsed()
    {
        // Arrange
        await SetupDatabase(70031, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 3;
        });

        var dbContext = IDbContext;
        var movieIds = await dbContext.PlexMovies
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        await SetMovieQualitySortTestDataAsync(dbContext, movieIds);

        var command = CreateCommand(PlexMediaType.Movie, 0, sort: "quality:desc", pageSize: 10);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Select(x => x.Title).ShouldBe(["4K Movie", "HD Movie", "SD Movie"]);
    }

    [Test]
    public async Task ShouldSortTvShowsByHighestQuality_WhenLegacyQualitySortFieldIsUsed()
    {
        // Arrange
        await SetupDatabase(70032, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexTvShowLibraryCount = 1;
            cfg.TvShowCount = 3;
        });

        var dbContext = IDbContext;
        var tvShowIds = await dbContext.PlexTvShows
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .ToListAsync(CancellationToken);

        await SetTvShowQualitySortTestDataAsync(dbContext, tvShowIds);

        var command = CreateCommand(PlexMediaType.TvShow, 0, sort: "quality:asc", pageSize: 10);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.Select(x => x.Title).ShouldBe(["SD Show", "HD Show", "4K Show"]);
    }

    [Test]
    public async Task ShouldReturnOnlyRequestedLibrary_WhenSpecificLibraryIdAndSortProvided()
    {
        // Arrange
        await SetupDatabase(70025, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 5;
        });

        var dbContext = IDbContext;
        var targetLibraryId = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .OrderBy(x => x.Id)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        var command = CreateCommand(
            PlexMediaType.Movie,
            targetLibraryId,
            sort: "Year:asc",
            pageSize: 10
        );

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldNotBeEmpty();
        result.Value.Items.ShouldAllBe(x => x.PlexLibraryId == targetLibraryId);
    }

    [Test]
    public async Task ShouldUseLibraryCounts_WhenNoFilterOrSearchAndAllLibrariesRequested()
    {
        // Arrange
        await SetupDatabase(70026, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 6;
        });

        var dbContext = IDbContext;
        var expectedMovieCount = await dbContext.PlexMovies.CountAsync(CancellationToken);
        var expectedMediaSize = await dbContext.PlexMovies.SumAsync(x => x.MediaSize, CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, 0);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.TotalCount.ShouldBe(expectedMovieCount);
        result.Value.MediaCount.ShouldBe(expectedMovieCount);
        result.Value.MovieCount.ShouldBe(expectedMovieCount);
        result.Value.TvShowCount.ShouldBe(0);
        result.Value.SeasonCount.ShouldBe(0);
        result.Value.EpisodeCount.ShouldBe(0);
        result.Value.MediaSize.ShouldBe(expectedMediaSize);
    }

    [Test]
    public async Task ShouldUseRequestedLibraryCounts_WhenNoFilterOrSearchAndSpecificLibraryRequested()
    {
        // Arrange
        await SetupDatabase(70027, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 2;
            cfg.MovieCount = 6;
        });

        var dbContext = IDbContext;
        var targetLibrary = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
            })
            .FirstAsync(CancellationToken);
        var expectedMovieCount = await dbContext.PlexMovies
            .Where(x => x.PlexLibraryId == targetLibrary.Id)
            .CountAsync(CancellationToken);
        var expectedMediaSize = await dbContext.PlexMovies
            .Where(x => x.PlexLibraryId == targetLibrary.Id)
            .SumAsync(x => x.MediaSize, CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, targetLibrary.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldAllBe(x => x.PlexLibraryId == targetLibrary.Id);
        result.Value.TotalCount.ShouldBe(expectedMovieCount);
        result.Value.MediaCount.ShouldBe(expectedMovieCount);
        result.Value.MovieCount.ShouldBe(expectedMovieCount);
        result.Value.TvShowCount.ShouldBe(0);
        result.Value.SeasonCount.ShouldBe(0);
        result.Value.EpisodeCount.ShouldBe(0);
        result.Value.MediaSize.ShouldBe(expectedMediaSize);
    }

    [Test]
    public async Task ShouldCalculateMediaSizeFromMovieRows_WhenLibraryMediaSizeSnapshotIsStale()
    {
        // Arrange
        await SetupDatabase(70232, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 6;
        });

        var dbContext = IDbContext;
        var targetLibrary = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.Movie)
            .OrderBy(x => x.Id)
            .FirstAsync(CancellationToken);
        var expectedMediaSize = await dbContext.PlexMovies
            .Where(x => x.PlexLibraryId == targetLibrary.Id)
            .SumAsync(x => x.MediaSize, CancellationToken);
        await dbContext.PlexLibraries
            .Where(x => x.Id == targetLibrary.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.MediaSize, 0), CancellationToken);

        var command = CreateCommand(PlexMediaType.Movie, targetLibrary.Id);

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.Items.ShouldAllBe(x => x.PlexLibraryId == targetLibrary.Id);
        result.Value.MediaSize.ShouldBe(expectedMediaSize);
        result.Value.TotalMediaSize.ShouldBe(expectedMediaSize);
        result.Value.MediaSize.ShouldBeGreaterThan(0);
    }

    [Test]
    public async Task ShouldFilterMoviesByCountryId_WhenMetadataCountryFilterIsApplied()
    {
        // Arrange
        await SetupDatabase(70028, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 6;
        });

        var dbContext = IDbContext;
        var expectedMovie = await ConfigureExactMetadataMatchAsync(dbContext, VideoQuality.FullHD);

        var command = CreateCommand(
            PlexMediaType.Movie,
            0,
            filter: $"Countries:any:Id:eq:{expectedMovie.CountryId}"
        );

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailed.ShouldBeFalse();
        AssertReturnedExactMovies(result.Value, [expectedMovie.MovieId, expectedMovie.CountryOnlyMovieId]);
    }

    [Test]
    public async Task ShouldFilterMoviesByRoleId_WhenMetadataRoleFilterIsApplied()
    {
        // Arrange
        await SetupDatabase(70029, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 6;
        });

        var dbContext = IDbContext;
        var expectedMovie = await ConfigureExactMetadataMatchAsync(dbContext, VideoQuality.FullHD);

        var command = CreateCommand(
            PlexMediaType.Movie,
            0,
            filter: $"Actors:any:Id:eq:{expectedMovie.ActorId}"
        );

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailed.ShouldBeFalse();
        AssertReturnedExactMovies(result.Value, [expectedMovie.MovieId, expectedMovie.ActorOnlyMovieId]);
    }

    [Test]
    public async Task ShouldFilterMoviesByGenreId_WhenMetadataGenreFilterIsApplied()
    {
        // Arrange
        await SetupDatabase(70230, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 6;
        });

        var dbContext = IDbContext;
        var expectedMovie = await ConfigureExactMetadataMatchAsync(dbContext, VideoQuality.FullHD);

        var command = CreateCommand(
            PlexMediaType.Movie,
            0,
            filter: $"Genres:any:Id:eq:{expectedMovie.GenreId}"
        );

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailed.ShouldBeFalse();
        AssertReturnedExactMovies(result.Value, [expectedMovie.MovieId, expectedMovie.GenreOnlyMovieId]);
    }

    [Test]
    public async Task ShouldFilterMoviesByQualityGenreCountryAndRole_WhenAllMetadataFiltersAreApplied()
    {
        // Arrange
        await SetupDatabase(70231, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 6;
        });

        var dbContext = IDbContext;
        var expectedMovie = await ConfigureExactMetadataMatchAsync(dbContext, VideoQuality.FullHD);

        var filter = string.Join(
            "&",
            $"MediaDataList:any:Quality:eq:{VideoQuality.FullHD}",
            $"Genres:any:Id:eq:{expectedMovie.GenreId}",
            $"Countries:any:Id:eq:{expectedMovie.CountryId}",
            $"Actors:any:Id:eq:{expectedMovie.ActorId}"
        );

        var command = CreateCommand(
            PlexMediaType.Movie,
            0,
            pageSize: 20,
            filter: filter
        );

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.IsFailed.ShouldBeFalse();
        AssertReturnedExactMovies(result.Value, [expectedMovie.MovieId]);
        result.Value.Items.ShouldAllBe(x => x.Qualities.Any(y => y.Quality == VideoQuality.FullHD));

        var returnedMovieId = result.Value.Items.Single().Id;
        var hasMatchingGenre = await dbContext.PlexMovieGenres
            .AnyAsync(x => x.PlexMovieId == returnedMovieId && x.GenresId == expectedMovie.GenreId, CancellationToken);
        var hasMatchingCountry = await dbContext.PlexMovieCountries
            .AnyAsync(x => x.PlexMovieId == returnedMovieId && x.CountryId == expectedMovie.CountryId, CancellationToken);
        var hasMatchingActor = await dbContext.PlexMovieActors
            .AnyAsync(x => x.PlexMovieId == returnedMovieId && x.PlexActorId == expectedMovie.ActorId, CancellationToken);

        hasMatchingGenre.ShouldBeTrue();
        hasMatchingCountry.ShouldBeTrue();
        hasMatchingActor.ShouldBeTrue();
        result.Value.Genres.ShouldBe([expectedMovie.GenreId]);
        result.Value.Countries.ShouldBe([expectedMovie.CountryId]);
        result.Value.Roles.ShouldBe([expectedMovie.ActorId]);
        result.Value.Qualities.ShouldBe([7]);
    }

    private async Task<ExpectedMovieMetadata> ConfigureExactMetadataMatchAsync(
        IReaparrDbContext dbContext,
        VideoQuality targetQuality
    )
    {
        var movies = await dbContext.PlexMovies
            .OrderBy(x => x.Id)
            .Select(x => new
            {
                x.Id,
                x.PlexLibraryId,
                x.PlexServerId,
            })
            .ToListAsync(CancellationToken);

        movies.Count.ShouldBeGreaterThanOrEqualTo(4);

        var expectedMovie = movies[0];
        var genreOnlyMovie = movies[1];
        var countryOnlyMovie = movies[2];
        var actorOnlyMovie = movies[3];

        var genre = new PlexGenre { Name = "Regression Genre", Key = "regression-genre" };
        var country = new PlexCountry { Name = "Regression Country", Key = "regression-country" };
        var actor = new PlexActor { Name = "Regression Actor", Key = "regression-actor" };

        dbContext.PlexGenres.Add(genre);
        dbContext.PlexCountries.Add(country);
        dbContext.PlexActors.Add(actor);
        await dbContext.SaveChangesAsync(CancellationToken);

        dbContext.PlexMovieGenres.AddRange(
            new PlexMovieGenres(genre.Id, expectedMovie.PlexLibraryId, expectedMovie.Id),
            new PlexMovieGenres(genre.Id, genreOnlyMovie.PlexLibraryId, genreOnlyMovie.Id)
        );
        dbContext.PlexMovieCountries.AddRange(
            new PlexMovieCountries(country.Id, expectedMovie.PlexLibraryId, expectedMovie.Id),
            new PlexMovieCountries(country.Id, countryOnlyMovie.PlexLibraryId, countryOnlyMovie.Id)
        );
        dbContext.PlexMovieActors.AddRange(
            new PlexMovieActors(actor.Id, expectedMovie.PlexLibraryId, expectedMovie.Id),
            new PlexMovieActors(actor.Id, actorOnlyMovie.PlexLibraryId, actorOnlyMovie.Id)
        );

        await dbContext.SaveChangesAsync(CancellationToken);

        await dbContext.PlexMovieData
            .Where(x => x.PlexMovieId == expectedMovie.Id)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(y => y.Quality, targetQuality)
                    .SetProperty(y => y.VideoResolution, targetQuality),
                CancellationToken
            );

        await dbContext.PlexMovieData
            .Where(x => x.PlexMovieId != expectedMovie.Id)
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(y => y.Quality, VideoQuality.HD)
                    .SetProperty(y => y.VideoResolution, VideoQuality.HD),
                CancellationToken
            );

        return new ExpectedMovieMetadata(
            expectedMovie.Id,
            genreOnlyMovie.Id,
            countryOnlyMovie.Id,
            actorOnlyMovie.Id,
            genre.Id,
            country.Id,
            actor.Id
        );
    }

    private async Task SetAllLibrarySortIndexRegressionDataAsync(IReaparrDbContext dbContext, IReadOnlyList<int> movieIds)
    {
        movieIds.Count.ShouldBeGreaterThanOrEqualTo(8);

        var titlesById = new[]
        {
            (movieIds[0], "Zulu", 1),
            (movieIds[1], "Alpha", 1),
            (movieIds[2], "Charlie", 2),
            (movieIds[3], "Bravo", 2),
            (movieIds[4], "Yankee", 3),
            (movieIds[5], "Delta", 3),
            (movieIds[6], "Xray", 4),
            (movieIds[7], "Whiskey", 4),
        };

        foreach (var (movieId, title, sortIndex) in titlesById)
        {
            await dbContext.PlexMovies
                .Where(x => x.Id == movieId)
                .ExecuteUpdateAsync(
                    x => x
                        .SetProperty(y => y.Title, title)
                        .SetProperty(y => y.SearchTitle, title.ToLowerInvariant())
                        .SetProperty(y => y.SortIndex, sortIndex),
                    CancellationToken
                );
        }
    }

    private async Task SetMovieQualitySortTestDataAsync(IReaparrDbContext dbContext, IReadOnlyList<int> movieIds)
    {
        await dbContext.PlexMovies
            .Where(x => x.Id == movieIds[0])
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(y => y.Title, "SD Movie")
                    .SetProperty(y => y.Quality, VideoQuality.SD),
                CancellationToken
            );

        await dbContext.PlexMovies
            .Where(x => x.Id == movieIds[1])
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(y => y.Title, "4K Movie")
                    .SetProperty(y => y.Quality, VideoQuality.UHD_4K),
                CancellationToken
            );

        await dbContext.PlexMovies
            .Where(x => x.Id == movieIds[2])
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(y => y.Title, "HD Movie")
                    .SetProperty(y => y.Quality, VideoQuality.FullHD),
                CancellationToken
            );
    }

    private async Task SetTvShowQualitySortTestDataAsync(IReaparrDbContext dbContext, IReadOnlyList<int> tvShowIds)
    {
        await dbContext.PlexTvShows
            .Where(x => x.Id == tvShowIds[0])
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(y => y.Title, "SD Show")
                    .SetProperty(y => y.Quality, VideoQuality.SD),
                CancellationToken
            );

        await dbContext.PlexTvShows
            .Where(x => x.Id == tvShowIds[1])
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(y => y.Title, "4K Show")
                    .SetProperty(y => y.Quality, VideoQuality.UHD_4K),
                CancellationToken
            );

        await dbContext.PlexTvShows
            .Where(x => x.Id == tvShowIds[2])
            .ExecuteUpdateAsync(
                x => x
                    .SetProperty(y => y.Title, "HD Show")
                    .SetProperty(y => y.Quality, VideoQuality.FullHD),
                CancellationToken
            );
    }

    [Test]
    public async Task ShouldSetRequestHashFromFilter_WhenQueryReturnsResults()
    {
        // Arrange
        await SetupDatabase(70231, cfg =>
        {
            cfg.PlexServerCount = 1;
            cfg.PlexMovieLibraryCount = 1;
            cfg.MovieCount = 4;
        });

        var command = CreateCommand(
            PlexMediaType.Movie,
            0,
            filterOfflineMedia: true,
            page: 2,
            pageSize: 3,
            sort: "Year:desc",
            filter: "Year:gte:2000"
        );

        // Act
        var result = await Sut.ExecuteAsync(command, CancellationToken);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.QueryHash.ShouldBe(command.Filter.QueryHash);
    }

    private static void AssertReturnedExactMovies(PagedMediaQueryResult result, IReadOnlyCollection<int> expectedMovieIds)
    {
        expectedMovieIds.ShouldNotBeEmpty();
        result.Items.ShouldNotBeEmpty();
        result.Items.Select(x => x.Id).OrderBy(x => x).ShouldBe(expectedMovieIds.OrderBy(x => x));
        result.TotalCount.ShouldBe(expectedMovieIds.Count);
        result.MediaCount.ShouldBe(expectedMovieIds.Count);
        result.MovieCount.ShouldBe(expectedMovieIds.Count);
        result.TvShowCount.ShouldBe(0);
        result.SeasonCount.ShouldBe(0);
        result.EpisodeCount.ShouldBe(0);
    }

    private sealed record ExpectedMovieMetadata(
        int MovieId,
        int GenreOnlyMovieId,
        int CountryOnlyMovieId,
        int ActorOnlyMovieId,
        int GenreId,
        int CountryId,
        int ActorId
    );

    private static GetMediaByTypeCommand CreateCommand(
        PlexMediaType mediaType,
        int plexLibraryId,
        bool filterOfflineMedia = false,
        bool filterOwnedMedia = false,
        int? page = null,
        int? pageSize = null,
        string? sort = null,
        string? filter = null
    ) => new()
    {
        Filter = new MediaQueryFilter
        {
            MediaType = mediaType,
            PlexLibraryId = plexLibraryId,
            FilterOfflineMedia = filterOfflineMedia,
            FilterOwnedMedia = filterOwnedMedia,
            Parameters = new FlexQueryParameters
            {
                Page = page,
                PageSize = pageSize,
                Sort = sort,
                Filter = filter,
            },
        },
    };
}
