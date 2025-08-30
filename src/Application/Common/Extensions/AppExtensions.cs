using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using Reaparr.Environment;
using Reaparr.Logging;

namespace Reaparr.Application;

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

    private static readonly ILog _log = new LogConfig().CreateLogInstance<AppExtensions>();

    /// <summary>
    ///   Log the identity of the current process, including environment variables and user/group IDs.
    /// </summary>
    public static void LogIdentity(
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0
    )
    {
        if (EnvironmentExtensions.ShouldLogEnvVars())
        {
            var envDict = System.Environment.GetEnvironmentVariables();
            var json = JsonSerializer.Serialize(envDict, DefaultJsonSerializerOptions.UserSettingsOptions);
            _log.Here(sourceFilePath, memberName, sourceLineNumber).Debug("Vars:\n {EnvironmentVars}", json);
        }

        // Retrieve PUID and PGID; guard for non-Unix platforms
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _log.Here(sourceFilePath, memberName, sourceLineNumber)
                .Information(
                    "PUID from env: {EnvPUID} and from the system: {PUID}",
                    EnvironmentExtensions.GetPuid(),
                    getuid()
                );
            _log.Here(sourceFilePath, memberName, sourceLineNumber)
                .Information(
                    "PGID from env: {EnvPGID} and from the system: {PGID}",
                    EnvironmentExtensions.GetPgid(),
                    getgid()
                );
        }
        else
        {
            _log.Here(sourceFilePath, memberName, sourceLineNumber)
                .Information(
                    "Non-Unix OS ({OS}); only env values available. PUID: {EnvPUID}, PGID: {EnvPGID}",
                    RuntimeInformation.OSDescription,
                    EnvironmentExtensions.GetPuid(),
                    EnvironmentExtensions.GetPgid()
                );
        }

        _log.Here(sourceFilePath, memberName, sourceLineNumber)
            .Information("Current system Username: {SystemPUIDName}", System.Environment.UserName);
    }
}
