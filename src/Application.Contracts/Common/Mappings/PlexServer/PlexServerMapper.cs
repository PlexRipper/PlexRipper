using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
[UseStaticMapper(typeof(PlexServerConnectionMapper))]
[UseStaticMapper(typeof(PlexServerStatusMapper))]
public static partial class PlexServerMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial PlexServerDTO ToDTO(this PlexServer plexServer);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<PlexServerDTO> ToDTO(this List<PlexServer> plexServers);

    #endregion

    #region ToEntity

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial PlexServer ToModel(this PlexServerDTO plexServer);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<PlexServer> ToModel(this List<PlexServerDTO> plexServers);

    #endregion
}