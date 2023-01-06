using PlexRipper.FileSystem;

namespace FileSystem.UnitTests;

public class PathSystem_SanitizeFileName_UnitTests : BaseUnitTest<PathSystem>
{
    public PathSystem_SanitizeFileName_UnitTests(ITestOutputHelper output) : base(output) { }


    [Theory]
    [InlineData("Shaun het Schaap: De Film (2015)")]
    [InlineData("RANDOM MOVIE: # · GREAT")]
    public void ShouldFilterAllInvalidCharsFromName_WhenGivenInvalidName(string testString)
    {
        // Act
        var result = _sut.SanitizePath(testString);

        // Assert
        result.ShouldNotBeNullOrEmpty();
        result.ShouldNotContain("  ");
        var invalidChars = Path.GetInvalidFileNameChars();
        result.ShouldAllBe(c => !invalidChars.Contains(c));
    }
}