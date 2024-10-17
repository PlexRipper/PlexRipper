namespace PlexRipper.Application.UnitTests.Execute;

public class PlexDownloadClient_Setup_UnitTests : BaseUnitTest<PlexDownloadClient>
{
    public PlexDownloadClient_Setup_UnitTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldReturnFailedResult_WhenNullDownloadTaskIsGiven()
    {
        //Arrange
        await SetupDatabase(82345);

        // Act
        var result = await _sut.Setup(
            new DownloadTaskKey
            {
                Type = DownloadTaskType.None,
                Id = default,
                PlexServerId = 0,
                PlexLibraryId = 0,
            }
        );

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.Count.ShouldBeGreaterThan(0);
    }
}
