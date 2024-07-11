namespace Environment;

public static class OsInfo
{
    public static OperatingSystemPlatform CurrentOS { get; }

    public static bool IsNotWindows => !IsWindows;

    public static bool IsLinux => CurrentOS == OperatingSystemPlatform.Linux;

    public static bool IsOsx => CurrentOS == OperatingSystemPlatform.Osx;

    public static bool IsWindows => CurrentOS == OperatingSystemPlatform.Windows;

    static OsInfo()
    {
        var platform = System.Environment.OSVersion.Platform;

        switch (platform)
        {
            case PlatformID.Win32NT:
            {
                CurrentOS = OperatingSystemPlatform.Windows;
                break;
            }

            case PlatformID.MacOSX:
            case PlatformID.Unix:
            {
                // Sometimes Mac OS reports itself as Unix
                if (
                    Directory.Exists("/System/Library/CoreServices/")
                    && (
                        File.Exists("/System/Library/CoreServices/SystemVersion.plist")
                        || File.Exists("/System/Library/CoreServices/ServerVersion.plist")
                    )
                )
                    CurrentOS = OperatingSystemPlatform.Osx;
                else
                    CurrentOS = OperatingSystemPlatform.Linux;

                break;
            }
        }
    }
}
