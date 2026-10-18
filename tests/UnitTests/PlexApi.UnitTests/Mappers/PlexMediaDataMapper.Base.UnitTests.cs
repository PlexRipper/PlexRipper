using Reaparr.BaseTests;

namespace Reaparr.PlexApi.UnitTests;

public class PlexMediaDataMapperBaseUnitTests : BaseUnitTest
{
    [Test]
    [Arguments("flac", true)]
    [Arguments("mkv", true)]
    [Arguments("mp4", false)]
    public void ShouldLeaveReleaseSourceUnknown_WhenContainerDoesNotProveProvenance(
        string container,
        bool optimizedForStreaming
    )
    {
        // Arrange
        var source = FakeData.GetLibraryMediaItemDTO(new Seed(625802), mediaType: PlexMediaType.Movie).Generate();
        var media = source.Media.ShouldHaveSingleItem();
        source.Media[0] = media with
        {
            Container = container,
            OptimizedForStreaming = optimizedForStreaming,
        };

        // Act
        var movie = source.ToPlexMovie();

        // Assert
        movie.MediaDataList.ShouldHaveSingleItem().Source.ShouldBe(ReleaseSource.None);
    }

    [Test]
    [Arguments("vob", "aac", "", ReleaseSource.DVD)]
    [Arguments("m2ts", "truehd", "", ReleaseSource.BluRayRemux)]
    [Arguments("m2ts", "aac", "", ReleaseSource.BluRay)]
    [Arguments("ts", "aac", "", ReleaseSource.HDTV)]
    public void ShouldRetainReleaseSourceHints_ForDiscOrBroadcastContainers(
        string container,
        string audioCodec,
        string audioProfile,
        ReleaseSource expectedSource
    )
    {
        // Arrange
        var source = FakeData.GetLibraryMediaItemDTO(new Seed(625803), mediaType: PlexMediaType.Movie).Generate();
        var media = source.Media.ShouldHaveSingleItem();
        source.Media[0] = media with
        {
            Container = container,
            AudioCodec = audioCodec,
            AudioProfile = audioProfile,
        };

        // Act
        var movie = source.ToPlexMovie();

        // Assert
        movie.MediaDataList.ShouldHaveSingleItem().Source.ShouldBe(expectedSource);
    }

    [Test]
    public void ShouldUseLocalParentGuid_WhenParentRatingKeyIsMissing()
    {
        // Arrange
        var source = FakeData
            .GetLibraryMediaItemDTO(new Seed(625804), mediaType: PlexMediaType.Movie)
            .Generate() with
        {
            Type = PlexMediaType.PhotoImage,
            ParentRatingKey = string.Empty,
            ParentGuid = "local://98765",
        };

        // Act
        var photo = source.ToPlexPhoto();

        // Assert
        photo.ParentKey.ShouldBe(98765);
    }

    [Test]
    public void ShouldPreferParentRatingKey_WhenParentGuidAlsoContainsLocalKey()
    {
        // Arrange
        var source = FakeData
            .GetLibraryMediaItemDTO(new Seed(625805), mediaType: PlexMediaType.Movie)
            .Generate() with
        {
            Type = PlexMediaType.PhotoImage,
            ParentRatingKey = "12345",
            ParentGuid = "local://98765",
        };

        // Act
        var photo = source.ToPlexPhoto();

        // Assert
        photo.ParentKey.ShouldBe(12345);
    }

    [Test]
    [Arguments("")]
    [Arguments("plex://photoalbum/98765")]
    public void ShouldReturnUnknownParent_WhenParentRatingKeyIsMissingAndParentGuidIsNotLocal(string parentGuid)
    {
        // Arrange
        var source = FakeData
            .GetLibraryMediaItemDTO(new Seed(625806), mediaType: PlexMediaType.Movie)
            .Generate() with
        {
            Type = PlexMediaType.PhotoImage,
            ParentRatingKey = string.Empty,
            ParentGuid = parentGuid,
        };

        // Act
        var photo = source.ToPlexPhoto();

        // Assert
        photo.ParentKey.ShouldBe(-1);
    }
}
