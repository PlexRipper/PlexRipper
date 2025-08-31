using Reaparr.Application.Contracts;

namespace Reaparr.Application;

public static class PlexGenreDTOMapper
{
    public static PlexGenreDTO ToDTO(this PlexGenre source) => new() { Id = source.Id, Name = source.Name };

    public static List<PlexGenreDTO> ToDTO(this IEnumerable<PlexGenre> genres) =>
        genres.Select(x => x.ToDTO()).ToList();
}
