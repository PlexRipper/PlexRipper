namespace Reaparr.Data.Contracts;

public record PagedMediaQueryResult
{
    public required List<PlexMediaSlimDTO> Items { get; init; }

    public required int TotalCount { get; init; }
}
