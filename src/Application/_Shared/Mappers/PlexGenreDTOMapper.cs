namespace Reaparr.Application;

public static class PlexGenreDTOMapper
{
    public static PlexGenreDTO ToDTO(this PlexGenreVideo source) => new() { Id = source.Id, Name = source.Name };

    public static PlexGenreDTO ToDTO(this PlexGenreAudio source) => new() { Id = source.Id, Name = source.Name };

    public static List<PlexGenreDTO> ToDTO(this IEnumerable<PlexGenreVideo> genres) =>
        genres.Select(x => x.ToDTO()).ToList();

    public static List<PlexGenreDTO> ToDTO(this IEnumerable<PlexGenreAudio> genres) =>
        genres.Select(x => x.ToDTO()).ToList();
}
