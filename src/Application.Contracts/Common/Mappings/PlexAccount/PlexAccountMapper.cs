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
        dto.PlexServerAccess = plexAccount.PlexAccountServers.Select(y => y.PlexServer).ToList().ToAccessDTO();
        return dto;
    }

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial PlexAccountDTO ToDTOMapper(this PlexAccount plexAccount);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<PlexAccountDTO> ToDTO(this List<PlexAccount> plexAccounts);

    #endregion

    #region ToModel

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapperIgnoreTarget(nameof(PlexAccount.PlexServers))]
    [MapperIgnoreTarget(nameof(PlexAccount.PlexAccountServers))]
    public static partial PlexAccount ToModel(this PlexAccountDTO plexAccount);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<PlexAccount> ToModel(this List<PlexAccountDTO> plexAccounts);

    #endregion
}