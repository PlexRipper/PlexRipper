using System.Runtime.CompilerServices;
using FluentResults;
using Logging.Common;
using Logging.Interface;
using Serilog;

namespace Logging;

public static partial class LogExtensions
{
    public static LogMetaData Here(
        this ILog logger,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        var className = Path.GetFileNameWithoutExtension(sourceFilePath);
        return new LogMetaData(logger, className, memberName, sourceLineNumber);
    }

    public static ILogger Here(
        this ILogger logger,
        [CallerLineNumber] int sourceLineNumber = 0,
        [CallerFilePath] string sourceFilePath = "",
        [CallerMemberName] string memberName = ""
    ) =>
        logger
            .ForContext("MethodName", sourceFilePath)
            .ForContext("MethodName", memberName)
            .ForContext("LineNumber", sourceLineNumber);

    public static Result ToResult(this LogMetaData logMetaData)
    {
        var error = new Error(logMetaData.ToString());
        error.Metadata.Add("ClassName", logMetaData.ClassName);
        error.Metadata.Add("MethodName", logMetaData.MethodName);
        error.Metadata.Add("LineNumber", logMetaData.LineNumber);
        error.Metadata.Add("LogLevel", logMetaData.LogLevel);
        error.Metadata.Add("Exception", logMetaData.Exception);

        return Result.Fail(error);
    }
}
