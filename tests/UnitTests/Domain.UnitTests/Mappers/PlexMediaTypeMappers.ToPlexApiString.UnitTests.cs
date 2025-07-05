namespace Domain.UnitTests.Mappers;

public partial class PlexMediaTypeMappersUnitTests
{
    [Theory]
    [InlineData(PlexMediaType.Movie, "movie")]
    [InlineData(PlexMediaType.TvShow, "show")]
    [InlineData(PlexMediaType.Season, "season")]
    [InlineData(PlexMediaType.Episode, "episode")]
    [InlineData(PlexMediaType.Artist, "artist")]
    [InlineData(PlexMediaType.Album, "album")]
    [InlineData(PlexMediaType.Song, "track")]
    [InlineData(PlexMediaType.PhotoAlbum, "photoalbum")]
    [InlineData(PlexMediaType.Photos, "photo")]
    public void ShouldConvertEnumToPlexApiString_WhenValidEnumValueProvided(PlexMediaType input, string expected)
    {
        // Act
        var result = input.ToPlexApiString();

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(PlexMediaType.None)]
    [InlineData(PlexMediaType.Music)]
    [InlineData(PlexMediaType.OtherVideos)]
    [InlineData(PlexMediaType.Games)]
    [InlineData(PlexMediaType.Unknown)]
    [InlineData((PlexMediaType)999)]
    [InlineData((PlexMediaType)(-1))]
    [InlineData((PlexMediaType)100)]
    public void ShouldThrowNotImplementedException_WhenUnsupportedEnumValueProvided(PlexMediaType input)
    {
        // Act & Assert
        Should.Throw<NotImplementedException>(() => input.ToPlexApiString());
    }

    [Fact]
    public void ShouldHandleAllSupportedEnumValues_WhenConvertingToPlexApiString()
    {
        // Arrange - These are the enum values that have PlexApi string representations
        var supportedEnumValues = new Dictionary<PlexMediaType, string>
        {
            { PlexMediaType.Movie, "movie" },
            { PlexMediaType.TvShow, "show" },
            { PlexMediaType.Season, "season" },
            { PlexMediaType.Episode, "episode" },
            { PlexMediaType.Artist, "artist" },
            { PlexMediaType.Album, "album" },
            { PlexMediaType.Song, "track" },
            { PlexMediaType.PhotoAlbum, "photoalbum" },
            { PlexMediaType.Photos, "photo" },
        };

        // Act & Assert
        foreach (var (enumValue, expectedString) in supportedEnumValues)
        {
            // Convert enum to PlexApi string
            var result = enumValue.ToPlexApiString();

            // Verify the result matches expected value
            result.ShouldBe(
                expectedString,
                $"Expected {enumValue} to convert to '{expectedString}', but got '{result}'"
            );
        }
    }

    [Fact]
    public void ShouldCompleteRoundTripConversion_WhenConvertingEnumToPlexApiStringAndBack()
    {
        // Arrange - These are the enum values that have PlexApi string representations
        var supportedEnumValues = new[]
        {
            PlexMediaType.Movie,
            PlexMediaType.TvShow,
            PlexMediaType.Season,
            PlexMediaType.Episode,
            PlexMediaType.Artist,
            PlexMediaType.Album,
            PlexMediaType.Song,
            PlexMediaType.PhotoAlbum,
            PlexMediaType.Photos,
        };

        // Act & Assert
        foreach (var originalEnumValue in supportedEnumValues)
        {
            // Convert enum to PlexApi string
            var plexApiString = originalEnumValue.ToPlexApiString();

            // Convert PlexApi string back to enum using ToPlexMediaType
            var convertedBackEnum = plexApiString.ToPlexMediaType();

            // Verify round-trip conversion works perfectly
            convertedBackEnum.ShouldBe(
                originalEnumValue,
                $"Round-trip conversion failed: {originalEnumValue} -> '{plexApiString}' -> {convertedBackEnum}"
            );
        }
    }

    [Fact]
    public void ShouldReturnLowercaseStrings_WhenConvertingValidEnums()
    {
        // Arrange & Act & Assert - Test that strings are in lowercase format
        PlexMediaType.Movie.ToPlexApiString().ShouldBe("movie");
        PlexMediaType.TvShow.ToPlexApiString().ShouldBe("show");
        PlexMediaType.Season.ToPlexApiString().ShouldBe("season");
        PlexMediaType.Episode.ToPlexApiString().ShouldBe("episode");
        PlexMediaType.Artist.ToPlexApiString().ShouldBe("artist");
        PlexMediaType.Album.ToPlexApiString().ShouldBe("album");
        PlexMediaType.Song.ToPlexApiString().ShouldBe("track"); // Note: Song maps to "track"
        PlexMediaType.PhotoAlbum.ToPlexApiString().ShouldBe("photoalbum");
        PlexMediaType.Photos.ToPlexApiString().ShouldBe("photo");
    }

    [Fact]
    public void ShouldReturnConsistentPlexApiResults_WhenCalledMultipleTimes()
    {
        // Arrange
        var testEnumValue = PlexMediaType.Movie;

        // Act
        var result1 = testEnumValue.ToPlexApiString();
        var result2 = testEnumValue.ToPlexApiString();
        var result3 = testEnumValue.ToPlexApiString();

        // Assert
        result1.ShouldBe(result2);
        result2.ShouldBe(result3);
        result1.ShouldBe("movie");
    }

    [Fact]
    public void ShouldThrowForUnsupportedEnumValues_WhenCalledWithNonPlexApiValues()
    {
        // Arrange - These enum values do NOT have PlexApi string representations
        var unsupportedEnumValues = new[]
        {
            PlexMediaType.None,
            PlexMediaType.Music,
            PlexMediaType.OtherVideos,
            PlexMediaType.Games,
            PlexMediaType.Unknown,
        };

        // Act & Assert
        foreach (var enumValue in unsupportedEnumValues)
        {
            Should.Throw<NotImplementedException>(
                () => enumValue.ToPlexApiString(),
                $"Expected {enumValue} to throw NotImplementedException"
            );
        }
    }
}
