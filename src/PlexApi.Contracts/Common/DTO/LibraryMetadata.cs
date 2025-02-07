using PlexRipper.Domain;

namespace PlexApi.Contracts;

public record LibraryMetadata
{
    public PlexLibrary Library { get; set; }

    public List<PlexCountry> Countries { get; set; } = [];

    public List<PlexGenre> Genres { get; set; } = [];

    public List<PlexRole> Roles { get; set; } = [];
}
