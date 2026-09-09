namespace Reaparr.Domain.UnitTests;

public class StringExtensionsUnitTests : BaseUnitTest
{
    [Test]
    [Arguments((string?)null)]
    [Arguments("")]
    [Arguments("http://[::1")]
    [Arguments("not a url")]
    [Arguments("/relative/path")]
    [Arguments("ftp://example.com")]
    public void ShouldReturnFalse_WhenUrlIsNotAnAbsoluteHttpUrl(string? url)
    {
        // Arrange

        // Act
        var result = url.IsValidHttpUrl();

        // Assert
        result.ShouldBeFalse();
    }

    [Test]
    [Arguments("http://example.com")]
    [Arguments("https://example.com")]
    public void ShouldReturnTrue_WhenUrlUsesHttpOrHttpsScheme(string url)
    {
        // Arrange

        // Act
        var result = url.IsValidHttpUrl();

        // Assert
        result.ShouldBeTrue();
    }

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
