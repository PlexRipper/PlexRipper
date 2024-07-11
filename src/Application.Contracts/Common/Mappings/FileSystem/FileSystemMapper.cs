using PlexRipper.Domain;

namespace Application.Contracts;

public static class FileSystemMapper
{
    #region ToDTO

    public static FileSystemDTO ToDTO(this FileSystemResult value) =>
        new()
        {
            Parent = value.Parent,
            Directories = value.Directories.ToDTO(),
            Files = value.Files.ToDTO(),
        };

    public static List<FileSystemDTO> ToDTO(this List<FileSystemResult> value) => value.ConvertAll(ToDTO);

    public static FileSystemModelDTO ToDTO(this FileSystemModel value) =>
        new()
        {
            Type = value.Type,
            Name = value.Name,
            Path = value.Path,
            Extension = value.Extension,
            Size = value.Size,
            LastModified = value.LastModified,
        };

    public static List<FileSystemModelDTO> ToDTO(this List<FileSystemModel> value) => value.ConvertAll(ToDTO);

    #endregion

    #region ToModel

    public static FileSystemResult ToModel(this FileSystemDTO fileSystemResult) =>
        new()
        {
            Parent = fileSystemResult.Parent,
            Directories = fileSystemResult.Directories.ToModel(),
            Files = fileSystemResult.Files.ToModel(),
        };

    public static List<FileSystemResult> ToModel(this List<FileSystemDTO> fileSystemResult) =>
        fileSystemResult.ConvertAll(ToModel);

    public static FileSystemModel ToModel(this FileSystemModelDTO fileSystemModel) =>
        new()
        {
            Type = fileSystemModel.Type,
            Name = fileSystemModel.Name,
            Path = fileSystemModel.Path,
            Extension = fileSystemModel.Extension,
            Size = fileSystemModel.Size,
            LastModified = fileSystemModel.LastModified,
        };

    public static List<FileSystemModel> ToModel(this List<FileSystemModelDTO> fileSystemModel) =>
        fileSystemModel.ConvertAll(ToModel);

    #endregion
}
