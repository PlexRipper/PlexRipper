using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using Environment;
using Logging.Interface;
using Serilog.Events;

namespace PlexRipper.Application;

public class AppExtensions
{
    /// <summary>
    ///  Get the real user ID of the calling process.
    /// </summary>
    /// <returns></returns>
    [DllImport("libc")]
    public static extern uint getuid();

    /// <summary>
    ///  Get the real group ID of the calling process.
    /// </summary>
    /// <returns></returns>
    [DllImport("libc")]
    public static extern uint getgid();

    public static ILog _log = LogManager.CreateLogInstance(typeof(AppExtensions));

    /// <summary>
    ///   Log the identity of the current process, including environment variables and user/group IDs.
    /// </summary>
    public static void LogIdentity(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        if (_log.IsLogLevelEnabled(LogEventLevel.Verbose))
        {
            var envDict = System.Environment.GetEnvironmentVariables();
            var json = JsonSerializer.Serialize(envDict, DefaultJsonSerializerOptions.UserSettingsOptions);
            _log.Verbose("Vars:\n {EnvironmentVars}", json);
        }

        // Retrieve PUID and PGID from environment variables
        _log.Here(memberName, sourceFilePath, sourceLineNumber)
            .Debug("PUID from env: {PUID} and from the system: {PUID}", EnvironmentExtensions.GetPuid(), getuid());

        _log.Here(memberName, sourceFilePath, sourceLineNumber)
            .Debug("PGID from env: {PGID} and from the system: {PGID}", EnvironmentExtensions.GetPgid(), getgid());

        _log.Here(memberName, sourceFilePath, sourceLineNumber)
            .Debug("Current system Username: {SystemPUIDName}", System.Environment.UserName);
    }
}
