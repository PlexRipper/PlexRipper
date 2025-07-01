using Application.Contracts;

namespace PlexRipper.Application;

public static class PlexCountryDTOMapper
{
    public static PlexCountryDTO ToDTO(this PlexCountry source) => new() { Id = source.Id, Name = source.Name };

    public static List<PlexCountryDTO> ToDTO(this IEnumerable<PlexCountry> countries) =>
        countries.Select(x => x.ToDTO()).ToList();
}
