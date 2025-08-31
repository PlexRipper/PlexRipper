using System.Runtime.CompilerServices;
using Reaparr.Environment;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Reaparr.Logging;

public static class LogManager
{
    #region Methods

    #region Public

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
                _log.Debug(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case { } s when s.StartsWith("info:"):
                _log.Information(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case { } s when s.StartsWith("fail:"):
                _log.Error(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
        }
    }

    public static void SetupLogging(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        MinimumLogLevel = minimumLogLevel;
        Serilog.Log.Logger = new LogConfig().GetLogger(minimumLogLevel);
        _log.Information("Logging level set to {LogLevel}", MinimumLogLevel);

        if (EnvironmentExtensions.IsUnmasked())
        {
            _log.Warning(
                "Environment variable {UnmaskedKey} has been set to true, which means that sensitive data will be shown in the logs!",
                EnvironmentExtensions.UnmaskedModeKey
            );

            _log.Warning("This username should be shown: {Username}", "SomeSecretUsername");
        }
    }

    public static void CloseAndFlush()
    {
        Serilog.Log.CloseAndFlush();
    }

    #endregion

    #endregion

    private static readonly ILogger _log = new LogConfig().CreateLogInstance(typeof(LogManager));
    public static LogEventLevel MinimumLogLevel { get; private set; }
}
