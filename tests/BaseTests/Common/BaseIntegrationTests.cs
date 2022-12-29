namespace PlexRipper.BaseTests;

public class BaseIntegrationTests : IAsyncLifetime, IAsyncDisposable
{
    protected BaseContainer Container;

    protected BaseIntegrationTests(ITestOutputHelper output)
    {
        Log.SetupTestLogging(output);
    }

    protected async Task CreateContainer(int seed)
    {
        await CreateContainer(config => config.Seed = seed);
    }

    protected async Task CreateContainer([CanBeNull] Action<UnitTestDataConfig> options = null)
    {
        Container = await BaseContainer.Create(options);
    }

    public async Task InitializeAsync()
    {
        Log.Information("Initialize Integration Test");
    }

    public async Task DisposeAsync() { }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        Log.Fatal("Container disposed");
        await Container.Boot.StopAsync(CancellationToken.None);
        Container?.Dispose();
    }
}