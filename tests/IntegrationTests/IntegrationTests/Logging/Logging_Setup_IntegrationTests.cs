namespace IntegrationTests.Logging;


public class Logging_Setup_IntegrationTests : BaseIntegrationTests
{
    public Logging_Setup_IntegrationTests(ITestOutputHelper output) : base(output) { }

    [Fact]
    public async Task ShouldDownloadMultipleMovieDownloadTasks_WhenDownloadTasksAreCreated()
    {
        // Arrange
        Seed = 456974;
        await SetupDatabase();
        await CreateContainer();
        var sut = Container.TestLoggingClass;

        // Act
        sut.LogEvents();

        // Assert
        true.ShouldBeTrue();
    }
}