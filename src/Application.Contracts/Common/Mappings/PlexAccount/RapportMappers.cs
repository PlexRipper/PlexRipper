using PlexRipper.Application;

namespace Application.Contracts;

public static class RapportMappers
{
    public static PlexServerAccessRapportDTO ToDTO(this PlexServerAccessRapport source) =>
        new()
        {
            Created = source.Created,
            Updated = source.Updated,
            Deleted = source.Deleted,
        };

    public static PlexLibraryAccessCrudRapportDTO ToDTO(this PlexLibraryAccessCrudRapport source) =>
        new()
        {
            PlexServerId = source.PlexServerId,
            Created = source.Created,
            Updated = source.Updated,
            Deleted = source.Deleted,
        };

    public static List<PlexLibraryAccessCrudRapportDTO> ToDTO(this List<PlexLibraryAccessCrudRapport> source) =>
        source.Select(ToDTO).ToList();
}
