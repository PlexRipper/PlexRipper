using Autofac.Extras.Quartz;
using Reaparr.Application;
using Reaparr.Settings.Contracts;

namespace Reaparr.BaseTests;

/// <summary>
/// Add the default test mock modules here which can later be overridden
/// </summary>
public class TestModule : Module
{
    public required string MemoryDbName { get; init; }
    public required UnitTestDataConfig Config { get; init; }

    protected override void Load(ContainerBuilder builder)
    {
        // Database context can be setup once and then retrieved by its DB name.
        builder
            .Register((_, _) => MockDatabase.GetMemoryReaparrDbContext(MemoryDbName))
            .As<ReaparrDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryReaparrDbContext(MemoryDbName))
            .As<IReaparrDbContext>()
            .As<IReaparrDbContextDatabase>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(MemoryDbName))
            .As<AuthDbContext>()
            .InstancePerDependency();

        builder
            .Register((_, _) => MockDatabase.GetMemoryAuthDbContext(MemoryDbName))
            .As<IAuthDbContext>()
            .As<IAuthDbContextDatabase>()
            .InstancePerDependency();

        builder.RegisterType<MockProgressHubService>().As<IProgressHubService>().SingleInstance();
        builder.RegisterType<MockDownloadHubService>().As<IDownloadHubService>().SingleInstance();
        builder.RegisterType<MockNotificationHubService>().As<INotificationHubService>().SingleInstance();
        builder.RegisterType<MockPlexApiServer>().As<IMockPlexApiServer>().SingleInstance();

        builder
            .Register((_, _) => Config.OverrideAppBuildInfo ?? new MockAppBuildInfo())
            .As<IAppBuildInfo>()
            .SingleInstance();

        builder
            .Register((ctx, _) => new MockPathProvider(MemoryDbName, ctx.Resolve<IAppBuildInfo>()))
            .As<IPathProvider>()
            .InstancePerDependency();

        SetMockedDependencies(builder);

        // Register Quartz dependencies
        builder.RegisterModule(
            new QuartzAutofacFactoryModule { ConfigurationProvider = _ => QuartzModule.TestQuartzConfiguration() }
        );
    }

    private void SetMockedDependencies(ContainerBuilder builder)
    {
        if (Config.BaseMockHttpClientOptions is not null || Config.HttpClientOptions is not null)
        {
            builder
                .Register(context =>
                {
                    var handler = new Mock<HttpMessageHandler>(MockBehavior.Loose);

                    if (Config.BaseMockHttpClientOptions is not null)
                    {
                        var mockPlexApiServer = context.Resolve<IMockPlexApiServer>();
                        mockPlexApiServer.Setup(handler, Config.BaseMockHttpClientOptions);
                    }

                    // We do not invoke Config.HttpClientOptions here because it requires a per-dependency service (like dbContext)
                    return handler;
                })
                .SingleInstance();

            builder
                .Register(context =>
                {
                    var handler = context.Resolve<Mock<HttpMessageHandler>>();

                    if (Config.HttpClientOptions is not null)
                    {
                        var dbContext = context.Resolve<IReaparrDbContext>();
                        Config.HttpClientOptions.Invoke(handler, dbContext);
                    }

                    var client = new HttpClient(handler.Object, disposeHandler: false);
                    client.DefaultRequestHeaders.Add("User-Agent", "MockHttpClient");
                    return client;
                })
                .As<HttpClient>()
                .InstancePerDependency();
        }

        if (Config.MockConfigManager is not null)
            builder.RegisterInstance(Config.MockConfigManager).As<IConfigManager>();
    }
}
