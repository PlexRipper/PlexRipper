namespace Reaparr.Application.UnitTests;


public class GetDownloadPreviewQueryValidatorUnitTests : BaseUnitTest<GetDownloadPreviewQueryValidator>
{
    [Test]
    [Arguments(PlexMediaType.PhotoAlbum)]
    [Arguments(PlexMediaType.PhotoImage)]
    public async Task ShouldAcceptPhotoSelection_WhenQualitiesAreEmpty(PlexMediaType type)
    {
        // Arrange
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = type,
                    PlexServerId = 1,
                    PlexLibraryId = 2,
                    MediaIds = [3],
                    Qualities = [],
                },
            ]
        );

        // Act
        var result = await Sut.ValidateAsync(query, CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
    }

    [Test]
    [Arguments(PlexMediaType.PhotoAlbum)]
    [Arguments(PlexMediaType.PhotoImage)]
    public async Task ShouldRejectPhotoSelection_WhenQualitySelectorIsPresent(PlexMediaType type)
    {
        // Arrange
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = type,
                    PlexServerId = 1,
                    PlexLibraryId = 2,
                    MediaIds = [3],
                    Qualities =
                    [
                        new PlexMediaQualityDTO
                        {
                            MediaId = 3,
                            DataId = 4,
                            MediaDataType = PlexMediaType.PhotoImage,
                            VideoQuality = VideoQuality.Unknown,
                        },
                    ],
                },
            ]
        );

        // Act
        var result = await Sut.ValidateAsync(query, CancellationToken);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        result.Errors.Single().ErrorMessage.ShouldBe("Photo selections must not include quality or part selectors.");
    }

    [Test]
    public async Task ShouldStillAcceptMovieQualitySelector_WhenPhotoRuleDoesNotApply()
    {
        // Arrange
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.Movie,
                    PlexServerId = 1,
                    PlexLibraryId = 2,
                    MediaIds = [3],
                    Qualities =
                    [
                        new PlexMediaQualityDTO
                        {
                            MediaId = 3,
                            DataId = 4,
                            MediaDataType = PlexMediaType.Movie,
                            VideoQuality = VideoQuality.Unknown,
                        },
                    ],
                },
            ]
        );

        // Act
        var result = await Sut.ValidateAsync(query, CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
    }

    [Test]
    [Arguments(PlexMediaType.MusicArtist, false)]
    [Arguments(PlexMediaType.MusicArtist, true)]
    [Arguments(PlexMediaType.MusicAlbum, false)]
    [Arguments(PlexMediaType.MusicAlbum, true)]
    [Arguments(PlexMediaType.MusicTrack, false)]
    [Arguments(PlexMediaType.MusicTrack, true)]
    [Arguments(PlexMediaType.OtherVideos, false)]
    [Arguments(PlexMediaType.OtherVideos, true)]
    public async Task ShouldAcceptMusicOrOtherVideoSelection_WhenAloneOrMixedWithMovie(
        PlexMediaType type,
        bool mixedWithMovie
    )
    {
        // Arrange
        var selections = new List<DownloadMediaDTO>
        {
            new()
            {
                Type = type,
                PlexServerId = 1,
                PlexLibraryId = 11,
                MediaIds = [101],
                Qualities = [],
            },
        };
        if (mixedWithMovie)
            selections.Insert(
                0,
                new DownloadMediaDTO
                {
                    Type = PlexMediaType.Movie,
                    PlexServerId = 1,
                    PlexLibraryId = 11,
                    MediaIds = [102],
                    Qualities = [],
                }
            );

        // Act
        var result = await Sut.ValidateAsync(new GetDownloadPreviewQuery(selections), CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.ShouldBeEmpty();
    }

    [Test]
    public async Task ShouldAcceptOrdinaryMixedRequest_WhenEverySupportedSelectionLevelIsPresent()
    {
        // Arrange
        var supportedTypes = new[]
        {
            PlexMediaType.Movie,
            PlexMediaType.TvShow,
            PlexMediaType.Season,
            PlexMediaType.Episode,
            PlexMediaType.MusicArtist,
            PlexMediaType.MusicAlbum,
            PlexMediaType.MusicTrack,
            PlexMediaType.PhotoAlbum,
            PlexMediaType.PhotoImage,
            PlexMediaType.OtherVideos,
        };
        var query = new GetDownloadPreviewQuery(
            supportedTypes
                .Select(
                    (type, index) => new DownloadMediaDTO
                    {
                        Type = type,
                        PlexServerId = index + 1,
                        PlexLibraryId = index + 11,
                        MediaIds = [index + 101],
                        Qualities = [],
                    }
                )
                .ToList()
        );

        // Act
        var result = await Sut.ValidateAsync(query, CancellationToken);

        // Assert
        result.IsValid.ShouldBeTrue();
        result.Errors.Count.ShouldBe(0);
    }

    [Test]
    [Arguments(PlexMediaType.None)]
    [Arguments(PlexMediaType.Games)]
    [Arguments(PlexMediaType.Unknown)]
    [Arguments((PlexMediaType)int.MaxValue)]
    public async Task ShouldRejectUnsupportedMediaType_WhenSelectionIsOtherwiseValid(PlexMediaType type)
    {
        // Arrange
        var query = new GetDownloadPreviewQuery(
            [
                new DownloadMediaDTO
                {
                    Type = type,
                    PlexServerId = 1,
                    PlexLibraryId = 11,
                    MediaIds = [101],
                    Qualities = [],
                },
            ]
        );

        // Act
        var result = await Sut.ValidateAsync(query, CancellationToken);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBe(1);
        result.Errors.Single().PropertyName.ShouldBe("DownloadMedias[0].Type");
    }
}
