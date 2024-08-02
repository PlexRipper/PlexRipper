using Logging.Interface;
using Serilog;

namespace Logging;

public class Log<T> : Log, ILog<T>
    where T : class
{
    public Log(ILogger logger, Type classType)
        : base(logger)
    {
        ClassType = classType ?? typeof(T);
    }
}
