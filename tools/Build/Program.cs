using Reaparr.Logging;
using Serilog;
using Spectre.Console.Cli;

namespace Reaparr.Build;

internal static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var logConfig = new SlimLogConfig();
        Log.Logger = logConfig.GetLogger();
        var log = Log.Logger.ForContext(typeof(Program));

        try
        {
            var container = Startup.CreateContainer(log);
            return await new CommandApp(container).ConfigureConsole(log).RunAsync(args);
        }
        catch (Exception ex)
        {
            log.Here().Fatal(ex, "{Message}", ex.Message);
            return 1;
        }
    }
}
