namespace IntegrationTests.BaseTests.Setup;

public class IntegrationTest_Setup : BaseIntegrationTests
{
    public IntegrationTest_Setup(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task ShouldHaveValidApiHttpClient_WhenStartingAnIntegrationTest()
    {
        using var Container = await CreateContainer();
        Container.ApiClient.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHaveAllResolvePropertiesValid_WhenStartingAnIntegrationTest()
    {
        using var Container = await CreateContainer();
        Container.FileSystem.ShouldNotBeNull();
        Container.GetDownloadQueue.ShouldNotBeNull();
        Container.GetPlexApiService.ShouldNotBeNull();
        Container.Mediator.ShouldNotBeNull();
        Container.PathProvider.ShouldNotBeNull();
        Container.DbContext.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHaveUniqueInMemoryDatabase_WhenConfigFileIsGivenToContainer()
    {
        // Arrange
        using var Container = await CreateContainer(x => x.Seed = 9999);

        // Assert
        Container.ShouldNotBeNull();
        Container.DbContext.ShouldNotBeNull();
        Container.DbContext.DatabaseName.ShouldNotBeEmpty();
        Container.DbContext.DatabaseName.ShouldContain("memory_database");
    }

    [Fact]
    public async Task ShouldAllowForMultipleContainersToBeCreated_WhenMultipleAreCalled()
    {
        // Arrange
        using var container = await CreateContainer(x => x.Seed = 3457);
        using var container2 = await CreateContainer(x => x.Seed = 9654);

        // Assert
        container2.ShouldNotBeNull();
        container.DbContext.ShouldNotBeNull();
    }
}
