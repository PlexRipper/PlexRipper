using Autofac;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using PlexRipper.WebAPI;

namespace PlexRipper.BaseTests;

public class PlexRipperWebApplicationFactory : WebApplicationFactory<Program>
{
    public readonly string MemoryDbName;

    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(PlexRipperWebApplicationFactory));
    private MockPlexApi? _mockPlexApi;
    public readonly List<PlexMockServer> PlexMockServers = [];

    private readonly UnitTestDataConfig _config;

    public PlexRipperWebApplicationFactory(string memoryDbName, Action<UnitTestDataConfig>? options = null)
    {
        MemoryDbName = memoryDbName;
        _config = UnitTestDataConfig.FromOptions(options);
        SetupPlexMockServers(_config);
    }

    private void SetupPlexMockServers(UnitTestDataConfig config)
    {
        var mockConfig = MockPlexApiConfig.FromOptions(config.PlexMockApiOptions);
        _mockPlexApi = config.PlexMockApiOptions != null ? new MockPlexApi(_log, _config.PlexMockApiOptions) : null;

        foreach (var serverConfig in mockConfig.MockServers)
            PlexMockServers.Add(new PlexMockServer(serverConfig));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureContainer<ContainerBuilder>(autoFacBuilder =>
        {
            autoFacBuilder.RegisterModule(
                new TestModule()
                {
                    MemoryDbName = MemoryDbName,
                    MockPlexApi = _mockPlexApi,
                    Config = _config,
                }
            );
        });

        try
        {
            return base.CreateHost(builder);
        }
        catch (Exception e)
        {
            _log.Fatal(e);
            throw;
        }
    }

    protected override void Dispose(bool disposing)
    {
        PlexMockServers.ForEach(server => server.Dispose());

        base.Dispose(disposing);
    }
}
