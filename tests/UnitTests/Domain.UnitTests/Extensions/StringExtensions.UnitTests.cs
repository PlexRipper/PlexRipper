namespace Reaparr.Domain.UnitTests;

public class StringExtensionsUnitTests : BaseUnitTest
{
    [Test]
    [Arguments("Shaun het Schaap: De Film (2015)")]
    [Arguments("RANDOM MOVIE: # · GREAT")]
    public void ShouldFilterAllInvalidCharsFromName_WhenGivenInvalidName(string testString)
    {
        // Arrange
        testString += Path.GetInvalidFileNameChars().First();

        // Act
        var result = testString.SanitizeFolderName();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldNotContain("  ");
        var invalidChars = Path.GetInvalidFileNameChars();
        result.ShouldAllBe(c => !invalidChars.Contains(c));
    }
}
