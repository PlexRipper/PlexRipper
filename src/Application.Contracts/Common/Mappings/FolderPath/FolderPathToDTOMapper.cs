using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class FolderPathToDTOMapper
{
    [MapProperty(nameof(FolderPath.DirectoryPath), nameof(FolderPathDTO.Directory))]
    public static partial FolderPathDTO ToDTO(this FolderPath folderPaths);

    [MapProperty(nameof(FolderPath.DirectoryPath), nameof(FolderPathDTO.Directory))]
    public static partial List<FolderPathDTO> ToDTO(this List<FolderPath> folderPaths);
}