using Microsoft.EntityFrameworkCore;
using Reaparr.PublicAPI;
using BencodeNET.Parsing;
using BencodeNET.Torrents;

namespace PublicApi.UnitTests;

public class DownloadTorrentEndpointUnitTests : BaseUnitTest
{
    public DownloadTorrentEndpointUnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldReturnValidTorrent_WhenEpisodeExists()
    {
        // Arrange – seed an episode with media data/part
        await SetupDatabase(1001, config =>
        {
            config.PlexServerCount = 1;
            config.PlexMovieLibraryCount = 1;
            config.PlexTvShowLibraryCount = 1;
            config.TvShowCount = 10;
            config.TvShowSeasonCount = 3;
            config.TvShowEpisodeCount = 5;
        });

        var server = await IDbContext.PlexServers.FirstAsync(CancellationToken);
        var library = await IDbContext.PlexLibraries.FirstAsync(CancellationToken);
        var episodeData = await IDbContext.PlexTvShowEpisodeData.Include(x => x.Parts).FirstAsync(CancellationToken);
        var part = episodeData.Parts.First();

        var req = new DownloadTorrentEndpointRequest
        {
            Type = PlexMediaType.Episode,
            MediaId = episodeData.PlexTvShowEpisodeId,
            DataId = episodeData.Id,
            PartId = part.Id,
            PartPlexId = part.PlexId,
            Quality = VideoQuality.HD,
            LibraryId = library.Id,
            ServerId = server.Id,
        };

        // Act
        var ep = SetupEndpointUnitTest<DownloadTorrentEndpoint>();
        var buffer = new MemoryStream();
        ep.HttpContext.Response.Body = buffer;
        await ep.HandleAsync(req, CancellationToken);

        // Assert – response is a torrent file with the correct content type
        ep.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        ep.HttpContext.Response.ContentType.ShouldBe("application/x-bittorrent");

        // Assert – bytes were written and are a valid torrent
        var bytes = buffer.ToArray();
        bytes.Length.ShouldBeGreaterThan(0);
        var parser = new BencodeParser();
        var torrent = parser.Parse<Torrent>(new MemoryStream(bytes));
        torrent.ShouldNotBeNull();
        torrent.File.ShouldNotBeNull();
        torrent.File.FileName.ShouldEndWith(".torrent");
        torrent.File.FileSize.ShouldBeGreaterThan(0);
        
    }

    [Fact]
    public async Task ShouldReturnValidTorrent_WhenMovieExists()
    {
        // Arrange – seed a movie with media data/part
        await SetupDatabase(1002, config =>
        {
            config.PlexServerCount = 1;
            config.MovieCount = 1;
        });

        var server = await IDbContext.PlexServers.FirstAsync(CancellationToken);
        var library = await IDbContext.PlexLibraries.FirstAsync(CancellationToken);
        var movieData = await IDbContext.PlexMovieData.Include(x => x.Parts).FirstAsync(CancellationToken);
        var part = movieData.Parts.First();

        var req = new DownloadTorrentEndpointRequest
        {
            Type = PlexMediaType.Movie,
            MediaId = movieData.PlexMovieId,
            DataId = movieData.Id,
            PartId = part.Id,
            PartPlexId = part.PlexId,
            Quality = VideoQuality.HD,
            LibraryId = library.Id,
            ServerId = server.Id,
        };

        // Act
        var ep = SetupEndpointUnitTest<DownloadTorrentEndpoint>();
        var buffer = new MemoryStream();
        ep.HttpContext.Response.Body = buffer;
        await ep.HandleAsync(req, CancellationToken);

        // Assert – response is a torrent file with the correct content type
        ep.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
        ep.HttpContext.Response.ContentType.ShouldBe("application/x-bittorrent");

        // Assert – bytes were written and are a valid torrent
        var bytes = buffer.ToArray();
        bytes.Length.ShouldBeGreaterThan(0);
        var parser = new BencodeParser();
        var torrent = parser.Parse<Torrent>(new MemoryStream(bytes));
        torrent.ShouldNotBeNull();
        torrent.File.ShouldNotBeNull();
        torrent.File.FileName.ShouldEndWith(".torrent");
        torrent.File.FileSize.ShouldBeGreaterThan(0);
    }

    [Fact]
    public async Task ShouldReturnNotFound_WhenNoMatchingMediaFound()
    {
        // Arrange – database with server/library but no matching data id
        await SetupDatabase(1003, config =>
        {
            config.PlexServerCount = 1;
            config.PlexMovieLibraryCount = 1;
            config.PlexTvShowLibraryCount = 1;
            config.MovieCount = 0;
            config.TvShowCount = 0;
        });

        var server = await IDbContext.PlexServers.FirstAsync(CancellationToken);
        var library = await IDbContext.PlexLibraries.FirstAsync(CancellationToken);

        // pick an episode type with random non-existing ids (deterministic constants)
        var req = new DownloadTorrentEndpointRequest
        {
            Type = PlexMediaType.Episode,
            MediaId = 9999,
            DataId = 9999,
            PartId = 9999,
            PartPlexId = 9999,
            Quality = VideoQuality.HD,
            LibraryId = library.Id,
            ServerId = server.Id,
        };

        // Act
        var ep = SetupEndpointUnitTest<DownloadTorrentEndpoint>();
        await ep.HandleAsync(req, CancellationToken);

        // Assert
        ep.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
    }

    [Fact]
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
            PartPlexId = 1,
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

    [Fact]
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
            PartPlexId = 0,
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
}
