using System.Runtime.CompilerServices;
using Logging.Interface;
using Serilog.Core;
using Serilog.Events;
using Xunit.Abstractions;

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
        Serilog.Log.Logger = LogConfig.GetLogger();
        _log.Information("Logging level set to {LogLevel}", MinimumLogLevel);
    }

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public static ILog<T> CreateLogInstance<T>()
        where T : class => new Log<T>(LogConfig.GetLogger(), typeof(T));

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public static ILog CreateLogInstance(ITestOutputHelper output)
    {
        LogConfig.SetTestOutputHelper(output);
        return new Log(LogConfig.GetLogger());
    }

    public static ILog<T> CreateLogInstance<T>(ITestOutputHelper output)
        where T : class
    {
        LogConfig.SetTestOutputHelper(output);
        return new Log<T>(LogConfig.GetLogger(), typeof(T));
    }

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public static ILog CreateLogInstance(Type classType) => new Log<Type>(LogConfig.GetLogger(), classType);

    public static ILog CreateLogInstance(ITestOutputHelper output, Type classType)
    {
        LogConfig.SetTestOutputHelper(output);
        return new Log<Type>(LogConfig.GetLogger(), classType);
    }

    public static void CloseAndFlush()
    {
        Serilog.Log.CloseAndFlush();
    }

    #endregion

    #endregion

    private static readonly ILog _log = CreateLogInstance(typeof(LogManager));
    public static LogEventLevel MinimumLogLevel { get; private set; }
}
