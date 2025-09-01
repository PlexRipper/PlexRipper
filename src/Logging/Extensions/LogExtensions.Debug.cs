using Serilog;
using Serilog.Core;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    [MessageTemplateFormatMethod("messageTemplate")]
    public static string DebugMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Debug(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }
}
