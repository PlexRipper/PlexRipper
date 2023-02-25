using Logging.Common;
using Serilog.Events;

namespace Logging;

public static partial class LogExtensions
{
    public static LogMetaData Error<T0, T1, T2>(
        this LogMetaData logMetaData,
        string messageTemplate,
        T0 propertyValue0,
        T1 propertyValue1,
        T2 propertyValue2)
    {
        logMetaData.Update(LogEventLevel.Error, messageTemplate, propertyValue0, propertyValue1, propertyValue2).Write();
        return logMetaData;
    }
}