using FluentValidation;
using Microsoft.Extensions.Logging;

namespace Reaparr.FluentResultExtensions;

public static class FluentResultConfiguration
{
    public static void Setup()
    {
        Result.Setup(cfg =>
        {
            cfg.Logger = new FluentResultLogger();

            cfg.DefaultTryCatchHandler = exception =>
                exception switch
                {
                    OperationCanceledException canceledException => new ExceptionalError(
                        "Operation was cancelled",
                        canceledException
                    ),
                    ValidationException validationException => new ExceptionalError(
                        "Validation failed",
                        validationException
                    ),
                    _ => new ExceptionalError(exception),
                };
        });
    }
}

public class FluentResultLogger : IResultLogger
{
    public void Log(string context, string? content, ResultBase result, LogLevel logLevel)
    {
        result.LogResultBase(logLevel.ToSerilogLevel());
    }

    public void Log<TContext>(string? content, ResultBase result, LogLevel logLevel)
    {
        result.LogResultBase(logLevel.ToSerilogLevel(), typeof(TContext).FullName!);
    }
}
