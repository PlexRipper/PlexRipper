#nullable enable
using System.Runtime.CompilerServices;
using Serilog.Core;

namespace Logging.LogStatic;

public static partial class LogStatic
{
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
                Debug(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case { } s when s.StartsWith("info:"):
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                Information(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
            case { } s when s.StartsWith("fail:"):
                // ReSharper disable once TemplateIsNotCompileTimeConstantProblem
                // TODO Re-enable Error here when implemented
                //Error(messageTemplate, memberName, sourceFilePath, sourceLineNumber);
                break;
        }
    }
}