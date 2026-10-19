using Serilog.Events;
using Serilog.Parsing;

namespace Reaparr.Logging.UnitTests;

public class LogEventMapperUnitTests : BaseUnitTest
{
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public void ShouldPreserveEventIdentityAndContent_WhenHistoryAndLiveDeliveryMapTheSameEvent(bool historyFirst)
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);
        var exception = new InvalidOperationException("Download stopped");
        var logEvent = new LogEvent(
            timestamp,
            LogEventLevel.Warning,
            exception,
            new MessageTemplateParser().Parse("Processed {Count} items"),
            [
                new LogEventProperty("Count", new ScalarValue(42)),
                new LogEventProperty(SlimLogConfig.FileName, new ScalarValue("DownloadWorker.cs")),
                new LogEventProperty(SlimLogConfig.LineNumber, new ScalarValue(123)),
                new LogEventProperty(SlimLogConfig.MethodName, new ScalarValue("Execute")),
            ]
        );
        var buffer = new LogBufferService();

        // Act
        LiveLogEventDTO live;
        if (historyFirst)
        {
            buffer.Emit(logEvent);
            live = logEvent.ToLiveLogEvent();
        }
        else
        {
            live = logEvent.ToLiveLogEvent();
            buffer.Emit(logEvent);
        }

        // Assert
        var history = buffer.GetAll().Single();
        history.Sequence.ShouldBe(live.Sequence);
        history.ShouldBeSameAs(live);
        live.Timestamp.ShouldBe(timestamp);
        live.Level.ShouldBe(LogSeverity.Warning);
        live.Message.ShouldBe("Processed 42 items");
        live.Exception.ShouldBe(exception.ToString());
        live.SourceContext.ShouldBe("DownloadWorker.cs:123.Execute()");
    }

    [Test]
    public void ShouldKeepDistinctEventIdentities_WhenMessagesAndTimestampsAreIdentical()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero);
        var template = new MessageTemplateParser().Parse("Library sync finished");
        var first = new LogEvent(timestamp, LogEventLevel.Information, null, template, []);
        var second = new LogEvent(timestamp, LogEventLevel.Information, null, template, []);
        var buffer = new LogBufferService();

        // Act
        buffer.Emit(first);
        buffer.Emit(second);
        var firstLive = first.ToLiveLogEvent();
        var secondLive = second.ToLiveLogEvent();

        // Assert
        firstLive.Message.ShouldBe("Library sync finished");
        secondLive.Message.ShouldBe("Library sync finished");
        firstLive.Timestamp.ShouldBe(timestamp);
        secondLive.Timestamp.ShouldBe(timestamp);
        secondLive.Sequence.ShouldBeGreaterThan(firstLive.Sequence);
        firstLive.Exception.ShouldBeNull();
        buffer.GetAll().Select(x => x.Sequence).ShouldBe([firstLive.Sequence, secondLive.Sequence]);
        first.ToLiveLogEvent().Sequence.ShouldBe(firstLive.Sequence);
    }

    [Test]
    public void ShouldKeepOneEventIdentity_WhenMappingConcurrently()
    {
        // Arrange
        var logEvent = new LogEvent(
            new DateTimeOffset(2026, 10, 18, 12, 0, 0, TimeSpan.Zero),
            LogEventLevel.Information,
            null,
            new MessageTemplateParser().Parse("Library sync finished"),
            []
        );
        var results = new LiveLogEventDTO[64];
        var buffer = new LogBufferService();

        // Act
        Parallel.For(0, results.Length, index => results[index] = logEvent.ToLiveLogEvent());
        buffer.Emit(logEvent);

        // Assert
        var history = buffer.GetAll().Single();
        results.Select(x => x.Sequence).Distinct().ShouldBe([history.Sequence]);
        foreach (var result in results)
            result.ShouldBeSameAs(history);
        history.Message.ShouldBe("Library sync finished");
    }
}
