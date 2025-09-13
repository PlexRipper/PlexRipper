using Serilog;
using Serilog.Core;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    [MessageTemplateFormatMethod("messageTemplate")]
    public static string FatalMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Fatal(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }
}
