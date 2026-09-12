namespace Reaparr.Domain.UnitTests;

public class PlexGenreTypeExtensionsUnitTests
{
    [Test]
    [Arguments("Anime", PlexGenreType.Anime)]
    [Arguments(" anime ", PlexGenreType.Anime)]
    [Arguments("Documentary", PlexGenreType.Documentary)]
    [Arguments("DOCUMENTAIRE", PlexGenreType.Documentary)]
    [Arguments("Sport", PlexGenreType.Sport)]
    [Arguments("sports", PlexGenreType.Sport)]
    [Arguments("Animation", PlexGenreType.Unknown)]
    [Arguments("Sportcommentaar", PlexGenreType.Unknown)]
    public void ShouldMapExactNormalizedAlias_WhenGenreIsClassified(string genre, PlexGenreType expected)
    {
        // Arrange
        // Act
        var result = genre.ToPlexGenreType();

        // Assert
        result.ShouldBe(expected);
    }

    [Test]
    [Arguments("Sport / Documentary")]
    [Arguments("Anime, Sport")]
    [Arguments("Documentaire & Anime")]
    [Arguments("Sport;Anime")]
    public void ShouldReturnGroup_WhenMultipleDistinctKnownTypesArePresent(string genre)
    {
        // Arrange
        // Act
        var result = genre.ToPlexGenreType();

        // Assert
        result.ShouldBe(PlexGenreType.Group);
    }

    [Test]
    [Arguments("Sport / Sports", PlexGenreType.Sport)]
    [Arguments("Sport / Ambiguous", PlexGenreType.Sport)]
    [Arguments("Action / Adventure", PlexGenreType.Unknown)]
    public void ShouldIgnoreDuplicateAndUnknownTokens_WhenAtMostOneKnownTypeRemains(
        string genre,
        PlexGenreType expected
    )
    {
        // Arrange
        // Act
        var result = genre.ToPlexGenreType();

        // Assert
        result.ShouldBe(expected);
    }
}
