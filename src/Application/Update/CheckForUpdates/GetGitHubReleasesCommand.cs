using System.Net.Http.Json;
using Semver;

namespace Reaparr.Application;

/// <summary>
/// Command to fetch the latest releases from GitHub for the Reaparr repository. This is used to check for updates and notify users of new versions.
/// This also filters to only return new releases based on the current version and if this Reaparr instance is running on the dev or stable channel, so that users only see relevant releases in update notifications.
/// </summary>
public record GetGitHubReleasesCommand : ICommand<Result<IReadOnlyList<ReleaseNote>>>;

public class GetGitHubReleasesCommandValidator : AbstractValidator<GetGitHubReleasesCommand>
{
    public GetGitHubReleasesCommandValidator()
    {
        RuleFor(x => x).NotNull();
    }
}

public class GetGitHubReleasesCommandHandler
    : ICommandHandler<GetGitHubReleasesCommand, Result<IReadOnlyList<ReleaseNote>>>
{
    private readonly ILogger _log;
    private readonly IHttpClientFactory _httpClientFactory;

    public GetGitHubReleasesCommandHandler(ILogger log, IHttpClientFactory httpClientFactory)
    {
        _log = log.ForContext<GetGitHubReleasesCommandHandler>();
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Result<IReadOnlyList<ReleaseNote>>> ExecuteAsync(
        GetGitHubReleasesCommand command,
        CancellationToken cancellationToken
    )
    {
        try
        {
            using var client = _httpClientFactory.CreateGitHubHttpClient();
            var releases = await client.GetFromJsonAsync<IReadOnlyList<GitHubReleaseDTO>>(
                "repos/Reaparr/Reaparr/releases",
                cancellationToken
            );

            var currentVersion = ToSemVersion(EnvironmentExtensions.GetVersion());
            var isDevBuild = EnvironmentExtensions.IsDevRelease();
            var filteredReleases = (releases ?? [])
                .Where(x =>
                    ToSemVersion(x.TagName).ComparePrecedenceTo(currentVersion) > 0 && x.Prerelease == isDevBuild
                )
                .ToList();

            return Result.Ok(filteredReleases.ToReleaseNotes());
        }
        catch (Exception ex)
        {
            _log.Here().Warning(ex, "Failed to fetch GitHub releases");
            return Result.Fail(new ExceptionalError(ex)).LogError();
        }
    }

    private SemVersion ToSemVersion(string version) => SemVersion.Parse(version.TrimStart('v'), SemVersionStyles.Any);
}
