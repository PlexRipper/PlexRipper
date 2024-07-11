using Autofac;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using PlexRipper.WebAPI;

namespace PlexRipper.BaseTests;

public class PlexRipperWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _memoryDbName;
    private readonly MockPlexApi _mockPlexApi;
    private static readonly ILog _log = LogManager.CreateLogInstance(typeof(PlexRipperWebApplicationFactory));

    private readonly UnitTestDataConfig _config;

    public PlexRipperWebApplicationFactory(
        string memoryDbName,
        Action<UnitTestDataConfig> options = null,
        MockPlexApi mockPlexApi = null
    )
    {
        _memoryDbName = memoryDbName;
        _mockPlexApi = mockPlexApi;
        _config = UnitTestDataConfig.FromOptions(options);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureContainer<ContainerBuilder>(autoFacBuilder =>
        {
            autoFacBuilder.RegisterModule(
                new TestModule()
                {
                    MemoryDbName = _memoryDbName,
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
}
