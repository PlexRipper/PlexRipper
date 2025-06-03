using Application.Contracts;

namespace PlexRipper.Application;

public static class PlexRoleDTOMapper
{
    public static PlexRoleDTO ToDTO(this PlexRole source) => new() { Id = source.Id, Name = source.Name };

    public static List<PlexRoleDTO> ToDTO(this IEnumerable<PlexRole> roles) => roles.Select(x => x.ToDTO()).ToList();
}
