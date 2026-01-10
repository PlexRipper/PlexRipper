namespace Reaparr.BackgroundJobs.UnitTests;

public class BasePlexMediaDataMapperGenerateReleaseNameUnitTests : BaseUnitTest
{
    public BasePlexMediaDataMapperGenerateReleaseNameUnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public void ShouldGenerateReleaseName_WhenGivenValidMediaMetadata()
    {
        // Arrange
        var seed = new Seed(12345);
        var mediaItem = FakeData.GetLibraryMediaItemDTO(seed).Generate();
        var media = mediaItem.Media.First();
        var part = media.Parts.First();
        var videoStream = part.Stream.First(s => s.StreamType == StreamType.Video);

        // Map codecs and formats as the actual method does
        var videoCodec = media.VideoCodec.MapVideoCodec();
        var audioCodec = media.AudioCodec.MapAudioCodec(media.AudioProfile);
        var audioLayout = media.AudioChannels.MapToFormattedChannels();
        var languageFormat = part.Stream.FormatLanguage();

        // Determine a source (simplified - using WebDl as default for this test)
        var source = ReleaseSource.WebDl;
        var isRemux = false;

        // Act
        var releaseName = BasePlexMediaDataMapper.GenerateReleaseName(
            title: mediaItem.Title,
            year: mediaItem.Year,
            videoResolution: media.VideoResolution,
            videoCodec: videoCodec,
            audioCodec: audioCodec,
            videoStream: videoStream,
            audioLayout: audioLayout,
            languageFormat: languageFormat,
            source: source,
            isRemux: isRemux
        );

        // Assert
        releaseName.ShouldNotBeNull();
        releaseName.ShouldNotBeEmpty();
        releaseName.ShouldContain(mediaItem.Year.ToString());
        releaseName.ShouldContain(media.VideoResolution.ToResolutionLabel());
        releaseName.ShouldContain(videoCodec);
        releaseName.ShouldContain(audioCodec);
        releaseName.ShouldContain(audioLayout);
    }
}
