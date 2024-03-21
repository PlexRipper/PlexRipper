using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class FolderPathMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(FolderPath.DirectoryPath), nameof(FolderPathDTO.Directory))]
    public static partial FolderPathDTO ToDTO(this FolderPath folderPaths);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    [MapProperty(nameof(FolderPath.DirectoryPath), nameof(FolderPathDTO.Directory))]
    public static partial List<FolderPathDTO> ToDTO(this List<FolderPath> folderPaths);

    #endregion

    #region ToModel

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(FolderPathDTO.Directory), nameof(FolderPath.DirectoryPath))]
    public static partial FolderPath ToModel(this FolderPathDTO folderPaths);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    [MapProperty(nameof(FolderPathDTO.Directory), nameof(FolderPath.DirectoryPath))]
    public static partial List<FolderPath> ToModel(this List<FolderPathDTO> folderPaths);

    #endregion
}