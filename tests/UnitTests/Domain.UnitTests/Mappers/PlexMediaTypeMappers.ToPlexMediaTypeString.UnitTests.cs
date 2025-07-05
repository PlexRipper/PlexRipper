namespace Domain.UnitTests.Mappers;

public partial class PlexMediaTypeMappersUnitTests
{
    [Theory]
    [InlineData(PlexMediaType.None, "None")]
    [InlineData(PlexMediaType.Movie, "Movie")]
    [InlineData(PlexMediaType.TvShow, "TvShow")]
    [InlineData(PlexMediaType.Season, "Season")]
    [InlineData(PlexMediaType.Episode, "Episode")]
    [InlineData(PlexMediaType.Music, "Music")]
    [InlineData(PlexMediaType.Artist, "Artist")]
    [InlineData(PlexMediaType.Album, "Album")]
    [InlineData(PlexMediaType.Song, "Song")]
    [InlineData(PlexMediaType.PhotoAlbum, "PhotoAlbum")]
    [InlineData(PlexMediaType.Photos, "Photos")]
    [InlineData(PlexMediaType.OtherVideos, "OtherVideos")]
    [InlineData(PlexMediaType.Games, "Games")]
    [InlineData(PlexMediaType.Unknown, "Unknown")]
    public void ShouldConvertEnumToStringName_WhenValidEnumValueProvided(PlexMediaType input, string expected)
    {
        // Act
        var result = input.ToPlexMediaTypeString();

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData((PlexMediaType)999)]
    [InlineData((PlexMediaType)(-1))]
    [InlineData((PlexMediaType)100)]
    [InlineData((PlexMediaType)int.MaxValue)]
    [InlineData((PlexMediaType)int.MinValue)]
    public void ShouldThrowNotImplementedException_WhenInvalidEnumValueProvided(PlexMediaType input)
    {
        // Act & Assert
        Should.Throw<NotImplementedException>(() => input.ToPlexMediaTypeString());
    }

    [Fact]
    public void ShouldHandleAllEnumValues_WhenConvertingToString()
    {
        // Arrange
        var allEnumValues = Enum.GetValues<PlexMediaType>();

        // Act & Assert
        foreach (var enumValue in allEnumValues)
        {
            // Convert enum to string using ToPlexMediaTypeString
            var result = enumValue.ToPlexMediaTypeString();

            // Verify the result is not null or empty
            result.ShouldNotBeNullOrEmpty($"ToPlexMediaTypeString returned null/empty for {enumValue}");

            // Verify the result matches the enum name
            result.ShouldBe(
                enumValue.ToString(),
                $"Expected {enumValue} to convert to '{enumValue}', but got '{result}'"
            );
        }
    }

    [Fact]
    public void ShouldCompleteRoundTripConversion_WhenConvertingEnumToStringAndBack()
    {
        // Arrange
        var allEnumValues = Enum.GetValues<PlexMediaType>();

        // Act & Assert
        foreach (var originalEnumValue in allEnumValues)
        {
            // Convert enum to string
            var stringValue = originalEnumValue.ToPlexMediaTypeString();

            // Convert string back to enum
            var convertedBackEnum = stringValue.ToPlexMediaType();

            // Verify round-trip conversion works perfectly
            convertedBackEnum.ShouldBe(
                originalEnumValue,
                $"Round-trip conversion failed: {originalEnumValue} -> '{stringValue}' -> {convertedBackEnum}"
            );
        }
    }

    [Fact]
    public void ShouldReturnExpectedStringFormat_WhenConvertingValidEnums()
    {
        // Arrange & Act & Assert - Test that strings match exact enum names
        PlexMediaType.None.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.None));
        PlexMediaType.Movie.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Movie));
        PlexMediaType.TvShow.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.TvShow));
        PlexMediaType.Season.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Season));
        PlexMediaType.Episode.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Episode));
        PlexMediaType.Music.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Music));
        PlexMediaType.Artist.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Artist));
        PlexMediaType.Album.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Album));
        PlexMediaType.Song.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Song));
        PlexMediaType.PhotoAlbum.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.PhotoAlbum));
        PlexMediaType.Photos.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Photos));
        PlexMediaType.OtherVideos.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.OtherVideos));
        PlexMediaType.Games.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Games));
        PlexMediaType.Unknown.ToPlexMediaTypeString().ShouldBe(nameof(PlexMediaType.Unknown));
    }

    [Fact]
    public void ShouldReturnConsistentStringResults_WhenCalledMultipleTimes()
    {
        // Arrange
        var testEnumValue = PlexMediaType.Movie;

        // Act
        var result1 = testEnumValue.ToPlexMediaTypeString();
        var result2 = testEnumValue.ToPlexMediaTypeString();
        var result3 = testEnumValue.ToPlexMediaTypeString();

        // Assert
        result1.ShouldBe(result2);
        result2.ShouldBe(result3);
        result1.ShouldBe("Movie");
    }
}
