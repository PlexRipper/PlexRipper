using PlexRipper.Domain;

namespace Application.Contracts;

public static class FolderPathMapper
{
    #region ToDTO

    public static FolderPathDTO ToDTO(this FolderPath folderPath) =>
        new()
        {
            Id = folderPath.Id,
            FolderType = folderPath.FolderType,
            MediaType = folderPath.MediaType,
            DisplayName = folderPath.DisplayName,
            Directory = folderPath.DirectoryPath,
            IsValid = folderPath.IsValid,
            IsDefault = folderPath.Id <= 10,
        };

    public static List<FolderPathDTO> ToDTO(this List<FolderPath> folderPaths) => folderPaths.ConvertAll(ToDTO);

    #endregion

    #region ToModel

    public static FolderPath ToModel(this FolderPathDTO folderPaths) =>
        new()
        {
            Id = folderPaths.Id,
            FolderType = folderPaths.FolderType,
            MediaType = folderPaths.MediaType,
            DisplayName = folderPaths.DisplayName,
            DirectoryPath = folderPaths.Directory,
        };

    public static List<FolderPath> ToModel(this List<FolderPathDTO> folderPaths) =>
        folderPaths.Select(ToModel).ToList();

    #endregion
}
