using BencodeNET.Parsing;
using BencodeNET.Torrents;

namespace Reaparr.PublicAPI.UnitTests;

public class DownloadTorrentEndpointUnitTests
    : BaseEndpointUnitTest<DownloadTorrentEndpoint, DownloadTorrentEndpointRequest>
{
    [Test]
    public async Task ShouldReturnValidTorrent_WhenEpisodeExists()
    {
        // Arrange – seed an episode with media data/part
        await SetupDatabase(
            1001,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.TvShowCount = 10;
                config.TvShowSeasonCount = 3;
                config.TvShowEpisodeCount = 5;
            }
        );

        var server = await IDbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        server.ShouldNotBeNull();

        var library = await IDbContext
            .PlexLibraries.Where(x => x.Type == PlexMediaType.TvShow)
            .FirstOrDefaultAsync(CancellationToken);
        library.ShouldNotBeNull();

        var episodeData = await IDbContext.PlexTvShowEpisodeData.FirstOrDefaultAsync(CancellationToken);
        episodeData.ShouldNotBeNull();

        var req = new DownloadTorrentEndpointRequest
        {
            Type = PlexMediaType.Episode,
            MediaId = episodeData.PlexTvShowEpisodeId,
            DataId = episodeData.Id,
            PartId = episodeData.Id,
            PlexApiPartId = episodeData.PlexApiPartId,
            Quality = VideoQuality.HD,
            LibraryId = library.Id,
            ServerId = server.Id,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(req);
        var buffer = endpointResult.Endpoint.HttpContext.Response.Body;

        // Assert – response is a torrent file with the correct content type
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        endpointResult.ContentType.ShouldBe("application/x-bittorrent");

        // Assert – bytes were written and are a valid torrent
        var bytes = ((MemoryStream)buffer).ToArray();
        bytes.Length.ShouldBeGreaterThan(0);
        var parser = new BencodeParser();
        var torrent = parser.Parse<Torrent>(new MemoryStream(bytes));
        torrent.ShouldNotBeNull();
        torrent.File.ShouldNotBeNull();
        torrent.File.FileName.ShouldEndWith(".torrent");
        torrent.File.FileSize.ShouldBeGreaterThan(0);

        // Assert – piece size and standard fields
        torrent.PieceSize.ShouldBe(256 * 1024);
        torrent.CreatedBy.ShouldBe("Reaparr");
        torrent.Comment.ShouldContain("Reaparr");

        // Assert – extra fields contain request metadata
        var extra = torrent.ExtraFields;
        extra.ShouldNotBeNull();
        foreach (var kv in req.Values)
        {
            extra.TryGetValue(kv.Key, out var v).ShouldBeTrue();
            v!.ToString().ShouldBe(kv.Value);
        }
    }

    [Test]
    public async Task ShouldReturnValidTorrent_WhenMovieExists()
    {
        // Arrange – seed a movie with media data/part
        await SetupDatabase(
            1002,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.MovieCount = 1;
            }
        );

        var server = await IDbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        server.ShouldNotBeNull();

        var library = await IDbContext
            .PlexLibraries.Where(x => x.Type == PlexMediaType.Movie)
            .FirstOrDefaultAsync(CancellationToken);
        library.ShouldNotBeNull();

        var movieData = await IDbContext.PlexMovieData.FirstOrDefaultAsync(CancellationToken);
        movieData.ShouldNotBeNull();

        var req = new DownloadTorrentEndpointRequest
        {
            Type = PlexMediaType.Movie,
            MediaId = movieData.PlexMovieId,
            DataId = movieData.Id,
            PartId = movieData.Id,
            PlexApiPartId = movieData.PlexApiPartId,
            Quality = VideoQuality.HD,
            LibraryId = library.Id,
            ServerId = server.Id,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(req);
        var buffer = endpointResult.Endpoint.HttpContext.Response.Body;

        // Assert – response is a torrent file with the correct content type
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status200OK);
        endpointResult.ContentType.ShouldBe("application/x-bittorrent");

        // Assert – bytes were written and are a valid torrent
        var bytes = ((MemoryStream)buffer).ToArray();
        bytes.Length.ShouldBeGreaterThan(0);
        var parser = new BencodeParser();
        var torrent = parser.Parse<Torrent>(new MemoryStream(bytes));
        torrent.ShouldNotBeNull();
        torrent.File.ShouldNotBeNull();
        torrent.File.FileName.ShouldEndWith(".torrent");
        torrent.File.FileSize.ShouldBeGreaterThan(0);

        // Assert – piece size and standard fields
        torrent.PieceSize.ShouldBe(256 * 1024);
        torrent.CreatedBy.ShouldBe("Reaparr");
        torrent.Comment.ShouldContain("Reaparr");

        // Assert – extra fields contain request metadata
        var extra = torrent.ExtraFields;
        extra.ShouldNotBeNull();
        foreach (var kv in req.Values)
        {
            extra.TryGetValue(kv.Key, out var v).ShouldBeTrue();
            v!.ToString().ShouldBe(kv.Value);
        }
    }

    [Test]
    public async Task ShouldReturnNotFound_WhenNoMatchingMediaFound()
    {
        // Arrange – database with server/library but no matching data id
        await SetupDatabase(
            1003,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
                config.PlexTvShowLibraryCount = 1;
                config.MovieCount = 0;
                config.TvShowCount = 0;
            }
        );

        var server = await IDbContext.PlexServers.FirstOrDefaultAsync(CancellationToken);
        server.ShouldNotBeNull();

        var library = await IDbContext
            .PlexLibraries.Where(x => x.Type == PlexMediaType.TvShow)
            .FirstOrDefaultAsync(CancellationToken);
        library.ShouldNotBeNull();

        // pick an episode type with random non-existing ids (deterministic constants)
        var req = new DownloadTorrentEndpointRequest
        {
            Type = PlexMediaType.Episode,
            MediaId = 9999,
            DataId = 9999,
            PartId = 9999,
            PlexApiPartId = 9999,
            Quality = VideoQuality.HD,
            LibraryId = library.Id,
            ServerId = server.Id,
        };

        // Act
        var endpointResult = await TestEndpointHandleAsync(req);

        // Assert
        endpointResult.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Test]
    public void DownloadTorrentEndpointRequestValidator_ShouldValidateRequiredFields()
    {
        // Arrange
        var validator = new DownloadTorrentEndpointRequestValidator();
        var req = new DownloadTorrentEndpointRequest
        {
            Type = PlexMediaType.Movie,
            MediaId = 1,
            DataId = 1,
            PartId = 1,
            PlexApiPartId = 1,
            Quality = VideoQuality.HD,
            LibraryId = 1,
            ServerId = 1,
        };

        // Act
        var result = validator.Validate(req);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public void DownloadTorrentEndpointRequestValidator_ShouldFail_OnInvalidFields()
    {
        // Arrange
        var validator = new DownloadTorrentEndpointRequestValidator();
        var invalid = new DownloadTorrentEndpointRequest
        {
            Type = (PlexMediaType)999,
            MediaId = 0,
            DataId = 0,
            PartId = 0,
            PlexApiPartId = 0,
            Quality = (VideoQuality)999,
            LibraryId = 0,
            ServerId = 0,
        };

        // Act
        var result = validator.Validate(invalid);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }

    [Test]
    public void DownloadTorrentEndpointRequestValidator_ShouldRejectUnsupportedMediaTypes()
    {
        // Arrange
        var validator = new DownloadTorrentEndpointRequestValidator();
        var request = new DownloadTorrentEndpointRequest
        {
            Type = PlexMediaType.Season,
            MediaId = 1,
            DataId = 1,
            PartId = 1,
            PlexApiPartId = 1,
            Quality = VideoQuality.HD,
            LibraryId = 1,
            ServerId = 1,
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == "Type");
    }
}
