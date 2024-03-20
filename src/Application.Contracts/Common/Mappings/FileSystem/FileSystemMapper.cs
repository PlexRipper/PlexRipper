using PlexRipper.Domain;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
public static partial class FileSystemMapper
{
    #region ToDTO

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial FileSystemDTO ToDTO(this FileSystemResult fileSystemResult);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<FileSystemDTO> ToDTO(this List<FileSystemResult> fileSystemResult);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial FileSystemModelDTO ToDTO(this FileSystemModel fileSystemModel);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    public static partial List<FileSystemModelDTO> ToDTO(this List<FileSystemModel> fileSystemModel);

    #endregion

    #region ToModel

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial FileSystemResult ToModel(this FileSystemDTO fileSystemResult);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<FileSystemResult> ToModel(this List<FileSystemDTO> fileSystemResult);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial FileSystemModel ToModel(this FileSystemModelDTO fileSystemModel);

    [MapperRequiredMapping(RequiredMappingStrategy.Source)]
    public static partial List<FileSystemModel> ToModel(this List<FileSystemModelDTO> fileSystemModel);

    #endregion
}