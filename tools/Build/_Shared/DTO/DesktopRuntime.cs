namespace Reaparr.Build;

internal sealed record DesktopRuntime
{
    public DesktopRuntime(string RuntimeIdentifier, string PublishProfile, string MainExecutable)
    {
        this.RuntimeIdentifier = RuntimeIdentifier;
        this.PublishProfile = PublishProfile;
        this.MainExecutable = MainExecutable;
    }

    public string RuntimeIdentifier { get; init; }
    public string PublishProfile { get; init; }
    public string MainExecutable { get; init; }
}
