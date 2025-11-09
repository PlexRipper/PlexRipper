using System.Net;
using BencodeNET.Objects;
using BencodeNET.Torrents;
using FastEndpoints;
using Reaparr.Application.Contracts;
using Reaparr.Domain;
using Reaparr.PublicAPI;

namespace Reaparr.IntegrationTests.PublicAPI;

[Collection("Sequential")]
public class AddTorrentEndpointIntegrationTests : BaseIntegrationTests
{
    public AddTorrentEndpointIntegrationTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHandleUnsupportedMediaType_WhenSettingHashId()
    {
        // Arrange
        Mock.Mock<ICommandExecutor>()
            .Setup(x => x.Send(It.IsAny<CreateDownloadTasksCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        using var container = await CreateContainer(
            4564,
            x =>
            {
                x.OverrideServices = builder =>
                {
                    builder.RegisterAllAutoMocks(Mock);
                };
            }
        );

        // Act
        var client = container.GetApiClient();
        await container.SignInDownloadClient(client);

        var unsupportedMetadata = CreateValidTorrentMetadata(PlexMediaType.Music); // Unsupported type
        var (torrentFile, torrentFileMock) = CreateMockTorrentFile(unsupportedMetadata, "music.torrent");
        var request = new AddTorrentEndpointRequest { TorrentFile = torrentFile };

        var response = await client.POSTAsync<AddTorrentEndpoint, AddTorrentEndpointRequest>(request);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        // Should handle unsupported media type gracefully

        // Verify command was called once
        Mock.Mock<ICommandExecutor>()
            .Verify(x => x.Send(It.IsAny<CreateDownloadTasksCommand>(), It.IsAny<CancellationToken>()), Times.Once);

        // Verify IFormFile mock interactions
        torrentFileMock.Verify(f => f.OpenReadStream(), Times.Once);
        torrentFileMock.Verify(f => f.FileName, Times.AtLeastOnce);
    }

    private static TorrentMetadataDTO CreateValidTorrentMetadata(PlexMediaType type = PlexMediaType.Movie) =>
        new()
        {
            Type = type,
            MediaId = 1,
            DataId = 1,
            PartId = 1,
            PartPlexId = 12345,
            Quality = VideoQuality.HD,
            LibraryId = 1,
            ServerId = 1,
        };

    private static (IFormFile file, Mock<IFormFile> mock) CreateMockTorrentFile(
        TorrentMetadataDTO metadata,
        string fileName
    )
    {
        // Create a proper torrent using BencodeNET.Torrents.Torrent class like in DownloadTorrentEndpoint
        var extraFields = new BDictionary
        {
            ["created by"] = new BString("Reaparr"),
            ["comment"] = new BString("Test torrent for unit tests"),

            // Add metadata as extra fields
            [nameof(TorrentMetadataDTO.Type)] = new BString(metadata.Type.ToString()),
            [nameof(TorrentMetadataDTO.MediaId)] = new BString(metadata.MediaId.ToString()),
            [nameof(TorrentMetadataDTO.DataId)] = new BString(metadata.DataId.ToString()),
            [nameof(TorrentMetadataDTO.PartId)] = new BString(metadata.PartId.ToString()),
            [nameof(TorrentMetadataDTO.PartPlexId)] = new BString(metadata.PartPlexId.ToString()),
            [nameof(TorrentMetadataDTO.Quality)] = new BString(metadata.Quality.ToString()),
            [nameof(TorrentMetadataDTO.LibraryId)] = new BString(metadata.LibraryId.ToString()),
            [nameof(TorrentMetadataDTO.ServerId)] = new BString(metadata.ServerId.ToString()),
        };

        var torrent = new Torrent
        {
            CreationDate = DateTime.UtcNow,
            Pieces = new byte[20], // Single piece hash (20 bytes SHA1)
            PieceSize = 32768,
            Trackers =
            [
                ["http://tracker.example.com/announce"],
            ],
            File = new SingleFileInfo { FileName = "test.mkv", FileSize = 1000000 },
            ExtraFields = extraFields,
        };

        var torrentBytes = torrent.EncodeAsBytes();

        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.FileName).Returns(fileName);
        formFileMock.Setup(f => f.Length).Returns(torrentBytes.Length);
        formFileMock.Setup(f => f.OpenReadStream()).Returns(() => new MemoryStream(torrentBytes));
        formFileMock.Setup(f => f.ContentType).Returns("application/x-bittorrent");

        return (formFileMock.Object, formFileMock);
    }
}
