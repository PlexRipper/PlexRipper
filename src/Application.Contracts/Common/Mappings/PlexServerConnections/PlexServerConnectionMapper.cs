using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
[UseStaticMapper(typeof(PlexServerStatusMapper))]
public static partial class PlexServerConnectionMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial PlexServerConnectionDTO ToDTO(this PlexServerConnection plexServerConnection);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<PlexServerConnectionDTO> ToDTO(this List<PlexServerConnection> plexServerConnections);

    #endregion

    #region ToEntity

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial PlexServerConnection ToModel(this PlexServerConnectionDTO plexServerConnection);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<PlexServerConnection> ToModel(this List<PlexServerConnectionDTO> plexServerConnections);

    #endregion
}