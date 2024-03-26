using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
[UseStaticMapper(typeof(PlexServerStatusMapper))]
public static partial class PlexServerConnectionMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(PlexServerConnection.LatestConnectionStatus), nameof(PlexServerConnectionDTO.LatestConnectionStatus))]
    [MapperIgnoreSource(nameof(PlexServerConnection.PlexServer))]
    public static partial PlexServerConnectionDTO ToDTO(this PlexServerConnection plexServerConnection);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<PlexServerConnectionDTO> ToDTO(this List<PlexServerConnection> plexServerConnections);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial ServerConnectionCheckStatusProgressDTO ToDTO(this ServerConnectionCheckStatusProgress serverConnectionCheckStatusProgress);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<ServerConnectionCheckStatusProgressDTO> ToDTO(
        this List<ServerConnectionCheckStatusProgress> serverConnectionCheckStatusProgress);

    #endregion

    #region ToModel

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial PlexServerConnection ToModel(this PlexServerConnectionDTO plexServerConnection);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<PlexServerConnection> ToModel(this List<PlexServerConnectionDTO> plexServerConnections);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial ServerConnectionCheckStatusProgress ToModel(this ServerConnectionCheckStatusProgressDTO serverConnectionCheckStatusProgress);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<ServerConnectionCheckStatusProgress> ToModel(
        this List<ServerConnectionCheckStatusProgressDTO> serverConnectionCheckStatusProgress);

    #endregion
}