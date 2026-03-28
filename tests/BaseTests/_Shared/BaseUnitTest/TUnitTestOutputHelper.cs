namespace Reaparr.BaseTests;

public sealed class TUnitTestOutputHelper : ITestOutputHelper
{
    private readonly List<string> _messages = [];
    private readonly System.Text.StringBuilder _currentLine = new();

    public string Output =>
        _currentLine.Length == 0
            ? string.Join(global::System.Environment.NewLine, _messages)
            : string.Join(global::System.Environment.NewLine, _messages.Append(_currentLine.ToString()));

    public void Write(string message)
    {
        _currentLine.Append(message);
        TUnit.Core.TestContext.Current?.OutputWriter.Write(message);
    }

    public void Write(string format, params object[] args)
    {
        Write(string.Format(format, args));
    }

    public void WriteLine(string message)
    {
        _currentLine.Append(message);
        _messages.Add(_currentLine.ToString());
        _currentLine.Clear();
        TUnit.Core.TestContext.Current?.OutputWriter.WriteLine(message);
    }

    public void WriteLine(string format, params object[] args)
    {
        WriteLine(string.Format(format, args));
    }
}
