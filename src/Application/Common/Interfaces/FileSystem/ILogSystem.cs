using PlexRipper.Domain;
using Serilog;
using Serilog.Core;

namespace PlexRipper.FileSystem
{
    public interface ILogSystem : ISetup
    {
        LoggerConfiguration GetLogFileConfiguration();

        Logger GetLogger();
    }
}