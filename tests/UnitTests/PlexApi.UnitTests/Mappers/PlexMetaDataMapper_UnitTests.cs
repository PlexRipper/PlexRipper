using PlexRipper.PlexApi;

namespace PlexApi.UnitTests.Mappers;

public class PlexMetaDataMapper_UnitTests : BaseUnitTest
{
    public PlexMetaDataMapper_UnitTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public void Should_When()
    {
        // Arrange
        var metaData = FakePlexApiData.GetLibraryMediaMetadata(PlexMediaType.Movie, config => { config.Seed = 2353; });
        var source = metaData.Generate();

        // Act
        var plexMovie = source.ToPlexMovie();

        // Assert
        plexMovie.ShouldNotBeNull();

        plexMovie.Key.ShouldBe(int.Parse(source.RatingKey));
        plexMovie.MetaDataKey.ShouldBe(465779469);
        plexMovie.Title.ShouldBe(source.Title);
        plexMovie.FullTitle.ShouldBe($"{source.Title} ({source.Year})");
        plexMovie.Year.ShouldBe(source.Year);
        plexMovie.SortTitle.ShouldBe(source.TitleSort);
        plexMovie.Duration.ShouldBe(source.Duration);
        plexMovie.MediaSize.ShouldBe(source.Media.Sum(y => y.Part.Sum(z => z.Size)));
        plexMovie.ChildCount.ShouldBe(source.ChildCount);
        plexMovie.AddedAt.ShouldBe(source.AddedAt);
        plexMovie.UpdatedAt.ShouldBe(source.UpdatedAt);
        plexMovie.Studio.ShouldBe(source.Studio);
        plexMovie.Type.ShouldBe(PlexMediaType.Movie);
        plexMovie.MediaData.ShouldNotBeNull();
        plexMovie.MediaData.MediaData.ShouldNotBeNull();
    }
}