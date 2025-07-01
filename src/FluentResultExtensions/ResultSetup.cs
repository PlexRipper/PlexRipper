using FluentResults;
using Logging;
using Microsoft.Extensions.Logging;

namespace FluentResultExtensions;

public static class FluentResultConfiguration
{
    public static void Setup()
    {
        Result.Setup(cfg =>
        {
            cfg.Logger = new FluentResultLogger();
            cfg.DefaultTryCatchHandler = exception => new ExceptionalError(exception);
        });
    }
}

public class FluentResultLogger : IResultLogger
{
    public void Log(string context, string content, ResultBase result, LogLevel logLevel)
    {
        result.LogResultBase(logLevel.ToSerilogLevel());
    }

    public void Log<TContext>(string content, ResultBase result, LogLevel logLevel)
    {
        result.LogResultBase(logLevel.ToSerilogLevel(), typeof(TContext).FullName!);
    }
}
