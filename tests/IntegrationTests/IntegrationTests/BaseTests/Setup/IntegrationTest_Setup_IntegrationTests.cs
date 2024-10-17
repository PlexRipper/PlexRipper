namespace IntegrationTests.BaseTests.Setup;

public class IntegrationTestSetup : BaseIntegrationTests
{
    public IntegrationTestSetup(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveValidApiHttpClient_WhenStartingAnIntegrationTest()
    {
        using var container = await CreateContainer(253);
        container.ApiClient.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHaveAllResolvePropertiesValid_WhenStartingAnIntegrationTest()
    {
        using var container = await CreateContainer(2362);
        container.FileSystem.ShouldNotBeNull();
        container.GetDownloadQueue.ShouldNotBeNull();
        container.GetPlexApiService.ShouldNotBeNull();
        container.Mediator.ShouldNotBeNull();
        container.PathProvider.ShouldNotBeNull();
        container.DbContext.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHaveUniqueInMemoryDatabase_WhenConfigFileIsGivenToContainer()
    {
        // Arrange
        using var container = await CreateContainer(9999);

        // Assert
        container.ShouldNotBeNull();
        container.DbContext.ShouldNotBeNull();
        container.DbContext.DatabaseName.ShouldNotBeEmpty();
        container.DbContext.DatabaseName.ShouldContain("memory_database");
    }

    [Fact]
    public async Task ShouldAllowForMultipleContainersToBeCreated_WhenMultipleAreCalled()
    {
        // Arrange
        using var container = await CreateContainer(3457);
        using var container2 = await CreateContainer(9654);

        // Assert
        container2.ShouldNotBeNull();
        container.DbContext.ShouldNotBeNull();
    }
}
