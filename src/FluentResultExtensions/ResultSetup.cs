using FluentResults;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Reaparr.Logging;

namespace Reaparr.FluentResultExtensions;

public static class FluentResultConfiguration
{
    public static void Setup()
    {
        Result.Setup(cfg =>
        {
            cfg.Logger = new FluentResultLogger();

            cfg.DefaultTryCatchHandler = exception =>
            {
                if (exception is OperationCanceledException canceledException)
                    return new ExceptionalError("Operation was cancelled", canceledException);

                if (exception is ValidationException validationException)
                    return new ExceptionalError("Validation failed", validationException);

                return new ExceptionalError(exception);
            };
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
