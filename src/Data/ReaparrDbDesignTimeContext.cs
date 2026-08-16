using Microsoft.EntityFrameworkCore.Design;

namespace Reaparr.Data;

public class ReaparrDbDesignTimeContext : IDesignTimeDbContextFactory<ReaparrDbContext>
{
    public ReaparrDbContext CreateDbContext(string[] args)
    {
        IAppRuntimeInfo appRuntimeInfo = new AppRuntimeInfo();

        // NOTE: EF Core design-time runs under `dotnet-ef` entry assembly,
        // so AppBuildInfo cannot read Reaparr assembly metadata (RuntimeMode).
        // Force desktop mode for design-time path resolution to avoid
        // PlatformNotSupportedException on Linux hosts.
        IAppBuildInfo designTimeBuildInfo = new DesignTimeAppBuildInfo();
        IPathProvider pathProvider = new PathProvider(designTimeBuildInfo, appRuntimeInfo);

        return new ReaparrDbContext(Log.Logger, pathProvider, appRuntimeInfo);
    }

    private sealed class DesignTimeAppBuildInfo : IAppBuildInfo
    {
        public string RuntimeMode => "desktop";

        public string RuntimeIdentifier => "design-time";

        public string Version => "0.0.0";

        public string InformationalVersion => "0.0.0-design-time";

        public bool IsDesktopMode => true;

        public bool IsDockerMode => false;

        public bool IsDevRelease => true;

        public OperatingSystemPlatform CurrentOS =>
            true switch
            {
                _ when OperatingSystem.IsWindows() => OperatingSystemPlatform.Windows,
                _ when OperatingSystem.IsMacOS() => OperatingSystemPlatform.Osx,
                _ when OperatingSystem.IsLinux() => OperatingSystemPlatform.Linux,
                _ => OperatingSystemPlatform.Unknown,
            };

        public bool IsWindows => OperatingSystem.IsWindows();

        public bool IsLinux => OperatingSystem.IsLinux();

        public bool IsMacOS => OperatingSystem.IsMacOS();
    }
}
