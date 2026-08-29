using Autofac;
using Reaparr.Environment;
using Serilog.Sinks.TestCorrelator;

namespace Reaparr.Logging.UnitTests;

public class IntegrationCredentialRedactionUnitTests : BaseUnitTest
{
    [Test]
    public void ShouldMaskIntegrationCredentialPropertiesHeadersAndUrlQueryValues()
    {
        // Arrange
        try
        {
            SetAppRuntimeInfo(x => x.IsUnmasked = false);
            var runtimeInfo = Mock.Container.Resolve<IAppRuntimeInfo>();
            var pathProvider = Mock.Container.Resolve<IPathProvider>();
            LogFactory.SetupLogging(new TestLogConfig(runtimeInfo, pathProvider), runtimeInfo);
            var log = LogFactory.Create<IntegrationCredentialRedactionUnitTests>();
            var secrets = new[] { "arr-secret", "qbittorrent-secret", "torznab-secret", "bearer-secret", "query-secret" };

            using var context = TestCorrelator.CreateContext();

            // Act
            log.Information(
                "{ArrApiKey} {QBittorrentApiKey} {TorznabApiKey} {Authorization}",
                secrets[0],
                secrets[1],
                secrets[2],
                secrets[3]
            );
            log.Information("{Url}", $"https://example.test/api?apikey={secrets[4]}&password={secrets[0]}");

            // Assert
            var messages = TestCorrelator
                .GetLogEventsFromContextId(context.Id)
                .Where(x =>
                    x.MessageTemplate.Text
                    is "{ArrApiKey} {QBittorrentApiKey} {TorznabApiKey} {Authorization}"
                        or "{Url}"
                )
                .Select(x => x.RenderMessage())
                .ToList();
            messages.Count.ShouldBe(2);
            messages.ShouldAllBe(x => x.Contains("***MASKED***"));
            foreach (var secret in secrets)
                messages.ShouldAllBe(x => !x.Contains(secret, StringComparison.Ordinal));
        }
        finally
        {
            LogFactory.CloseAndFlush();
        }
    }
}
