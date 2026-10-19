using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Autofac;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi.UnitTests;

public sealed class GetPhotoLibrarySnapshotFromPlexApiCommandUnitTests
    : BaseCommandUnitTest<GetPhotoLibrarySnapshotFromPlexApiCommand>
{
    [Test]
    public async Task ShouldMergeRootFolderAlbumAndClipRepresentations_WhenAllRequiredPhotoTypesAreReturned()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6101);
        var requests = new List<HttpRequestMessage>();
        var transport = SetupTransport(
            request =>
            {
                requests.Add(request);
                var path = request.RequestUri!.PathAndQuery;
                var start = GetContainerStart(request);
                if (path == "/library/sections")
                    return JsonResponse(
                        request,
                        SectionsJson(
                            library.Key,
                            [LocationJson(34, "/fixture/photos"), LocationJson(null, "/fixture/other")]
                        )
                    );
                if (path == $"/library/sections/{library.Key}/folder")
                    return JsonResponse(request, PageJson(null, null, null, "Metadata"));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            0,
                            2,
                            2,
                            "Metadata",
                            AssetJson(1001, "photo", "/fixture/photos/root.jpg", 2001, 3001),
                            FolderJson($"/library/sections/{library.Key}/folder?parent=3706", "Folder A")
                        )
                    );
                if (path == $"/library/sections/{library.Key}/folder?parent=3706")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(1001, "photo", "/fixture/photos/root.jpg", 2001, 3001),
                            AssetJson(1002, "photo", "/fixture/photos/Folder A/child.jpg", 2002, 3002)
                        )
                    );
                if (path == $"/library/sections/{library.Key}/all?type=14")
                    return JsonResponse(
                        request,
                        PageJson(0, 1, 1, "Metadata", AlbumJson(9001, "/library/metadata/9001/children", "Album A"))
                    );
                if (path == "/library/metadata/9001/children")
                    return JsonResponse(
                        request,
                        PageJson(
                            0,
                            1,
                            1,
                            "Metadata",
                            AssetJson(
                                1003,
                                "clip",
                                "/fixture/other/clip.mkv",
                                2003,
                                3003
                            )
                        )
                    );
                if (path == $"/library/sections/{library.Key}/all?type=12")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Video",
                            AssetJson(
                                1003,
                                "clip",
                                "/fixture/other/clip.mkv",
                                2003,
                                3003
                            )
                        )
                    );
                return JsonResponse(request, "{}", HttpStatusCode.NotFound);
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result
            .Value.SourceRoots.Select(x => (x.PlexApiLocationId, x.IdentityKey, x.Path))
            .ShouldBe([((long?)34, "id:34", "/fixture/photos"), (null, "path:posix:/fixture/other", "/fixture/other")]);
        result.Value.PlexLibrary.PhotoAlbums.Select(x => x.PlexApiRatingKey).ShouldBe([9001]);
        result.Value.PlexLibrary.PhotoMediaData.Select(x => x.PlexApiRatingKey).ShouldBe([1001, 1002, 1003]);
        var root = result.Value.PlexLibrary.PhotoMediaData.Single(x => x.PlexApiRatingKey == 1001);
        root.PlexApiMediaId.ShouldBe(2001);
        root.PlexApiPartId.ShouldBe(3001);
        root.SourcePath.ShouldBe("/fixture/photos/root.jpg");
        root.PlexPhotoFolderId.ShouldBeNull();
        var clip = result.Value.PlexLibrary.PhotoMediaData.Single(x => x.PlexApiRatingKey == 1003);
        clip.AssetKind.ShouldBe(PhotoAssetKind.VideoClip);
        clip.PlexApiMediaId.ShouldBe(2003);
        clip.PlexApiPartId.ShouldBe(3003);
        clip.SourcePath.ShouldBe("/fixture/other/clip.mkv");
        result.Value.AlbumMemberships.Select(x => (x.AlbumRatingKey, x.FileRatingKey)).ShouldBe([(9001, 1003)]);
        requests.ShouldNotBeEmpty();
        requests.Select(x => x.Headers.GetValues("X-Plex-Token").Single()).Distinct().ShouldBe(["FAKE_AUTH_TOKEN"]);
        requests.ShouldAllBe(x => x.Headers.Accept.Any(y => y.MediaType == "application/json"));
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldDeduplicateLargeMixedInventory_WhenFolderRootAndTypedViewsOverlap()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6113);
        var rootAssets = Enumerable
            .Range(1, 111)
            .Select(x => AssetJson(15000 + x, "photo", $"/fixture/root/{x}.jpg", 25000 + x, 35000 + x))
            .ToArray();
        var albumAssets = Enumerable
            .Range(1, 17)
            .Select(x => AssetJson(16000 + x, "photo", $"/fixture/album-a/{x}.jpg", 26000 + x, 36000 + x))
            .ToArray();
        var secondAlbumAsset = AssetJson(17001, "photo", "/fixture/album-b/1.jpg", 27001, 37001);
        var finalRootPage = rootAssets.Skip(100).Concat(new[] { rootAssets[0] }).ToArray();
        var sharedChildrenRequests = 0;
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                var start = GetContainerStart(request);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/folder")
                    return JsonResponse(request, PageJson(null, null, null, "Metadata", rootAssets[0]));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                {
                    return start switch
                    {
                        0 => JsonResponse(request, PageJson(0, 50, 112, "Metadata", rootAssets.Take(50).ToArray())),
                        50 => JsonResponse(
                            request,
                            PageJson(50, 50, 112, "Metadata", rootAssets.Skip(50).Take(50).ToArray())
                        ),
                        100 => JsonResponse(request, PageJson(100, 12, 112, "Metadata", finalRootPage)),
                        _ => JsonResponse(request, "{}", HttpStatusCode.BadRequest),
                    };
                }
                if (path == $"/library/sections/{library.Key}/all?type=14")
                    return JsonResponse(
                        request,
                        PageJson(
                            0,
                            2,
                            2,
                            "Metadata",
                            AlbumJson(19001, "/library/metadata/shared/children", "Album A"),
                            AlbumJson(19002, "/library/metadata/shared/children", "Album B")
                        )
                    );
                if (path == "/library/metadata/shared/children")
                {
                    sharedChildrenRequests++;
                    return JsonResponse(
                        request,
                        PageJson(
                            0,
                            18,
                            18,
                            "Metadata",
                            albumAssets.Concat(new[] { secondAlbumAsset }).ToArray()
                        )
                    );
                }
                if (path == $"/library/sections/{library.Key}/all?type=12")
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                return JsonResponse(request, "{}", HttpStatusCode.NotFound);
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.PlexLibrary.PhotoAlbums.Select(x => x.PlexApiRatingKey).ShouldBe([19001, 19002]);
        result.Value.PlexLibrary.PhotoMediaData.Count.ShouldBe(129);
        result.Value.PlexLibrary.PhotoMediaData.Count(x => !result.Value.AlbumMemberships.Any(m => m.FileRatingKey == x.PlexApiRatingKey)).ShouldBe(111);
        result.Value.AlbumMemberships.Select(x => x.FileRatingKey).Distinct().Count().ShouldBe(18);
        result.Value.AlbumMemberships.Count.ShouldBe(36);
        result.Value.AlbumMemberships.Count(x => x.AlbumRatingKey == 19002).ShouldBe(18);
        sharedChildrenRequests.ShouldBe(1);
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldKeepRootOnlyImagesAndClips_WhenNoAlbumsAreReturned()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6102);
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/folder")
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(null, null, null, "Photo", AssetJson(1101, "photo", "/fixture/root.jpg", 2101, 3101))
                    );
                if (path == $"/library/sections/{library.Key}/all?type=14")
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                if (path == $"/library/sections/{library.Key}/all?type=12")
                    return JsonResponse(
                        request,
                        PageJson(null, null, null, "Video", AssetJson(1102, "clip", "/fixture/root.mp4", 2102, 3102))
                    );
                return JsonResponse(request, "{}", HttpStatusCode.NotFound);
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.PlexLibrary.PhotoAlbums.ShouldBeEmpty();
        result
            .Value.PlexLibrary.PhotoMediaData.Select(x => (x.PlexApiRatingKey, x.AssetKind))
            .ShouldBe([
                (1101, PhotoAssetKind.Image),
                (1102, PhotoAssetKind.VideoClip),
            ]);
        result.Value.SourceRoots.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldIncludeAssetsOnlyReturnedByFolderRoot_WhenTypedPhotoViewsAreEmpty()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6114);
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (
                    path == $"/library/sections/{library.Key}/all?type=13"
                    || path == $"/library/sections/{library.Key}/all?type=14"
                    || path == $"/library/sections/{library.Key}/all?type=12"
                )
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                if (path == $"/library/sections/{library.Key}/folder")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(1151, "photo", "/fixture/root-only.jpg", 2151, 3151)
                        )
                    );
                return JsonResponse(request, "{}", HttpStatusCode.NotFound);
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        requests.ShouldBe([
            "/library/sections",
            $"/library/sections/{library.Key}/all?type=13",
            $"/library/sections/{library.Key}/all?type=14",
            $"/library/sections/{library.Key}/all?type=12",
            $"/library/sections/{library.Key}/folder",
        ]);
        result
            .Value.PlexLibrary.PhotoMediaData.Select(x => x.PlexApiRatingKey)
            .ShouldBe([1151]);
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldFailWithoutPublishing_WhenRequiredFolderRootFails()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6115);
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (
                    path == $"/library/sections/{library.Key}/all?type=13"
                    || path == $"/library/sections/{library.Key}/all?type=14"
                    || path == $"/library/sections/{library.Key}/all?type=12"
                )
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                if (path == $"/library/sections/{library.Key}/folder")
                    return JsonResponse(request, "{}", HttpStatusCode.InternalServerError);
                return JsonResponse(request, "{}", HttpStatusCode.NotFound);
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe([
            "/library/sections",
            $"/library/sections/{library.Key}/all?type=13",
            $"/library/sections/{library.Key}/all?type=14",
            $"/library/sections/{library.Key}/all?type=12",
            $"/library/sections/{library.Key}/folder",
        ]);
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldReturnCancelledWithoutPublishing_WhenRequiredFolderRootIsCancelled()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6116);
        var requests = new List<string>();
        var transport = new Mock<IPlexApiClient>(MockBehavior.Strict);
        transport.Setup(x => x.Dispose()).Verifiable(Times.Once());
        transport
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .Returns<HttpRequestMessage>(
                request =>
                {
                    var path = request.RequestUri!.PathAndQuery;
                    requests.Add(path);
                    if (path == "/library/sections")
                        return Task.FromResult(JsonResponse(request, SectionsJson(library.Key, [])));
                    if (
                        path == $"/library/sections/{library.Key}/all?type=13"
                        || path == $"/library/sections/{library.Key}/all?type=14"
                        || path == $"/library/sections/{library.Key}/all?type=12"
                    )
                        return Task.FromResult(JsonResponse(request, "{\"MediaContainer\":{}}"));
                    if (path == $"/library/sections/{library.Key}/folder")
                        return Task.FromCanceled<HttpResponseMessage>(new CancellationToken(true));
                    throw new InvalidOperationException($"Unexpected photo request: {path}");
                }
            )
            .Verifiable();
        SetupClientFactory(transport.Object);

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsCancelled.ShouldBeTrue();
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe([
            "/library/sections",
            $"/library/sections/{library.Key}/all?type=13",
            $"/library/sections/{library.Key}/all?type=14",
            $"/library/sections/{library.Key}/all?type=12",
            $"/library/sections/{library.Key}/folder",
        ]);
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldAdvanceByReturnedWindowSize_WhenRootPagesAreShorterThanRequestedBatch()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6103);
        var starts = new List<int>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                var start = GetContainerStart(request);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/folder")
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                if (path == $"/library/sections/{library.Key}/all?type=13")
                {
                    starts.Add(start);
                    return start switch
                    {
                        0 => JsonResponse(
                            request,
                            PageJson(0, 1, 2, "Metadata", AssetJson(1201, "photo", "/fixture/a.jpg", 2201, 3201))
                        ),
                        1 => JsonResponse(
                            request,
                            PageJson(1, 1, 2, "Metadata", AssetJson(1202, "photo", "/fixture/b.jpg", 2202, 3202))
                        ),
                        _ => JsonResponse(request, "{}", HttpStatusCode.BadRequest),
                    };
                }
                if (
                    path == $"/library/sections/{library.Key}/all?type=14"
                    || path == $"/library/sections/{library.Key}/all?type=12"
                )
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                return JsonResponse(request, "{}", HttpStatusCode.NotFound);
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        starts.ShouldBe([0, 1]);
        result.Value.PlexLibrary.PhotoMediaData.Select(x => x.PlexApiRatingKey).ShouldBe([1201, 1202]);
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldReadUntilAnEmptyPage_WhenPaginationHasNoReportedTotal()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6111);
        var starts = new List<int>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                var start = GetContainerStart(request);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/folder")
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                if (path == $"/library/sections/{library.Key}/all?type=13")
                {
                    starts.Add(start);
                    return start switch
                    {
                        0 => JsonResponse(
                            request,
                            PageJson(0, 1, null, "Metadata", AssetJson(1211, "photo", "/fixture/a.jpg", 2211, 3211))
                        ),
                        1 => JsonResponse(request, "{\"MediaContainer\":{}}"),
                        _ => JsonResponse(request, "{}", HttpStatusCode.BadRequest),
                    };
                }
                if (
                    path == $"/library/sections/{library.Key}/all?type=14"
                    || path == $"/library/sections/{library.Key}/all?type=12"
                )
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                return JsonResponse(request, "{}", HttpStatusCode.NotFound);
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        starts.ShouldBe([0, 1]);
        result.Value.PlexLibrary.PhotoMediaData.Select(x => x.PlexApiRatingKey).ShouldBe([1211]);
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectExternalNavigationKeysWithoutPublishingAnyAssets()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6104);
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Directory",
                            FolderJson("https://external.test/library/sections/999/folder", "Unsafe")
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(
            x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
            Times.Exactly(2)
        );
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectNavigationKeysContainingTokens()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6112);
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Directory",
                            FolderJson($"/library/sections/{library.Key}/folder?parent=1&X-Plex-Token=secret", "Unsafe")
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe(["/library/sections", $"/library/sections/{library.Key}/all?type=13"]);
        requests.ShouldNotContain(x => x.Contains("secret", StringComparison.Ordinal));
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectFolderCyclesBeforeRepeatingTheRequest()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6105);
        var folderRequests = 0;
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Directory",
                            FolderJson($"/library/sections/{library.Key}/folder?parent=1", "Cycle")
                        )
                    );
                if (path == $"/library/sections/{library.Key}/folder?parent=1")
                {
                    folderRequests++;
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Directory",
                            FolderJson($"/library/sections/{library.Key}/folder?parent=1", "Cycle")
                        )
                    );
                }
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        folderRequests.ShouldBe(1);
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldReturnCancelledResultAndLeaveLibraryUntouched_WhenRequiredRequestIsCancelled()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6106);
        var transport = new Mock<IPlexApiClient>(MockBehavior.Strict);
        transport.Setup(x => x.Dispose()).Verifiable(Times.Once());
        transport
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .Returns<HttpRequestMessage>(
                _ => Task.FromCanceled<HttpResponseMessage>(new CancellationToken(true))
            );
        SetupClientFactory(transport.Object);

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsCancelled.ShouldBeTrue();
        result.ValueOrDefault.ShouldBeNull();
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldIgnoreConflictingRawParentsBeforePublishing()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6107);
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (
                    path == $"/library/sections/{library.Key}/all?type=14"
                    || path == $"/library/sections/{library.Key}/all?type=12"
                    || path == $"/library/sections/{library.Key}/folder"
                )
                    return JsonResponse(request, PageJson(null, null, null, "Metadata"));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(1301, "photo", "/fixture/conflict.jpg", 2301, 3301, parentRatingKey: "8001"),
                            FolderJson($"/library/sections/{library.Key}/folder?parent=2", "Folder")
                        )
                    );
                if (path == $"/library/sections/{library.Key}/folder?parent=2")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(1301, "photo", "/fixture/conflict.jpg", 2301, 3301, parentRatingKey: "8002")
                        )
                    );
                return JsonResponse(request, "{}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.PlexLibrary.PhotoMediaData.Select(x => x.PlexApiRatingKey).ShouldBe([1301]);
        result.Value.AlbumMemberships.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldPreserveSourceSpellingAndUseScopedCanonicalIdentity_WhenLocationsUseDifferentPathStyles()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6109);
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                if (path == "/library/sections")
                    return JsonResponse(
                        request,
                        SectionsJson(
                            library.Key,
                            [
                                LocationJson(null, "/Fixture/Photos"),
                                LocationJson(null, "C:\\Photos\\A"),
                                LocationJson(null, "\\\\server\\share\\Photos"),
                            ]
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result
            .Value.SourceRoots.Select(x => (x.IdentityKey, x.Path))
            .ShouldBe([
                ("path:posix:/Fixture/Photos", "/Fixture/Photos"),
                ("path:windows:C:/PHOTOS/A", "C:\\Photos\\A"),
                ("path:windows://SERVER/SHARE/PHOTOS", "\\\\server\\share\\Photos"),
            ]);
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectUnknownAssetTypeBeforePublishingAnyPhoto()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6110);
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(1401, "mystery", "/fixture/unknown.bin", 2401, 3401)
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectInvalidConfiguredRootBeforeFetchingPhotoTypes()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6108);
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                requests.Add(request.RequestUri!.PathAndQuery);
                if (request.RequestUri!.PathAndQuery == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, [LocationJson(34, "../unsafe")]));
                return JsonResponse(request, "{}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe(["/library/sections"]);
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    [Arguments("Metadata")]
    [Arguments("Directory")]
    [Arguments("Photo")]
    [Arguments("Video")]
    public async Task ShouldRejectScalarTopLevelCollectionValues(string collectionName)
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(
            collectionName switch
            {
                "Metadata" => 6120,
                "Directory" => 6121,
                "Photo" => 6122,
                "Video" => 6123,
                _ => 6124,
            }
        );
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(request, $"{{\"MediaContainer\":{{\"{collectionName}\":\"malformed\"}}}}");
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe(["/library/sections", $"/library/sections/{library.Key}/all?type=13"]);
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(
            x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
            Times.Exactly(2)
        );
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectScalarLocationCollectionBeforeFetchingPhotoTypes()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6125);
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(
                        request,
                        $"{{\"MediaContainer\":{{\"size\":1,\"Directory\":[{{\"key\":\"{library.Key}\",\"type\":\"photo\",\"title\":\"Photos\",\"Location\":\"malformed\"}}]}}}}"
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe(["/library/sections"]);
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once());
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectCrossSectionNavigationKeysBeforeFetchingThem()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6126);
        var otherSectionKey = (long.Parse(library.Key, CultureInfo.InvariantCulture) + 1).ToString(
            CultureInfo.InvariantCulture
        );
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Directory",
                            FolderJson($"/library/sections/{otherSectionKey}/folder", "Unsafe")
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe(["/library/sections", $"/library/sections/{library.Key}/all?type=13"]);
        requests.ShouldNotContain(x => x.Contains($"/sections/{otherSectionKey}/", StringComparison.Ordinal));
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(
            x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
            Times.Exactly(2)
        );
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectDeclaredCrossSectionMembershipBeforeFollowingTheRecord()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6127);
        var otherSectionId = long.Parse(library.Key, CultureInfo.InvariantCulture) + 1;
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(
                                1801,
                                "photo",
                                "/fixture/wrong-section.jpg",
                                2801,
                                3801,
                                declaredSectionId: otherSectionId
                            )
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe(["/library/sections", $"/library/sections/{library.Key}/all?type=13"]);
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(
            x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
            Times.Exactly(2)
        );
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectDeclaredCrossSectionAlbumBeforeFetchingItsChildren()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6128);
        var otherSectionId = long.Parse(library.Key, CultureInfo.InvariantCulture) + 1;
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                if (path == $"/library/sections/{library.Key}/all?type=14")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AlbumJson(9801, "/library/metadata/9801/children", "Wrong Album", otherSectionId)
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe([
            "/library/sections",
            $"/library/sections/{library.Key}/all?type=13",
            $"/library/sections/{library.Key}/all?type=14",
        ]);
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(
            x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
            Times.Exactly(3)
        );
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldCanonicalizeReturnedEndpointAliasesBeforeRepeatingFetch()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6129);
        var connection = await IDbContext.PlexServerConnections.SingleAsync(CancellationToken);
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Directory",
                            FolderJson($"{connection.Url}/library/sections/{library.Key}/folder/", "Alias Root")
                        )
                    );
                if (path == $"/library/sections/{library.Key}/folder/")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(1810, "photo", "/fixture/alias.jpg", 2810, 3810)
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        requests.ShouldBe([
            "/library/sections",
            $"/library/sections/{library.Key}/all?type=13",
            $"/library/sections/{library.Key}/folder/",
            $"/library/sections/{library.Key}/all?type=14",
            $"/library/sections/{library.Key}/all?type=12",
        ]);
        result.Value.PlexLibrary.PhotoMediaData.Select(x => x.PlexApiRatingKey).ShouldBe([1810]);
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectEncodedDotTraversalBeforeFetchingIt()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6130);
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Directory",
                            FolderJson($"/library/sections/{library.Key}/folder/%2e%2e", "Unsafe")
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe(["/library/sections", $"/library/sections/{library.Key}/all?type=13"]);
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(
            x => x.SendAsync(It.IsAny<HttpRequestMessage>()),
            Times.Exactly(2)
        );
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldIgnoreUnresolvedRawParentObservation_WhenNoVerifiedAlbumEdgeExists()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6131);
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(
                                1811,
                                "photo",
                                "/fixture/legacy-parent.jpg",
                                2811,
                                3811,
                                legacyParentKey: "9991",
                                explicitNullParentRatingKey: true
                            )
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var file = result.Value.PlexLibrary.PhotoMediaData.Single();
        file.PlexApiRatingKey.ShouldBe(1811);
        result.Value.AlbumMemberships.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldKeepDistinctPosixRootsThatContainLiteralBackslashes()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6132);
        var transport = SetupTransport(
            request =>
            {
                if (request.RequestUri!.PathAndQuery == "/library/sections")
                    return JsonResponse(
                        request,
                        SectionsJson(
                            library.Key,
                            [
                                LocationJson(null, "/fixture/photos\\literal"),
                                LocationJson(null, "/fixture/photos/literal"),
                            ]
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result
            .Value.SourceRoots.Select(x => (x.IdentityKey, x.Path))
            .ShouldBe([
                ("path:posix:/fixture/photos\\literal", "/fixture/photos\\literal"),
                ("path:posix:/fixture/photos/literal", "/fixture/photos/literal"),
            ]);
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldPublishFlatAlbumsAndDirectFiles_WhenAssetsHaveVerifiedAlbumMembership()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6133);
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(1820, "photo", "/fixture/root-10.jpg", 2820, 3820, title: "Root 10"),
                            AssetJson(1821, "photo", "/fixture/root-2.jpg", 2821, 3821, title: "Root 2")
                        )
                    );
                if (path == $"/library/sections/{library.Key}/all?type=14")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AlbumJson(9820, "/library/metadata/9820/children", "Album 1")
                        )
                    );
                if (path == "/library/metadata/9820/children")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(
                                1822,
                                "photo",
                                "/fixture/child-10.jpg",
                                2822,
                                3822,
                                parentRatingKey: "9820",
                                title: "Child 10"
                            ),
                            AssetJson(
                                1823,
                                "photo",
                                "/fixture/child-2.jpg",
                                2823,
                                3823,
                                parentRatingKey: "9820",
                                title: "Child 2"
                            )
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result
            .Value.PlexLibrary.PhotoMediaData.Select(x => (x.PlexApiRatingKey, x.Title))
            .ShouldBe([
                (1820, "Root 10"),
                (1821, "Root 2"),
                (1822, "Child 10"),
                (1823, "Child 2"),
            ]);
        result
            .Value.AlbumMemberships.Select(x => (x.AlbumRatingKey, x.FileRatingKey))
            .ShouldBe([(9820, 1822), (9820, 1823)]);
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectConflictingAssetKindsBeforePublishing()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6134);
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(1830, "photo", "/fixture/kind.jpg", 2830, 3830)
                        )
                    );
                if (path == $"/library/sections/{library.Key}/all?type=14")
                    return JsonResponse(request, "{\"MediaContainer\":{}}");
                if (path == $"/library/sections/{library.Key}/all?type=12")
                    return JsonResponse(
                        request,
                        PageJson(null, null, null, "Video", AssetJson(1830, "clip", "/fixture/kind.jpg", 2830, 3830))
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe([
            "/library/sections",
            $"/library/sections/{library.Key}/all?type=13",
            $"/library/sections/{library.Key}/all?type=14",
            $"/library/sections/{library.Key}/all?type=12",
        ]);
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task ShouldRejectMultipleFileRepresentationsBeforePublishing(bool multipleMedia)
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(multipleMedia ? 6145 : 6135);
        var requests = new List<string>();
        var malformedAsset = multipleMedia
            ? AssetJson(1831, "photo", "/fixture/part-a.jpg", 2831, 3831, mediaCount: 2)
            : AssetJson(
                1831,
                "photo",
                "/fixture/part-a.jpg",
                2831,
                3831,
                parts: ["/fixture/part-a.jpg", "/fixture/part-b.jpg"]
            );
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=13")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            malformedAsset
                        )
                    );
                if (path == $"/library/sections/{library.Key}/folder")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AssetJson(1831, "photo", "/fixture/part-b.jpg", 2831, 3831)
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.Errors.Single().Message.ShouldContain("exactly one Media and one Part");
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe(["/library/sections", $"/library/sections/{library.Key}/all?type=13"]);
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectNestedNavigationInsideMetadataAlbum()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6136);
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, []));
                if (path == $"/library/sections/{library.Key}/all?type=14")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Metadata",
                            AlbumJson(9831, "/library/metadata/9831/children", "Album")
                        )
                    );
                if (path == "/library/metadata/9831/children")
                    return JsonResponse(
                        request,
                        PageJson(
                            null,
                            null,
                            null,
                            "Directory",
                            FolderJson("/library/sections/1/folder?parent=2", "Nested")
                        )
                    );
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe([
            "/library/sections",
            $"/library/sections/{library.Key}/all?type=13",
            $"/library/sections/{library.Key}/all?type=14",
            "/library/metadata/9831/children",
        ]);
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    [Test]
    public async Task ShouldRejectDeclaredSectionMismatchBeforeFetchingPhotoTypes()
    {
        // Arrange
        var library = await SetupPhotoLibraryAsync(6137);
        var otherSectionId = long.Parse(library.Key, CultureInfo.InvariantCulture) + 1;
        var requests = new List<string>();
        var transport = SetupTransport(
            request =>
            {
                var path = request.RequestUri!.PathAndQuery;
                requests.Add(path);
                if (path == "/library/sections")
                    return JsonResponse(request, SectionsJson(library.Key, [], otherSectionId));
                return JsonResponse(request, "{\"MediaContainer\":{}}");
            }
        );

        // Act
        var result = await TestHandlerExecuteAsync<PhotoLibrarySnapshot>(
            new GetPhotoLibrarySnapshotFromPlexApiCommand(library)
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBe(1);
        result.ValueOrDefault.ShouldBeNull();
        requests.ShouldBe(["/library/sections"]);
        library.PhotoAlbums.ShouldBeEmpty();
        library.PhotoMediaData.ShouldBeEmpty();
        transport.Verify(x => x.SendAsync(It.IsAny<HttpRequestMessage>()), Times.Once());
        transport.Verify(x => x.Dispose(), Times.Once());
    }

    private async Task<PlexLibrary> SetupPhotoLibraryAsync(int seed)
    {
        await SetupDatabase(
            seed,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexServerConnectionPerServerCount = 1;
                config.PlexAccountCount = 1;
                config.PlexPhotoLibraryCount = 1;
                config.PlexMovieLibraryCount = 0;
                config.PlexTvShowLibraryCount = 0;
                config.PlexMusicLibraryCount = 0;
                config.PlexOtherVideoLibraryCount = 0;
            }
        );

        return await IDbContext.PlexLibraries.SingleAsync(CancellationToken);
    }

    private Mock<IPlexApiClient> SetupTransport(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
    {
        var transport = new Mock<IPlexApiClient>(MockBehavior.Strict);
        transport.Setup(x => x.Dispose()).Verifiable(Times.Once());
        transport
            .Setup(x => x.SendAsync(It.IsAny<HttpRequestMessage>()))
            .Returns(
                (HttpRequestMessage request) => Task.FromResult(responseFactory(request))
            )
            .Verifiable();
        SetupClientFactory(transport.Object);
        return transport;
    }

    private void SetupClientFactory(IPlexApiClient client)
    {
        Func<PlexApiClientOptions?, IPlexApiClient> factory = _ => client;
        SetupDependencies(builder =>
            builder.RegisterInstance(factory).As<Func<PlexApiClientOptions?, IPlexApiClient>>()
        );
    }

    private static int GetContainerStart(HttpRequestMessage request) =>
        int.Parse(request.Headers.GetValues("X-Plex-Container-Start").Single(), CultureInfo.InvariantCulture);

    private static HttpResponseMessage JsonResponse(
        HttpRequestMessage request,
        string json,
        HttpStatusCode statusCode = HttpStatusCode.OK
    ) =>
        new(statusCode)
        {
            RequestMessage = request,
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };

    private static string SectionsJson(string key, IReadOnlyList<string> locations, long? declaredSectionId = null)
    {
        var locationJson = string.Join(',', locations);
        var sectionId = declaredSectionId is { } value ? $",\"librarySectionID\":{value}" : string.Empty;
        return $"{{\"MediaContainer\":{{\"size\":1,\"Directory\":[{{\"key\":\"{key}\",\"type\":\"photo\",\"title\":\"Photos\"{sectionId},\"Location\":[{locationJson}]}}]}}}}";
    }

    private static string LocationJson(long? id, string path)
    {
        var encodedPath = JsonSerializer.Serialize(path);
        return id is { } value ? $"{{\"id\":{value},\"path\":{encodedPath}}}" : $"{{\"path\":{encodedPath}}}";
    }

    private static string PageJson(int? offset, int? size, int? total, string arrayName, params string[] items)
    {
        var page = new List<string>();
        if (offset is { } pageOffset)
            page.Add($"\"offset\":{pageOffset}");
        if (size is { } pageSize)
            page.Add($"\"size\":{pageSize}");
        if (total is { } pageTotal)
            page.Add($"\"totalSize\":{pageTotal}");
        page.Add($"\"{arrayName}\":[{string.Join(',', items)}]");
        return $"{{\"MediaContainer\":{{{string.Join(',', page)}}}}}";
    }

    private static string FolderJson(string key, string title, long? declaredSectionId = null)
    {
        var sectionId = declaredSectionId is { } value ? $",\"librarySectionID\":{value}" : string.Empty;
        var titleJson = JsonSerializer.Serialize(title);
        return $"{{\"key\":\"{key}\",\"title\":{titleJson},\"type\":\"folder\"{sectionId}}}";
    }

    private static string AlbumJson(int ratingKey, string key, string title, long? declaredSectionId = null)
    {
        var sectionId = declaredSectionId is { } value ? $",\"librarySectionID\":{value}" : string.Empty;
        var titleJson = JsonSerializer.Serialize(title);
        return $"{{\"ratingKey\":\"{ratingKey}\",\"type\":\"photo\",\"key\":\"{key}\",\"title\":{titleJson}{sectionId}}}";
    }

    private static string AssetJson(
        int ratingKey,
        string type,
        string file,
        int mediaId,
        int partId,
        string? parentRatingKey = null,
        IReadOnlyList<string>? parts = null,
        long? declaredSectionId = null,
        string? legacyParentKey = null,
        bool explicitNullParentRatingKey = false,
        string? title = null,
        int mediaCount = 1
    )
    {
        var partFiles = parts ?? [file];
        var partJson = string.Join(
            ',',
            partFiles.Select(
                (part, index) =>
                    $"{{\"id\":{partId + index},\"key\":\"/library/parts/{partId + index}/file\",\"file\":\"{part}\",\"size\":{index + 1}000,\"container\":\"jpg\"}}"
            )
        );
        var mediaJson = string.Join(
            ',',
            Enumerable
                .Range(0, mediaCount)
                .Select(index => $"{{\"id\":{mediaId + index},\"videoResolution\":\"1080\",\"Part\":[{partJson}]}}")
        );
        var titleJson = JsonSerializer.Serialize(title ?? $"Asset {ratingKey}");
        var sectionId = declaredSectionId is { } value ? $",\"librarySectionID\":{value}" : string.Empty;
        string parent;
        if (explicitNullParentRatingKey)
            parent = ",\"parentRatingKey\":null";
        else if (parentRatingKey is null)
            parent = string.Empty;
        else
            parent = $",\"parentRatingKey\":\"{parentRatingKey}\"";
        var legacyParent = legacyParentKey is null ? string.Empty : $",\"parentKey\":\"{legacyParentKey}\"";
        return $"{{\"ratingKey\":\"{ratingKey}\",\"type\":\"{type}\",\"title\":{titleJson}{sectionId}{parent}{legacyParent},\"Media\":[{mediaJson}]}}";
    }
}
