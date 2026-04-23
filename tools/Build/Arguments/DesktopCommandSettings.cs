using Spectre.Console.Cli;

namespace Reaparr.Build;

internal sealed class DesktopCommandSettings : CommandSettings
{
    [CommandOption("-r|--rid <RID>")]
    public string RuntimeIdentifier { get; init; } = string.Empty;

    [CommandOption("--version <VERSION>")]
    public string? Version { get; init; }

    [CommandOption("--informational-version <VERSION>")]
    public string? InformationalVersion { get; init; }

    [CommandOption("--channel <CHANNEL>")]
    public string? Channel { get; init; }

    [CommandOption("--skip-frontend")]
    public bool SkipFrontend { get; init; }

    [CommandOption("--skip-restore")]
    public bool SkipRestore { get; init; }

    [CommandOption("--skip-package")]
    public bool SkipPackage { get; init; }

    [CommandOption("--dry-run")]
    public bool DryRun { get; init; }

    [CommandOption("--frontend-public-dir <PATH>")]
    public string? FrontendPublicDirectory { get; init; }

    [CommandOption("--artifact-dir <PATH>")]
    public string? ArtifactDirectory { get; init; }

    [CommandOption("--preserve-existing-artifacts")]
    public bool PreserveExistingArtifacts { get; init; }
}
