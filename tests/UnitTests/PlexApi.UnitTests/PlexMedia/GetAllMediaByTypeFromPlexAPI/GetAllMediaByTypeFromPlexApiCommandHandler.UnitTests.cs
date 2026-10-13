using System.Net;
using LukeHagar.PlexAPI.SDK;
using LukeHagar.PlexAPI.SDK.Models.Components;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using Reaparr.Application.Contracts;
using Reaparr.PlexApi.Contracts;
using Metadata = LukeHagar.PlexAPI.SDK.Models.Components.Metadata;
using SkipChildren = LukeHagar.PlexAPI.SDK.Models.Components.SkipChildren;
using SkipParent = LukeHagar.PlexAPI.SDK.Models.Components.SkipParent;

namespace Reaparr.PlexApi.UnitTests;

public class GetAllMediaByTypeFromPlexApiCommandHandlerUnitTests
    : BaseCommandUnitTest<GetAllMediaByTypeFromPlexApiCommand>
{
    [Test]
    public async Task ShouldAcceptExplicitZeroWithoutFetchingPages_WhenLibraryIsConfirmedEmpty()
    {
        // Arrange
        await SetupDatabase(
            9203,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexTvShowLibraryCount = 0;
                config.MovieCount = 0;
            }
        );
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var sectionId = long.Parse(library.Key);
        var sdk = new Mock<IPlexAPI>();
        sdk.Setup(x =>
                x.Content.ListContentAsync(
                    It.Is<ListContentRequest>(q =>
                        q.SectionId == sectionId
                        && q.XPlexContainerStart == 0
                        && q.XPlexContainerSize == 0
                        && q.MediaType == 1
                        && q.Sort == "titleSort:asc"
                    )
                )
            )
            .ReturnsAsync(CreateResponse(0, 0))
            .Verifiable(Times.Once());
        Mock.Mock<IPlexApiClientFactory>()
            .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<PlexApiClientOptions>()))
            .Returns(sdk.Object)
            .Verifiable(Times.Once());
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x =>
                x.UpdateItemAsync(
                    library.Id,
                    It.Is<LibraryProgressItem>(p =>
                        p.MediaType == PlexMediaType.OtherVideos && p.Received == 0 && p.Total == 0
                    ),
                    CancellationToken
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync<List<LibraryMediaItemDTO>>(
            new GetAllMediaByTypeFromPlexApiCommand(library, PlexMediaType.OtherVideos)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        result.Value.ShouldBeEmpty();
        sdk.Verify();
        Mock.Mock<IPlexApiClientFactory>().Verify();
        sdk.Verify(
            x => x.Content.ListContentAsync(It.Is<ListContentRequest>(q => q.XPlexContainerSize > 0)),
            Times.Never()
        );
        Mock.Mock<ILibrarySyncProgressStore>().Verify();
    }

    [Test]
    [Arguments(PlexMediaType.Movie)]
    [Arguments(PlexMediaType.OtherVideos)]
    [Arguments(PlexMediaType.MusicTrack)]
    public async Task ShouldRetrieveMediaAcrossAllPages_WhenUsingRealSdk(PlexMediaType mediaType)
    {
        // Arrange
        await SetupDatabase(
            9204,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexAccountCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexTvShowLibraryCount = 0;
                config.MovieCount = 0;
            }
        );
        var dbContext = IDbContext;
        var library = await dbContext.PlexLibraries.SingleAsync(CancellationToken);
        var itemType = mediaType == PlexMediaType.MusicTrack ? "track" : "movie";
        var items = new[] { CreateItem(1, itemType), CreateItem(2, itemType), CreateItem(3, itemType) };
        var requests = new List<IDictionary<string, string>>();
        var transport = new Mock<IPlexApiClient>(MockBehavior.Strict);
        var sdk = new PlexAPI(client: transport.Object, serverUrl: "http://plex.test", token: "test-token");

        transport
            .Setup(x =>
                x.CloneAsync(
                    It.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get
                        && r.RequestUri!.AbsolutePath == $"/library/sections/{library.Key}/all"
                    )
                )
            )
            .ReturnsAsync((HttpRequestMessage request) => request)
            .Verifiable(Times.Exactly(3));
        transport
            .Setup(x =>
                x.SendAsync(
                    It.Is<HttpRequestMessage>(r =>
                        r.Method == HttpMethod.Get
                        && r.RequestUri!.AbsolutePath == $"/library/sections/{library.Key}/all"
                    )
                )
            )
            .ReturnsAsync(
                (HttpRequestMessage request) =>
                {
                    var query = request.ParseQueryToDictionary();
                    requests.Add(query);
                    var start = int.Parse(query["X-Plex-Container-Start"]);
                    var size = int.Parse(query["X-Plex-Container-Size"]);
                    var available = query.ContainsKey("unwatched") ? [] : items;
                    var page = available.Skip(start).Take(size).ToArray();
                    return CreateResponse(available.Length, start, page)
                        .MediaContainerWithMetadata.ToJsonHttpResponse(request, HttpStatusCode.OK);
                }
            )
            .Verifiable(Times.Exactly(3));
        Mock.Mock<IPlexApiClientFactory>()
            .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<PlexApiClientOptions>()))
            .Returns(sdk)
            .Verifiable(Times.Once());
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x =>
                x.UpdateItemAsync(
                    library.Id,
                    It.Is<LibraryProgressItem>(p => p.MediaType == mediaType && p.Received == 2 && p.Total == 3),
                    CancellationToken
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());
        Mock.Mock<ILibrarySyncProgressStore>()
            .Setup(x =>
                x.UpdateItemAsync(
                    library.Id,
                    It.Is<LibraryProgressItem>(p => p.MediaType == mediaType && p.Received == 3 && p.Total == 3),
                    CancellationToken
                )
            )
            .Returns(Task.CompletedTask)
            .Verifiable(Times.Once());

        // Act
        var result = await TestHandlerExecuteAsync<List<LibraryMediaItemDTO>>(
            new GetAllMediaByTypeFromPlexApiCommand(library, mediaType, 2)
        );

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
        var expectedType = mediaType == PlexMediaType.OtherVideos ? PlexMediaType.Movie : mediaType;
        result
            .Value.Select(x => (x.RatingKey, x.Type, x.Title, x.Index))
            .ShouldBe([
                (1, expectedType, "Track 1", 1),
                (2, expectedType, "Track 2", 2),
                (3, expectedType, "Track 3", 3),
            ]);
        requests
            .Select(q => (q["X-Plex-Container-Start"], q["X-Plex-Container-Size"], q.ContainsKey("unwatched")))
            .ShouldBe([("0", "0", false), ("0", "2", false), ("2", "1", false)]);
        requests.Select(q => q["type"]).ShouldBe(Enumerable.Repeat(mediaType.ToPlexMetadataTypeId().ToString(), 3));
        transport.Verify();
        Mock.Mock<IPlexApiClientFactory>().Verify();
        Mock.Mock<ILibrarySyncProgressStore>().Verify();
    }

    private static Metadata CreateItem(int ratingKey, string type) =>
        new()
        {
            RatingKey = ratingKey.ToString(),
            Key = $"/library/metadata/{ratingKey}",
            Type = type,
            Title = $"Track {ratingKey}",
            Index = ratingKey,
            AddedAt = 1,
            SkipChildren = SkipChildren.CreateBoolean(false),
            SkipParent = SkipParent.CreateBoolean(false),
        };

    private static ListContentResponse CreateResponse(long? total, long offset, params Metadata[] items) =>
        new()
        {
            StatusCode = 200,
            RawResponse = new HttpResponseMessage(HttpStatusCode.OK),
            MediaContainerWithMetadata = new MediaContainerWithMetadata
            {
                MediaContainer = new MediaContainerWithMetadataMediaContainer
                {
                    TotalSize = total,
                    Offset = offset,
                    Size = items.Length,
                    Metadata = items.ToList(),
                },
            },
        };
}
