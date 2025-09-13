using Serilog;
using Serilog.Core;

namespace Reaparr.Logging;

public static partial class LogExtensions
{
    [MessageTemplateFormatMethod("messageTemplate")]
    public static string InformationMsg(this ILogger log, string messageTemplate, params object[] args)
    {
        log.Information(messageTemplate, args);
        return log.RenderMessage(messageTemplate, args);
    }
}
