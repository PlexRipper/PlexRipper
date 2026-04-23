namespace Reaparr.Environment;

public static class OsInfo
{
    // ReSharper disable once InconsistentNaming
    public static OperatingSystemPlatform CurrentOS
    {
        get
        {
            if (OperatingSystem.IsWindows())
                return OperatingSystemPlatform.Windows;

            if (OperatingSystem.IsMacOS())
                return OperatingSystemPlatform.Osx;

            if (OperatingSystem.IsLinux())
                return OperatingSystemPlatform.Linux;

            throw new PlatformNotSupportedException();
        }
    }

    public static bool IsWindows => OperatingSystem.IsWindows();
}
