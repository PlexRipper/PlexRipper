namespace PlexRipper.Domain;

public record PlexLibraryMetaData
{
    public required int TvShowCount { get; init; }

    public required int TvShowSeasonCount { get; init; }

    public required int TvShowEpisodeCount { get; init; }

    public required int MovieCount { get; init; }

    public required long MediaSize { get; init; }
}
