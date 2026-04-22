namespace Reaparr.Application;

public static class ReleaseNoteMapper
{
    /// <summary>
    /// Maps a GitHub release DTO to a release note.
    /// </summary>
    public static ReleaseNote ToReleaseNote(this GitHubReleaseDTO source) =>
        new()
        {
            Version = source.TagName,
            Notes = source.Body ?? string.Empty,
            ReleaseDate = source.PublishedAt ?? DateTime.MinValue,
            IsDevRelease = source.TagName.Contains("-dev."),
        };

    public static IReadOnlyList<ReleaseNote> ToReleaseNotes(this IReadOnlyList<GitHubReleaseDTO> source) =>
        source.Select(x => x.ToReleaseNote()).ToList();
}
