namespace Reaparr.Application.UnitTests;

public class BasePlexMediaDataMapperGenerateReleaseNameUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldGenerateReleaseName_WhenGivenValidMediaMetadata()
    {
        // Arrange
        var seed = new Seed(12345);
        var videoStream = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Video).Generate() with
        {
            DOVIPresent = false,
            Profile = "high",
            ColorPrimaries = "bt709",
            ColorTrc = "bt709",
        };

        // Act
        var releaseName = BasePlexMediaDataMapper.GenerateReleaseName(
            title: "Known Title",
            year: 2024,
            videoResolution: VideoQuality.FullHD,
            videoCodec: "x265",
            audioCodec: "AC3",
            videoStream: videoStream,
            audioLayout: "5.1",
            languageFormat: string.Empty,
            source: ReleaseSource.WebDl,
            isRemux: false
        );

        // Assert
        releaseName.ShouldBe("Known.Title.2024.1080p.WEB-DL.x265.AC3.5.1");
    }

    [Test]
    public void ShouldOmitSourceToken_WhenReleaseSourceIsUnknown()
    {
        // Arrange
        var seed = new Seed(12346);
        var videoStream = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Video).Generate() with
        {
            DOVIPresent = false,
            Profile = "high",
            ColorPrimaries = "bt709",
            ColorTrc = "bt709",
        };

        // Act
        var releaseName = BasePlexMediaDataMapper.GenerateReleaseName(
            title: "Known Title",
            year: 2024,
            videoResolution: VideoQuality.FullHD,
            videoCodec: "x265",
            audioCodec: "AC3",
            videoStream: videoStream,
            audioLayout: "5.1",
            languageFormat: string.Empty,
            source: ReleaseSource.None,
            isRemux: false
        );

        // Assert
        releaseName.ShouldBe("Known.Title.2024.1080p.x265.AC3.5.1");
    }

    [Test]
    public void ShouldUpdateMetadataFromVideoStream_WhenAudioStreamIsDefault()
    {
        // Arrange
        var seed = new Seed(12347);
        var mediaData = FakeData.GetPlexMovieMediaData(seed).Generate();
        mediaData.NeedsGeneratedName = true;
        mediaData.GeneratedFilename = null;
        mediaData.GeneratedNameSyncedAt = null;
        var metadata = FakeData.GetLibraryMediaItemDTO(seed, mediaType: PlexMediaType.Movie).Generate() with
        {
            Title = "Known Movie",
            Year = 2024,
        };
        var audioStream = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Audio).Generate() with
        {
            Default = true,
            LanguageCode = "en",
        };
        var videoStream = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Video).Generate() with
        {
            Default = false,
            DOVIPresent = true,
        };
        var part = FakeData.GetLibraryMediaItemPartDTO(seed).Generate() with
        {
            Stream = [audioStream, videoStream],
        };
        var media = metadata.Media.Single() with
        {
            VideoResolution = VideoQuality.FullHD,
            VideoCodec = "hevc",
            AudioCodec = "ac3",
            AudioProfile = "main",
            AudioChannels = 6,
            Parts = [part],
        };
        metadata = metadata with { Media = [media] };
        var startedAt = DateTime.UtcNow;

        // Act
        var updated = mediaData.UpdateStreamMetadata(metadata, media, part);

        // Assert
        updated.ShouldBeTrue();
        mediaData.GeneratedFilename.ShouldNotBeNull();
        mediaData.GeneratedFilename.ShouldContain(".DV.");
        mediaData.GeneratedFilename.ShouldContain(".x265.");
        mediaData.NeedsGeneratedName.ShouldBeFalse();
        mediaData.GeneratedNameSyncedAt.ShouldNotBeNull();
        mediaData.GeneratedNameSyncedAt.Value.ShouldBeGreaterThanOrEqualTo(startedAt);
        mediaData.GeneratedNameSyncedAt.Value.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Test]
    public void ShouldLeaveMetadataPending_WhenPartHasNoVideoStream()
    {
        // Arrange
        var seed = new Seed(12348);
        var mediaData = FakeData.GetPlexMovieMediaData(seed).Generate();
        mediaData.NeedsGeneratedName = true;
        mediaData.GeneratedFilename = null;
        mediaData.GeneratedNameSyncedAt = null;

        var metadata = FakeData.GetLibraryMediaItemDTO(seed, mediaType: PlexMediaType.Movie).Generate();
        var audioStream = FakeData.GetLibraryMediaItemStreamDTO(seed, StreamType.Audio).Generate() with
        {
            Default = true,
        };
        var part = FakeData.GetLibraryMediaItemPartDTO(seed).Generate() with { Stream = [audioStream] };
        var media = metadata.Media.Single() with { Parts = [part] };
        metadata = metadata with { Media = [media] };

        // Act
        var updated = mediaData.UpdateStreamMetadata(metadata, media, part);

        // Assert
        updated.ShouldBeFalse();
        mediaData.GeneratedFilename.ShouldBeNull();
        mediaData.NeedsGeneratedName.ShouldBeTrue();
        mediaData.GeneratedNameSyncedAt.ShouldBeNull();
    }
}
