using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class PlexAccountMapper
{
    #region ToDTO

    public static PlexAccountDTO ToDTO(this PlexAccount plexAccount)
    {
        var dto = plexAccount.ToDTOMapper();
        dto.PlexServerAccess = new List<PlexServerAccessDTO>();

        // Determine the access for each server and libraries
        if (plexAccount.PlexAccountLibraries is not null && plexAccount.PlexAccountServers is not null)
        {
            var plexLibraries = plexAccount.PlexAccountLibraries.Select(y => y.PlexLibrary).ToList();
            foreach (var plexAccountServer in plexAccount.PlexAccountServers)
            {
                var plexServerAccess = plexAccountServer.PlexServer.ToAccessDTO();
                plexServerAccess.PlexLibraryIds = plexLibraries
                    .FindAll(x => x.PlexServerId == plexAccountServer.PlexServerId)
                    .Select(x => x.Id)
                    .ToList();
                dto.PlexServerAccess.Add(plexServerAccess);
            }
        }

        return dto;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial PlexAccountDTO ToDTOMapper(this PlexAccount plexAccount);

    public static List<PlexAccountDTO> ToDTO(this List<PlexAccount> plexAccounts) => plexAccounts.Select(x => x.ToDTO()).ToList();

    #endregion

    #region ToModel

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreTarget(nameof(PlexAccount.PlexServers))]
    [MapperIgnoreTarget(nameof(PlexAccount.PlexAccountServers))]
    public static partial PlexAccount ToModel(this PlexAccountDTO plexAccount);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<PlexAccount> ToModel(this List<PlexAccountDTO> plexAccounts);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial PlexAccount ToModel(this UpdatePlexAccountDTO plexAccount);

    #endregion
}