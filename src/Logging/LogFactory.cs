using System.Runtime.CompilerServices;

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

    public static void SetupLogging(LogConfig logConfig, LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        MinimumLogLevel = minimumLogLevel;
        Log.Logger = logConfig.GetLogger(minimumLogLevel);
        var log = Create();

        log.Here().Information("Starting Reaparr!");

        log.Here().Information("Logging level set to {LogLevel}", MinimumLogLevel);

        if (EnvironmentExtensions.IsUnmasked())
        {
            log.Here()
                .Warning(
                    "Environment variable {UnmaskedKey} has been set to true, which means that sensitive data will be shown in the logs!",
                    EnvKeys.Unmasked
                );

            log.Here().Warning("This username should be shown: {Username}", "SomeSecretUsername");
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
