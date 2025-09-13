using Autofac;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Reaparr.AppHost;

namespace Reaparr.BaseTests;

public class ReaparrWebApplicationFactory : WebApplicationFactory<Program>
{
    public Seed Seed { get; }

    public readonly string MemoryDbName;

    private static readonly ILogger _log = new LogConfig().CreateLogInstance<ReaparrWebApplicationFactory>();

    private readonly UnitTestDataConfig _config;

    public ReaparrWebApplicationFactory(Seed seed, string memoryDbName, Action<UnitTestDataConfig>? options = null)
    {
        this.WithWebHostBuilder(builder =>
        {
            // Disable caching by using custom configurations
            builder.UseSetting("cacheEnabled", "false");

            builder.ConfigureTestServices(services =>
            {
                // https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-9.0#mock-authentication
                services
                    .AddAuthentication(defaultScheme: "TestScheme")
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });
            });
        });

        Seed = seed;

        MemoryDbName = memoryDbName;
        _config = UnitTestDataConfig.FromOptions(options);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureContainer<ContainerBuilder>(autoFacBuilder =>
            autoFacBuilder.RegisterModule(new TestModule { MemoryDbName = MemoryDbName, Config = _config })
        );

        try
        {
            return base.CreateHost(builder);
        }
        catch (Exception e)
        {
            _log.Here().Fatal(e.Message);
            throw;
        }
    }
}
