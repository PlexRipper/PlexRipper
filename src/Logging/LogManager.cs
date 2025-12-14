using System.Runtime.CompilerServices;
using Reaparr.Environment;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Reaparr.Logging;

public static class LogManager
{
    public static LogEventLevel MinimumLogLevel { get; private set; }

    [MessageTemplateFormatMethod("messageTemplate")]
    public static void DbContextLogger(
        string messageTemplate,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        switch (messageTemplate)
        {
            // ReSharper disable once StringLiteralTypo
            case { } s when s.StartsWith("dbug:"):
                GetLogger()
                    .Here(sourceFilePath, memberName, sourceLineNumber)
                    .Debug(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case { } s when s.StartsWith("info:"):
                GetLogger()
                    .Here(sourceFilePath, memberName, sourceLineNumber)
                    .Information(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case { } s when s.StartsWith("fail:"):
                GetLogger()
                    .Here(sourceFilePath, memberName, sourceLineNumber)
                    .Error(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
        }
    }

    public static void SetupLogging(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        MinimumLogLevel = minimumLogLevel;
        Log.Logger = new LogConfig().GetLogger(minimumLogLevel);

        GetLogger().Here().Information("Logging level set to {LogLevel}", MinimumLogLevel);

        if (EnvironmentExtensions.IsUnmasked())
        {
            GetLogger()
                .Here()
                .Warning(
                    "Environment variable {UnmaskedKey} has been set to true, which means that sensitive data will be shown in the logs!",
                    EnvironmentExtensions.UnmaskedModeKey
                );

            GetLogger().Here().Warning("This username should be shown: {Username}", "SomeSecretUsername");
        }
    }

    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }

    public static ILogger GetLogger<T>() => Log.Logger.ForContext<T>();

    public static ILogger GetLogger() => Log.Logger;
}
