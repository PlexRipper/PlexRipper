using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class PlexLibraryMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial PlexLibraryDTO ToDTO(this PlexLibrary plexLibrary);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<PlexLibraryDTO> ToDTO(this List<PlexLibrary> plexLibraries);

    #endregion

    #region ToModel

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial PlexLibrary ToModel(this PlexLibraryDTO plexLibrary);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<PlexLibrary> ToModel(this List<PlexLibraryDTO> plexLibraries);

    #endregion
}