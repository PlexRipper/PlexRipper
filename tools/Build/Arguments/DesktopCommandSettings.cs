using Spectre.Console.Cli;
using ValidationResult = Spectre.Console.ValidationResult;

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

    [CommandOption("--launch-mode <MODE>")]
    public string LaunchMode { get; init; } = "published";

    public override ValidationResult Validate()
    {
        if (string.IsNullOrWhiteSpace(RuntimeIdentifier))
        {
            return ValidationResult.Error(
                $"A runtime identifier is required. Pass --rid <RID>. Supported values: {DesktopRuntimeCatalog.SupportedRuntimeIdentifiers}."
            );
        }

        if (!DesktopRuntimeCatalog.TryGet(RuntimeIdentifier, out _))
        {
            return ValidationResult.Error(
                $"Unsupported desktop RID '{RuntimeIdentifier}'. Supported values: {DesktopRuntimeCatalog.SupportedRuntimeIdentifiers}."
            );
        }

        if (string.IsNullOrWhiteSpace(Version))
        {
            return ValidationResult.Error("A build version is required. Pass --version <VERSION>.");
        }

        if (string.IsNullOrWhiteSpace(InformationalVersion))
        {
            return ValidationResult.Error("An informational version is required. Pass --informational-version <VERSION>.");
        }

        if (
            RuntimeIdentifier.StartsWith("linux-", StringComparison.OrdinalIgnoreCase)
            && !SkipPackage
            && !string.Equals(LaunchMode, "published", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(LaunchMode, "packaged", StringComparison.OrdinalIgnoreCase)
        )
        {
            return ValidationResult.Error(
                $"Unsupported launch mode '{LaunchMode}'. Supported values are 'published' and 'packaged'."
            );
        }

        return ValidationResult.Success();
    }
}

