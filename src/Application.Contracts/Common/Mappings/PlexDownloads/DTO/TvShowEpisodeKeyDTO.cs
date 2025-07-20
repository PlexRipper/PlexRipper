using PlexRipper.Domain;

namespace Application.Contracts;

public record TvShowEpisodeKeyDTO
{
    public required int TvShowId { get; init; }

    public required int SeasonId { get; init; }

    public required int EpisodeId { get; init; }

    public required PlexMediaQuality Quality { get; init; }
}
