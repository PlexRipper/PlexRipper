using System.Runtime.CompilerServices;
using Environment;
using Logging.Interface;
using Serilog.Core;
using Serilog.Events;

namespace Logging;

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
        Serilog.Log.Logger = new LogConfig().GetLogger();
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

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public static ILog<T> CreateLogInstance<T>()
        where T : class => new Log<T>(new LogConfig().GetLogger(), typeof(T));

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public static ILog CreateLogInstance(Type classType) => new Log<Type>(new LogConfig().GetLogger(), classType);

    public static void CloseAndFlush()
    {
        Serilog.Log.CloseAndFlush();
    }

    #endregion

    #endregion

    private static readonly ILog _log = CreateLogInstance(typeof(LogManager));
    public static LogEventLevel MinimumLogLevel { get; private set; }
}
