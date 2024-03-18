using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class PlexServerStatusMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial PlexServerStatusDTO ToDTO(this PlexServerStatus plexServer);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<PlexServerStatusDTO> ToDTO(this List<PlexServerStatus> plexServers);

    #endregion

    #region ToEntity

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial PlexServerStatus ToModel(this PlexServerStatusDTO plexServer);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<PlexServerStatus> ToModel(this List<PlexServerStatusDTO> plexServers);

    #endregion
}