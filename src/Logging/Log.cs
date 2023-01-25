using System.Runtime.CompilerServices;

// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace Logging;

public static class Log
{
    private static string FormatForException(this string message, Exception ex)
    {
        return $"{message}: {ex?.ToString() ?? string.Empty}";
    }

    private static string FormatForContext(
        this string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        var fileName = Path.GetFileNameWithoutExtension(sourceFilePath);

        return $"[{fileName}.{memberName}] => {message}";
    }

    #region Verbose

    public static void Verbose(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        Serilog.Log.Verbose(message.FormatForContext(memberName, sourceFilePath));
    }

    public static void Verbose(
        string message,
        Exception ex,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        Serilog.Log.Verbose(
            message
                .FormatForException(ex)
                .FormatForContext(memberName, sourceFilePath));
    }

    public static void Verbose(
        Exception ex,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        Serilog.Log.Verbose(
            (ex != null ? ex.ToString() : string.Empty)
            .FormatForContext(memberName, sourceFilePath)
        );
    }

    #endregion


    #region Fatal

    public static void Fatal(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        FatalAction();

        Serilog.Log.Fatal(message.FormatForContext(memberName, sourceFilePath));
    }

    public static void Fatal(
        string message,
        Exception ex,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        FatalAction();

        Serilog.Log.Fatal(message.FormatForException(ex).FormatForContext(memberName, sourceFilePath));
    }

    public static void Fatal(
        Exception ex,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "")
    {
        FatalAction();

        Serilog.Log.Fatal((ex?.ToString() ?? string.Empty).FormatForContext(memberName, sourceFilePath));
    }

    private static void FatalAction()
    {
        System.Environment.ExitCode = -1;
    }

    #endregion

    public static void CloseAndFlush()
    {
        Serilog.Log.CloseAndFlush();
    }
}