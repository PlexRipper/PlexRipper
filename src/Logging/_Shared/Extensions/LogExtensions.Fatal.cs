namespace Reaparr.Logging;

public static partial class LogExtensions
{
    // This adapter intentionally forwards and renders caller-supplied Serilog templates.
    // ReSharper disable TemplateIsNotCompileTimeConstantProblem
    [MessageTemplateFormatMethod("messageTemplate")]
    public static string FatalMsg(this ILogger log, string messageTemplate, params object?[] args)
    {
        log.Fatal(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }
}
