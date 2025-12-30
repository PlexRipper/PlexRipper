using System.Runtime.CompilerServices;
using Reaparr.Environment;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Reaparr.Logging;

public static class LogFactory
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
                Create().Here(sourceFilePath, memberName, sourceLineNumber).Debug(messageTemplate);
                break;
            case { } s when s.StartsWith("info:"):
                Create().Here(sourceFilePath, memberName, sourceLineNumber).Information(messageTemplate);
                break;
            case { } s when s.StartsWith("fail:"):
                Create().Here(sourceFilePath, memberName, sourceLineNumber).Error(messageTemplate);
                break;
        }
    }

    public static void SetupLogging(LogEventLevel minimumLogLevel = LogEventLevel.Debug, LogConfig? logConfig = null)
    {
        MinimumLogLevel = minimumLogLevel;
        Log.Logger = (logConfig ?? new LogConfig()).GetLogger(minimumLogLevel);

        Create().Here().Information("Logging level set to {LogLevel}", MinimumLogLevel);

        if (EnvironmentExtensions.IsUnmasked())
        {
            Create()
                .Here()
                .Warning(
                    "Environment variable {UnmaskedKey} has been set to true, which means that sensitive data will be shown in the logs!",
                    EnvironmentExtensions.UnmaskedModeKey
                );

            Create().Here().Warning("This username should be shown: {Username}", "SomeSecretUsername");
        }
    }

    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }

    public static ILogger Create<T>() => Log.Logger.ForContext<T>();

    public static ILogger Create(Type classType) => Log.Logger.ForContext(classType);

    public static ILogger Create() => Log.Logger;
}
