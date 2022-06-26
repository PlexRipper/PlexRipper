namespace PlexRipper.Application
{
    public interface IFolderPathService
    {
        Task<Result<List<FolderPath>>> GetAllFolderPathsAsync();

        Task<Result<FolderPath>> UpdateFolderPathAsync(FolderPath folderPath);

        Task<Result> CheckIfFolderPathsAreValid(PlexMediaType mediaType = PlexMediaType.None);

        /// <summary>
        /// Get the download <see cref="FolderPath"/>.
        /// </summary>
        /// <returns>The <see cref="FolderPath"/> of the download folder.</returns>
        Task<Result<FolderPath>> GetDownloadFolderAsync();

        /// <summary>
        /// Get the destination <see cref="FolderPath"/> for Movie downloads.
        /// </summary>
        /// <returns>Destination <see cref="FolderPath"/> for Movies.</returns>
        Task<Result<FolderPath>> GetMovieDestinationFolderAsync();

        /// <summary>
        /// Get the destination <see cref="FolderPath"/> for TvShow downloads.
        /// </summary>
        /// <returns>Destination <see cref="FolderPath"/> for TvShows.</returns>
        Task<Result<FolderPath>> GetTvShowDestinationFolderAsync();

        Task<Result<FolderPath>> GetDestinationFolderByMediaType(PlexMediaType mediaType);

        Task<Result<FolderPath>> CreateFolderPath(FolderPath folderPath);

        Task<Result> DeleteFolderPathAsync(int folderPathId);

        /// <summary>
        /// Creates a <see cref="Dictionary{TKey,TValue}"/> with <see cref="PlexMediaType"/> keys,
        /// which are mapped to the default <see cref="FolderPath"/> destinations.
        /// </summary>
        /// <returns>Default destination dictionary.</returns>
        Task<Result<Dictionary<PlexMediaType, FolderPath>>> GetDefaultDestinationFolderDictionary();
    }
}