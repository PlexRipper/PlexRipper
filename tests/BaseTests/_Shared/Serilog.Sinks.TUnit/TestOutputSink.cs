using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace Reaparr.BaseTests;

/// <summary>
/// A Serilog sink that writes to an <see cref="ITestOutputHelper"/> (implemented by
/// <see cref="TUnitTestOutputHelper"/>, which forwards to <see cref="TUnit.Core.TestContext.Current"/>'s
/// output writer so log lines appear in the per-test TUnit output panel).
/// </summary>
public sealed class TestOutputSink(ITestOutputHelper testOutputHelper, ITextFormatter formatter) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        var sb = new StringWriter();
        formatter.Format(logEvent, sb);
        testOutputHelper.WriteLine(sb.ToString().Trim());
    }
}
