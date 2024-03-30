namespace Domain.UnitTests;

public class PathSystem_SanitizeFileName_UnitTests : BaseUnitTest
{
    public PathSystem_SanitizeFileName_UnitTests(ITestOutputHelper output) : base(output) { }

    [Theory]
    [InlineData("Shaun het Schaap: De Film (2015)")]
    [InlineData("RANDOM MOVIE: # · GREAT")]
    public void ShouldFilterAllInvalidCharsFromName_WhenGivenInvalidName(string testString)
    {
        // Act
        var result = testString.SanitizeFolderName();

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldNotContain("  ");
        var invalidChars = Path.GetInvalidFileNameChars();
        result.ShouldAllBe(c => !invalidChars.Contains(c));
    }
}