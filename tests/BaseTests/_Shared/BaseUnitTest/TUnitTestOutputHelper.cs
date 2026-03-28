namespace Reaparr.BaseTests;

public sealed class TUnitTestOutputHelper : ITestOutputHelper
{
    private readonly List<string> _messages = [];

    public string Output => string.Join(global::System.Environment.NewLine, _messages);

    public void Write(string message)
    {
        _messages.Add(message);
        TUnit.Core.TestContext.Current?.OutputWriter.Write(message);
    }

    public void Write(string format, params object[] args)
    {
        Write(string.Format(format, args));
    }

    public void WriteLine(string message)
    {
        _messages.Add(message);
        TUnit.Core.TestContext.Current?.OutputWriter.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        WriteLine(string.Format(format, args));
    }
}
