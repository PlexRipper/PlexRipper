namespace Domain.UnitTests.Converters;

public class HashGenerator_Generate_UnitTests : BaseUnitTest
{
    public HashGenerator_Generate_UnitTests(ITestOutputHelper output): base(output) { }

    [Fact]
    public void ShouldGenerateAValidMD5Hash_WhenGivenAValidDownloadTask()
    {
        // Arrange
        var downloadTask = FakeData.GetMovieDownloadTask().Generate();

        // Act
        var hash = HashGenerator.Generate(downloadTask);

        // Assert
        hash.Length.ShouldBe(32);
    }

    [Fact]
    public void ShouldGenerateTheValidMD5Hash_WhenGivenAValidDownloadTaskTwice()
    {
        // Arrange
        var downloadTask = FakeData.GetMovieDownloadTask().Generate();

        // Act
        var hash1 = HashGenerator.Generate(downloadTask);
        var hash2 = HashGenerator.Generate(downloadTask);

        // Assert
        hash1.Length.ShouldBe(32);
        hash2.Length.ShouldBe(32);
        hash1.ShouldBe(hash2);
    }
}