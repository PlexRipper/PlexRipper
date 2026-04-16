using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Reaparr.Application;

/// <summary>
/// Handles application update checks, downloads, and apply for both desktop (Velopack) and Docker deployments.
/// </summary>
public class UpdateManager : IUpdateManager
{
    private readonly ILogger _log;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly Velopack.UpdateManager _velopackManager;

    private const string GitHubRepoOwner = "Reaparr";
    private const string GitHubRepoName = "Reaparr";

    public UpdateManager(ILogger log, IHttpClientFactory httpClientFactory)
    {
        _log = log.ForContext<UpdateManager>();
        _httpClientFactory = httpClientFactory;

        var source = new Velopack.Sources.GithubSource(
            $"https://github.com/{GitHubRepoOwner}/{GitHubRepoName}",
            string.Empty,
            EnvironmentExtensions.IsDevRelease()
        );
        _velopackManager = new Velopack.UpdateManager(source);
    }

    public async Task<AppUpdateCheckResult> CheckForUpdatesAsync()
    {
        var currentVersion = EnvironmentExtensions.GetVersion();
        var isDevRelease = EnvironmentExtensions.IsDevRelease();
        var releases = await FetchGitHubReleasesAsync();

        // Desktop mode
        if (EnvironmentExtensions.IsDesktopMode())
        {
            var updateInfo = await _velopackManager.CheckForUpdatesAsync();

            if (updateInfo is null)
            {
                _log.Here().Information("No update available");
                return AppUpdateCheckResult.NoUpdate();
            }

            var targetVersion = updateInfo.TargetFullRelease.Version.ToString();
            _log.Here().Information("Update available: {Version}", targetVersion);

            return AppUpdateCheckResult.UpdateAvailable(targetVersion, BuildReleaseNotesSince(releases));
        }

        // Docker Mode
        var latest = releases.FirstOrDefault(r => r.Prerelease == isDevRelease);
        if (latest is null)
            return AppUpdateCheckResult.NoUpdate();

        var latestVersion = latest.TagName.TrimStart('v');

        if (!IsNewerThan(latestVersion, currentVersion))
        {
            _log.Here().Information("No update available");
            return AppUpdateCheckResult.NoUpdate();
        }

        _log.Here().Information("Update available: {Version}", latestVersion);

        return AppUpdateCheckResult.UpdateAvailable(latestVersion, BuildReleaseNotesSince(releases));
    }

    public async Task<string?> DownloadUpdateAsync(CancellationToken cancellationToken)
    {
        if (!EnvironmentExtensions.IsDesktopMode())
        {
            _log.Here().Debug("Skipping update download — not running in desktop mode");
            return null;
        }

        // Single network call: fetch latest update info, then download it.
        // Velopack's CheckForUpdatesAsync does not accept a CancellationToken.
        var updateInfo = await _velopackManager!.CheckForUpdatesAsync();
        if (updateInfo is null)
            return null;

        await _velopackManager.DownloadUpdatesAsync(updateInfo, null, cancellationToken);
        return updateInfo.TargetFullRelease.Version.ToString();
    }

    public void ApplyUpdateAndRestart()
    {
        if (!EnvironmentExtensions.IsDesktopMode())
        {
            _log.Here().Debug("Skipping update apply — not running in desktop mode");
            return;
        }

        var asset = _velopackManager!.UpdatePendingRestart;
        _velopackManager.ApplyUpdatesAndRestart(asset, []);
    }

    /// <summary>
    /// Collects release notes for all releases between the current version (exclusive) and the latest (inclusive),
    /// filtered to the active channel, ordered newest first.
    /// </summary>
    private static IReadOnlyList<ReleaseNote> BuildReleaseNotesSince(IReadOnlyList<GitHubRelease> releases)
    {
        var notes = new List<ReleaseNote>();

        foreach (var release in releases)
        {
            if (release.Prerelease != EnvironmentExtensions.IsDevRelease())
                continue;

            var releaseVersion = release.TagName.TrimStart('v');

            if (!IsNewerThan(releaseVersion, EnvironmentExtensions.GetVersion()))
                break; // releases are ordered newest-first; stop when we've passed current

            notes.Add(new ReleaseNote { Version = releaseVersion, Notes = release.Body });
        }

        return notes;
    }

    private async Task<IReadOnlyList<GitHubRelease>> FetchGitHubReleasesAsync()
    {
        try
        {
            using var client = _httpClientFactory.CreateGitHubHttpClient();
            var releases = await client.GetFromJsonAsync<List<GitHubRelease>>(
                $"repos/{GitHubRepoOwner}/{GitHubRepoName}/releases"
            );
            return releases ?? [];
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to fetch GitHub releases");
            return [];
        }
    }

    private static bool IsNewerThan(string candidateVersion, string currentVersion)
    {
        // Strip any pre-release suffix (e.g. "1.2.3-dev.4" → "1.2.3") for comparison
        var candidateCore = candidateVersion.Split('-')[0];
        var currentCore = currentVersion.Split('-')[0];

        if (!Version.TryParse(candidateCore, out var candidate))
            return false;
        if (!Version.TryParse(currentCore, out var current))
            return false;

        return candidate > current;
    }

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; init; } = string.Empty;

        [JsonPropertyName("prerelease")]
        public bool Prerelease { get; init; }

        [JsonPropertyName("body")]
        public string? Body { get; init; }
    }
}
