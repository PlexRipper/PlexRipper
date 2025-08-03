using Environment;
using Serilog.Sinks.TestCorrelator;

namespace Logging.UnitTests;

public class LogMaskingUnitTests : BaseUnitTest<LogMaskingUnitTests>
{
    #region Setup/Teardown

    public LogMaskingUnitTests(ITestOutputHelper output)
        : base(output) { }

    #endregion

    [Fact]
    public void ShouldHaveMaskedData_WhenLogPropertyIsConfiguredToBeMasked()
    {
        // Arrange
        var originalUnmaskedState = EnvironmentExtensions.IsUnmasked();
        try
        {
            LogManager.CloseAndFlush();
            EnvironmentExtensions.EnableUnmaskedLog(false);
            EnvironmentExtensions.IsUnmasked().ShouldBeFalse();

            var log = Log;
            using (var context = TestCorrelator.CreateContext())
            {
                // Act
                log.Debug("Test e-mail: {Email}", "john.doe@hotmail.com");
                log.Debug("Test ip: {Ip}", "http://182.18.45.10:32400/");
                log.Debug(
                    "Test ip url: {Url}",
                    "http://182.18.45.10:32400/library/sections?X-Plex-Token=PzvkRw39mjZyz4SH8Qcj"
                );
                log.Debug("Test domain name url: {Url}", "https://reykjavik.thetarsus.com/identity");
                log.Debug("Test PlexLibraryTitle property: {PlexLibraryName}", "FORBIDDEN");
                log.Debug("Test PlexAccountDisplayName property: {PlexAccountDisplayName}", "FORBIDDEN");
                log.Debug("Test PlexLibraryName property: {PlexLibraryName}", "FORBIDDEN");
                log.Debug("Test PlexServerName property: {PlexServerName}", "FORBIDDEN");
                log.Debug("Test UserName property: {UserName}", "FORBIDDEN");
                log.Debug("Test PublicAddress property: {PublicAddress}", "FORBIDDEN");
                log.Debug("Test PlexServerConnectionUrl property: {PlexServerConnectionUrl}", "FORBIDDEN");
                log.Debug("Test PlexServerConnection property: {PlexServerConnection}", "FORBIDDEN");
                log.Debug("Test PlexServerStatus property: {PlexServerStatus}", "FORBIDDEN");
                log.Debug("Test DownloadUrl property: {DownloadUrl}", "FORBIDDEN");
                log.Debug("Test AuthToken property: {AuthToken}", "FORBIDDEN");
                log.Debug("Test MachineIdentifier property: {MachineIdentifier}", "FORBIDDEN");

                // Assert - check again right before the assertion to ensure it hasn't changed
                EnvironmentExtensions.EnableUnmaskedLog(false);
                EnvironmentExtensions.IsUnmasked().ShouldBeFalse();

                var logEvents = TestCorrelator.GetLogEventsFromContextId(context.Id).ToList();
                logEvents.ShouldNotBeEmpty();

                foreach (var logEvent in logEvents)
                {
                    var msg = logEvent.RenderMessage();
                    msg.ShouldContain("***MASKED***");
                }
            }
        }
        finally
        {
            // Always restore the previous state and clean up logger
            LogManager.CloseAndFlush();
            EnvironmentExtensions.EnableUnmaskedLog(originalUnmaskedState);
        }
    }
}
