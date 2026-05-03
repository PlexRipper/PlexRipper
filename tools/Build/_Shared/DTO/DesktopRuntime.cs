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

    public void Deconstruct(out string RuntimeIdentifier, out string PublishProfile, out string MainExecutable)
    {
        RuntimeIdentifier = this.RuntimeIdentifier;
        PublishProfile = this.PublishProfile;
        MainExecutable = this.MainExecutable;
    }
}
