using FluentResults;
using PlexRipper.Domain;

namespace Application.Contracts;

public interface IFolderPathService
{
    Task<Result<FolderPath>> UpdateFolderPathAsync(FolderPath folderPath);

    Task<Result> CheckIfFolderPathsAreValid(PlexMediaType mediaType = PlexMediaType.None);


    Task<Result<FolderPath>> CreateFolderPath(FolderPath folderPath);

    /// <summary>
    /// Creates a <see cref="Dictionary{TKey,TValue}"/> with <see cref="PlexMediaType"/> keys,
    /// which are mapped to the default <see cref="FolderPath"/> destinations.
    /// </summary>
    /// <returns>Default destination dictionary.</returns>
    Task<Result<Dictionary<PlexMediaType, FolderPath>>> GetDefaultDestinationFolderDictionary();
}