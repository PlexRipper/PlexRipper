using Serilog;
using Serilog.Core;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    [MessageTemplateFormatMethod("messageTemplate")]
    public static string VerboseMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Verbose(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }
}
