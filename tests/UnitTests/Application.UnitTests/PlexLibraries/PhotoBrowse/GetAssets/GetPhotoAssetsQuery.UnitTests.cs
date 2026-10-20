namespace Reaparr.Application.UnitTests;

public class GetPhotoAssetsQueryUnitTests : BaseCommandUnitTest<GetPhotoAssetsQuery>
{
    [Test]
    public async Task ShouldReturnUniqueAggregateAssetsIncludingUnmappedFiles()
    {
        // Arrange
        var fixture = await ArrangeUniqueAssetFixtureAsync();

        // Act
        var result = await TestHandlerExecuteAsync<PhotoAssetPageDTO>(
            new GetPhotoAssetsQuery(null, 1, 100, "title:asc")
        );
        var scopedResult = await TestHandlerExecuteAsync<PhotoAssetPageDTO>(
            new GetPhotoAssetsQuery(fixture.LibraryId, 1, 100, "title:asc")
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        scopedResult.IsSuccess.ShouldBeTrue();
        scopedResult.Errors.Count.ShouldBe(0);
        result.Value.TotalCount.ShouldBe(fixture.Assets.Count);
        result.Value.TotalBytes.ShouldBe(fixture.ExpectedBytes);
        result.Value.Items.Select(x => x.Id).ShouldBe(fixture.Assets.Select(x => x.Id));
        result.Value.Items.Select(x => x.Id).Distinct().Count().ShouldBe(result.Value.Items.Count);
        result.Value.Items.Single(x => x.Id == fixture.UnmappedAsset.Id).RelativeFolderPath.ShouldBeNull();
        result.Value.Items.Single(x => x.Id == fixture.UnmappedAsset.Id).SourceRootTitle.ShouldBeNull();
        result.Value.Items.Single(x => x.Id == fixture.InactiveAsset.Id).SourceRootTitle.ShouldBe(
            fixture.InactiveRoot.Title
        );
        scopedResult.Value.Items.Single(x => x.Id == fixture.InactiveAsset.Id).SourceRootTitle.ShouldBe(
            fixture.InactiveRoot.Title
        );
        result.Value.Items.Select(x => (x.PlexServerId, x.PlexLibraryId, x.Id)).Distinct().Count().ShouldBe(
            result.Value.Items.Count
        );
    }
 
    private async Task<UniqueAssetFixture> ArrangeUniqueAssetFixtureAsync()
    {
        await SetupDatabase(71002, config =>
        {
            config.PlexPhotoLibraryCount = 2;
            config.PhotoAlbumCount = 2;
            config.PhotoCount = 3;
            config.PhotoClipCount = 1;
        });
        var dbContext = IDbContext;
        var assets = await dbContext.PlexPhotoMediaData
            .OrderBy(x => x.Title)
            .ThenBy(x => x.Id)
            .ToListAsync(CancellationToken);
        assets.Count.ShouldBeGreaterThan(0);
        var library = await dbContext.PlexLibraries
            .Where(x => x.Type == PlexMediaType.PhotoAlbum)
            .OrderBy(x => x.Id)
            .FirstAsync(CancellationToken);
        var libraryAssets = assets.Where(x => x.PlexLibraryId == library.Id).ToList();
        libraryAssets.Count.ShouldBeGreaterThan(1);
        var unmappedAsset = libraryAssets[0];
        var inactiveAsset = libraryAssets[1];
        var inactiveRoot = new PlexPhotoFolder
        {
            PlexServerId = library.PlexServerId,
            PlexLibraryId = library.Id,
            RelativePath = string.Empty,
            Title = "Inactive root",
            RootIdentityKey = "path:inactive",
            SourceRootPath = "/inactive",
            PlexApiLocationId = 9001,
            IsActive = false,
        };
        dbContext.PlexPhotoFolders.Add(inactiveRoot);
        await dbContext.SaveChangesAsync(CancellationToken);
        var inactiveChild = new PlexPhotoFolder
        {
            PlexServerId = library.PlexServerId,
            PlexLibraryId = library.Id,
            ParentId = inactiveRoot.Id,
            RelativePath = "inactive-root/child",
            Title = "Inactive child",
        };
        dbContext.PlexPhotoFolders.Add(inactiveChild);
        await dbContext.SaveChangesAsync(CancellationToken);
        await dbContext.PlexPhotoMediaData
            .Where(x => x.Id == inactiveAsset.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.PlexPhotoFolderId, inactiveChild.Id), CancellationToken);
        inactiveAsset.PlexPhotoFolderId = inactiveChild.Id;
        await dbContext.PlexPhotoMediaData
            .Where(x => x.Id == unmappedAsset.Id)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.PlexPhotoFolderId, (int?)null), CancellationToken);
        unmappedAsset.PlexPhotoFolderId = null;

        return new UniqueAssetFixture(
            library.Id,
            assets,
            assets.Sum(x => x.Size),
            unmappedAsset,
            inactiveAsset,
            inactiveRoot
        );
    }

    private sealed record UniqueAssetFixture(
        int LibraryId,
        IReadOnlyList<PlexPhotoMediaData> Assets,
        long ExpectedBytes,
        PlexPhotoMediaData UnmappedAsset,
        PlexPhotoMediaData InactiveAsset,
        PlexPhotoFolder InactiveRoot
    );

    [Test]
    public async Task ShouldSortNaturalTiesInBothDirectionsAndReturnEmptyBoundedPages()
    {
        // Arrange
        await SetupDatabase(71009, config =>
        {
            config.PlexPhotoLibraryCount = 1;
            config.PhotoAlbumCount = 1;
            config.PhotoCount = 4;
        });
        var dbContext = IDbContext;
        await dbContext.PlexPhotoMediaData.ExecuteUpdateAsync(
            x => x.SetProperty(y => y.Title, "Same title"),
            CancellationToken
        );
        var assets = await dbContext.PlexPhotoMediaData.OrderBy(x => x.Id).ToListAsync(CancellationToken);

        // Act
        var ascending = await TestHandlerExecuteAsync<PhotoAssetPageDTO>(
            new GetPhotoAssetsQuery(null, 1, 1, "title:asc")
        );
        var descending = await TestHandlerExecuteAsync<PhotoAssetPageDTO>(
            new GetPhotoAssetsQuery(null, 1, 1, "title:desc")
        );
        var emptyPage = await TestHandlerExecuteAsync<PhotoAssetPageDTO>(
            new GetPhotoAssetsQuery(null, 100, 2, "title:asc")
        );
        var invalidSize = await TestHandlerExecuteAsync<PhotoAssetPageDTO>(
            new GetPhotoAssetsQuery(null, 1, MediaQueryFilter.MaximumPageSize + 1, "title:asc")
        );
        var overflowPage = await TestHandlerExecuteAsync<PhotoAssetPageDTO>(
            new GetPhotoAssetsQuery(null, int.MaxValue, MediaQueryFilter.MaximumPageSize, "title:asc")
        );

        // Assert
        ascending.Value.Items.Select(x => x.Id).ShouldBe([assets[0].Id]);
        descending.Value.Items.Select(x => x.Id).ShouldBe([assets[^1].Id]);
        emptyPage.IsSuccess.ShouldBeTrue();
        emptyPage.Value.Items.ShouldBeEmpty();
        emptyPage.Value.TotalCount.ShouldBe(assets.Count);
        invalidSize.Has400BadRequestError().ShouldBeTrue();
        overflowPage.Has400BadRequestError().ShouldBeTrue();
    }

    [Test]
    public async Task ShouldExcludeDisabledServersFromAggregateAndRejectExplicitRequests()
    {
        // Arrange
        await SetupDatabase(71010, config =>
        {
            config.PlexPhotoLibraryCount = 1;
            config.PhotoAlbumCount = 1;
            config.PhotoCount = 2;
        });
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(x => x.Type == PlexMediaType.PhotoAlbum, CancellationToken);
        await dbContext.PlexServers
            .IgnoreIsEnabledFilter()
            .Where(x => x.Id == library.PlexServerId)
            .ExecuteUpdateAsync(x => x.SetProperty(y => y.IsEnabled, false), CancellationToken);

        // Act
        var aggregate = await TestHandlerExecuteAsync<PhotoAssetPageDTO>(new GetPhotoAssetsQuery(null));
        var explicitRequest = await TestHandlerExecuteAsync<PhotoAssetPageDTO>(
            new GetPhotoAssetsQuery(library.Id)
        );

        // Assert
        aggregate.IsSuccess.ShouldBeTrue();
        aggregate.Value.TotalCount.ShouldBe(0);
        aggregate.Value.TotalBytes.ShouldBe(0);
        explicitRequest.Has400BadRequestError().ShouldBeTrue();
    }
}
