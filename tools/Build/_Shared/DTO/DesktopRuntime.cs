namespace Reaparr.Build;

internal sealed record DesktopRuntime(
    string RuntimeIdentifier,
    string PublishProfile,
    string MainExecutable
);
