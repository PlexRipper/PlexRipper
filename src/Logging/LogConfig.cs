using Serilog.Enrichers.Sensitive;
using Serilog.Filters;

namespace Reaparr.Logging;

public class LogConfig : SlimLogConfig
{
    private readonly IAppRuntimeInfo _appRuntimeInfo;
    private readonly IPathProvider _pathProvider;

    protected LogConfig(IAppRuntimeInfo appRuntimeInfo, IPathProvider pathProvider)
    {
        ArgumentNullException.ThrowIfNull(pathProvider);
        _appRuntimeInfo = appRuntimeInfo;
        _pathProvider = pathProvider;
    }

    /// <summary>
    /// Provides a base configuration with console and debug sinks, and allows for extension by derived classes (e.g. to add file or Seq sinks).
    /// </summary>
    /// <param name="minimumLogLevel">The global minimum log level used before sink-specific filtering.</param>
    /// <returns></returns>
    protected override LoggerConfiguration GetBaseConfiguration(LogEventLevel minimumLogLevel = LogEventLevel.Debug)
    {
        
        var config = base.GetBaseConfiguration(minimumLogLevel);
        
        // Do not mask data when debugging
        if (!_appRuntimeInfo.IsUnmasked)
        {
            config.Enrich.WithSensitiveDataMasking(options =>
            {
                options.MaskingOperators.Clear();
                options.MaskingOperators = [new EmailAddressMaskingOperator(), new UrlMaskingOperator()];
                var sensitiveProperties = new[]
                {
                    "PlexLibraryTitle",
                    "PlexAccountDisplayName",
                    "PlexLibraryName",
                    "PlexServerName",
                    "UserName",
                    "PublicAddress",
                    "PlexServerConnectionUrl",
                    "PlexServerConnection",
                    "PlexServerStatus",
                    "DownloadUrl",
                    "AuthToken",
                    "AccessToken",
                    "RefreshToken",
                    "PlexAuthToken",
                    "Token",
                    "Password",
                    "ApiKey",
                    "VerificationCode",
                    "Authorization",
                    "SID",
                    "MachineIdentifier",
                };

                foreach (var property in sensitiveProperties)
                {
                    options.MaskProperties.Add(MaskProperty.WithDefaults(property));
                }
            });
        }

        return config;
    }

    /// <summary>
    /// Provides an extended sink configuration with file and Seq sinks.
    /// </summary>
    /// <param name="minimumLogLevel">The global minimum log level used before sink-specific filtering.</param>
    protected virtual LoggerConfiguration GetExtendedConfiguration(
        LogEventLevel minimumLogLevel = LogEventLevel.Debug
    ) => GetBaseConfiguration(minimumLogLevel)
        .WriteTo.Seq(_appRuntimeInfo.SEQ_Url, restrictedToMinimumLevel: minimumLogLevel)
        .WriteTo.File(
            FileTemplate,
            Path.Combine(_pathProvider.LogsDirectory, "log.txt"),
            restrictedToMinimumLevel: minimumLogLevel,
            rollingInterval: RollingInterval.Day,
            rollOnFileSizeLimit: true,
            retainedFileCountLimit: 7
        );

    public override Logger GetLogger(LogEventLevel minimumLogLevel = LogEventLevel.Debug) =>
        GetExtendedConfiguration(minimumLogLevel).CreateLogger();
}