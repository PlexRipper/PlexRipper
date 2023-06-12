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
        [CallerMemberName] string memberName = default!,
        [CallerFilePath] string sourceFilePath = default!,
        [CallerLineNumber] int sourceLineNumber = default!)
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
        Serilog.Log.Logger = LogConfig.GetLogger(minimumLogLevel);
    }

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public static ILog<T> CreateLogInstance<T>(LogEventLevel logLevel = LogEventLevel.Debug) where T : class
    {
        return new Log<T>(LogConfig.GetLogger(logLevel));
    }

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public static ILog CreateLogInstance(ITestOutputHelper output, LogEventLevel logLevel = LogEventLevel.Debug)
    {
        LogConfig.SetTestOutputHelper(output);
        return new Log(LogConfig.GetLogger(logLevel));
    }

    public static ILog<T> CreateLogInstance<T>(ITestOutputHelper output, LogEventLevel logLevel = LogEventLevel.Debug) where T : class
    {
        LogConfig.SetTestOutputHelper(output);
        return new Log<T>(LogConfig.GetLogger(logLevel));
    }

    /// <summary>
    /// Returns a new typed <see cref="ILog"/> instance.
    /// </summary>
    /// <returns></returns>
    public static ILog CreateLogInstance(Type classType, LogEventLevel logLevel = LogEventLevel.Debug)
    {
        return new Log<Type>(LogConfig.GetLogger(logLevel), classType);
    }

    public static ILog CreateLogInstance(ITestOutputHelper output, Type classType, LogEventLevel logLevel = LogEventLevel.Debug)
    {
        LogConfig.SetTestOutputHelper(output);
        return new Log<Type>(LogConfig.GetLogger(logLevel), classType);
    }

    public static void CloseAndFlush()
    {
        Serilog.Log.CloseAndFlush();
    }

    #endregion

    #endregion

    private static readonly ILog _log = CreateLogInstance(typeof(LogManager));
}