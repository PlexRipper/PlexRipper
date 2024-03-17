using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class PlexServerMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial PlexServerDTO ToDTO(this PlexServer folderPaths);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<PlexServerDTO> ToDTO(this List<PlexServer> folderPaths);

    #endregion

    #region ToEntity

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial PlexServer ToModel(this PlexServerDTO folderPaths);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<PlexServer> ToModel(this List<PlexServerDTO> folderPaths);

    #endregion
}