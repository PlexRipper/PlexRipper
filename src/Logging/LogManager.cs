using System.Runtime.CompilerServices;
using Logging.Interface;
using Serilog.Core;

namespace Logging;

public static class LogManager
{
    private static readonly ILog _log = LogConfig.GetLog(typeof(LogManager));

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
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                _log.Debug(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case { } s when s.StartsWith("info:"):
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                _log.Information(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case { } s when s.StartsWith("fail:"):
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                _log.Error(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
        }
    }
}