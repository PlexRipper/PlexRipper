namespace Reaparr.Application;

internal sealed class PhotoDownloadSelector
{
    private readonly IReaparrDbContext _dbContext;
    private readonly ILogger _log;

    public PhotoDownloadSelector(IReaparrDbContext dbContext, ILogger log)
    {
        _dbContext = dbContext;
        _log = log;
    }

    public async Task<Result<List<PhotoDownloadSelection>>> ResolveSelectedFilesAsync(
        CreateDownloadTasksRequest request,
        CancellationToken cancellationToken
    )
    {
        var selections = request.DownloadMedias
            .Where(x => x.Type is PlexMediaType.PhotoAlbum or PlexMediaType.PhotoImage)
            .ToList();
        var libraryIds = selections.Select(x => x.PlexLibraryId).Distinct().ToList();
        var libraries = await _dbContext.PlexLibraries
            .Where(x => libraryIds.Contains(x.Id) && x.Type == PlexMediaType.PhotoAlbum)
            .Select(x => new { x.Id, x.PlexServerId, x.Title, ServerName = x.PlexServer!.Name })
            .ToListAsync(cancellationToken);
        var libraryRootNames = libraries.ToDictionary(x => (x.PlexServerId, x.Id), x =>
        {
            var name = $"{x.ServerName.SanitizeFolderName()} - {x.Title.SanitizeFolderName()}".SanitizeFolderName();
            return string.IsNullOrWhiteSpace(name) ? $"server-{x.PlexServerId}-library-{x.Id}" : name;
        });
        if (selections.Any(x => !libraryRootNames.ContainsKey((x.PlexServerId, x.PlexLibraryId))))
            return ResultExtensions.Create400BadRequestResult(
                "The selected photo library does not match the requested server or media type."
            );

        var selected = new List<SelectedPhoto>();
        foreach (var scope in selections.GroupBy(x => (x.PlexServerId, x.PlexLibraryId)))
        {
            var result = await ResolveSelectedFilesForScopeAsync(scope, cancellationToken);
            if (result.IsCancelledOrFailed)
                return result.ToResult<List<PhotoDownloadSelection>>().LogIfFailed();
            selected.AddRange(result.Value);
        }
        if (selected.Count == 0)
            return Result.Ok(new List<PhotoDownloadSelection>());

        var destinationResult = await ResolveDestinationRootsAsync(
            request,
            selected.Select(x => x.MediaData.PlexLibraryId).Distinct().ToList()
        );
        if (destinationResult.IsCancelledOrFailed)
            return destinationResult.ToResult<List<PhotoDownloadSelection>>().LogIfFailed();

        var destinations = destinationResult.Value;
        var downloadRoot = NormalizeRootPath((await _dbContext.GetDownloadFolder(null)).DirectoryPath);
        var folders = await _dbContext.PlexPhotoFolders
            .Where(x => libraryIds.Contains(x.PlexLibraryId))
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var activeRoots = folders.Values
            .Where(x => x.ParentId is null && x.IsActive == true)
            .ToLookup(x => (x.PlexServerId, x.PlexLibraryId));
        var rootsByFolder = new Dictionary<int, PlexPhotoFolder?>();
        var photos = new List<PhotoDownloadSelection>();
        foreach (var item in selected.OrderBy(x => x.MediaData.Id))
        {
            var media = item.MediaData;
            var scope = (media.PlexServerId, media.PlexLibraryId);
            var scopeRoots = activeRoots[scope];
            var folder = media.PlexPhotoFolderId is int folderId
                ? folders.GetValueOrDefault(folderId)
                : null;
            PlexPhotoFolder? root = null;
            if (folder is not null && !rootsByFolder.TryGetValue(folder.Id, out root))
                rootsByFolder[folder.Id] = root = FindRoot(folder, folders);
            if (folder is null || root is null || !scopeRoots.Any(x => x.Id == root.Id))
            {
                _log.Here().Warning(
                    "Skipping photo {Title} (ID: {Id}) because it has no active catalog folder",
                    media.Title,
                    media.Id
                );
                continue;
            }

            var relativeDirectory = folder.RelativePath.Trim('/');
            if (scopeRoots.Count() > 1)
                relativeDirectory = string.IsNullOrEmpty(relativeDirectory)
                    ? $"root-{root.Id}"
                    : $"root-{root.Id}/{relativeDirectory}";
            photos.Add(
                new PhotoDownloadSelection(
                    media,
                    relativeDirectory,
                    media.OriginalFilename,
                    item.KeepCompleted,
                    downloadRoot,
                    destinations[media.PlexLibraryId],
                    libraryRootNames[scope]
                )
            );
        }

        return Result.Ok(photos);
    }

    public List<PhotoDownloadCandidate> FilterQueueEligibleFiles(
        IReadOnlyCollection<PhotoDownloadSelection> selections,
        IReadOnlyCollection<DownloadTaskPhotoFolder> existingFolders,
        IReadOnlyCollection<DownloadTaskPhotoImageFile> existingFiles
    )
    {
        var rootNames = ResolveQueueRootNames(selections, existingFolders);
        var existing = existingFiles
            .Select(x =>
                (
                    GetIdentity(x),
                    DownloadRoot: NormalizeRootPath(x.DirectoryMeta.DownloadRootPath),
                    DestinationRoot: NormalizeRootPath(x.DirectoryMeta.DestinationRootPath)
                )
            )
            .ToHashSet();
        var pathOwners = new Dictionary<string, HashSet<PhotoIdentity>>(StringComparer.OrdinalIgnoreCase);
        foreach (var file in existingFiles.Where(x => x.DownloadStatus != DownloadStatus.Deleted))
        {
            if (!TryGetDestinationPath(file.DirectoryMeta, file.FileName, out var path))
                continue;
            if (!pathOwners.TryGetValue(path, out var owners))
                pathOwners[path] = owners = [];
            owners.Add(GetIdentity(file));
        }

        var candidates = new List<(PhotoDownloadCandidate Candidate, PhotoIdentity Identity, string Path)>();
        foreach (var selection in selections)
        {
            var rootName = rootNames[(selection.MediaData.PlexServerId, selection.MediaData.PlexLibraryId)];
            var namedSelection = selection.RootName == rootName ? selection : selection with { RootName = rootName };
            var identity = GetIdentity(namedSelection.MediaData);
            if (existing.Contains(
                    (
                        identity,
                        namedSelection.DownloadRootPath,
                        namedSelection.DestinationRootPath
                    )
                ))
                continue;

            var directoryMeta = namedSelection.CreateDirectoryMetadata();
            if (TryGetDestinationPath(directoryMeta, namedSelection.FileName, out var path))
                candidates.Add((new(namedSelection, directoryMeta), identity, path));
            else
                LogInvalidDestination(namedSelection);
        }

        var accepted = new List<PhotoDownloadCandidate>();
        foreach (var group in candidates.GroupBy(x => x.Path, StringComparer.OrdinalIgnoreCase))
        {
            // All new participants are rejected when different assets target the same path.
            var identity = group.First().Identity;
            var collides = group.Any(x => x.Identity != identity)
                || (
                    pathOwners.TryGetValue(group.Key, out var owners)
                    && owners.Any(x => x != identity)
                );
            if (collides)
            {
                foreach (var candidate in group)
                    LogInvalidDestination(candidate.Candidate.Selection);
                continue;
            }

            accepted.AddRange(group.Select(x => x.Candidate));
        }
        return accepted;
    }

    internal static string NormalizeRootPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;
        try
        {
            return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return path;
        }
    }

    private async Task<Result<List<SelectedPhoto>>> ResolveSelectedFilesForScopeAsync(
        IGrouping<(int ServerId, int LibraryId), DownloadMediaDTO> selections,
        CancellationToken cancellationToken
    )
    {
        var (serverId, libraryId) = selections.Key;
        var albumIds = selections.Where(x => x.Type == PlexMediaType.PhotoAlbum)
            .SelectMany(x => x.MediaIds).Distinct().ToList();
        var albums = await _dbContext.PlexPhotoAlbums
            .Where(x => albumIds.Contains(x.Id) && x.PlexServerId == serverId && x.PlexLibraryId == libraryId)
            .Select(x => x.Id).ToListAsync(cancellationToken);
        if (albums.Count != albumIds.Count)
            return ResultExtensions.Create400BadRequestResult(
                "Every selected photo album must belong to the requested library and server."
            );

        var memberships = await _dbContext.PlexPhotoAlbumMemberships
            .Include(x => x.PlexPhotoMediaData)
            .Where(x => albumIds.Contains(x.PlexPhotoAlbumId)
                && x.PlexPhotoMediaData!.PlexServerId == serverId
                && x.PlexPhotoMediaData.PlexLibraryId == libraryId)
            .ToListAsync(cancellationToken);
        var filesByAlbum = memberships.ToLookup(x => x.PlexPhotoAlbumId, x => x.PlexPhotoMediaData!);
        var directIds = selections.Where(x => x.Type == PlexMediaType.PhotoImage)
            .SelectMany(x => x.MediaIds).Distinct().ToList();
        var directFiles = await _dbContext.PlexPhotoMediaData
            .Where(x => directIds.Contains(x.Id) && x.PlexServerId == serverId && x.PlexLibraryId == libraryId)
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var choices = new List<(PlexPhotoMediaData File, bool KeepCompleted, bool Direct)>();
        foreach (var selection in selections)
        {
            foreach (var id in selection.MediaIds.Distinct())
            {
                if (selection.Type == PlexMediaType.PhotoAlbum)
                    choices.AddRange(filesByAlbum[id].Select(file =>
                        (file, selection.KeepCompletedInDownloadFolder, Direct: false)));
                else if (directFiles.TryGetValue(id, out var file))
                    choices.Add((file, selection.KeepCompletedInDownloadFolder, Direct: true));
            }
        }

        var selected = new List<SelectedPhoto>();
        foreach (var file in choices.GroupBy(x => x.File.Id))
        {
            // Direct selection overrides album options; peers must agree, regardless of request order.
            var effective = file.Any(x => x.Direct) ? file.Where(x => x.Direct) : file;
            var options = effective.Select(x => x.KeepCompleted).Distinct().ToList();
            if (options.Count > 1)
                return ResultExtensions.Create400BadRequestResult(
                    "Conflicting photo KeepCompletedInDownloadFolder values were supplied."
                );
            selected.Add(new(file.First().File, options[0]));
        }
        return Result.Ok(selected);
    }

    private async Task<Result<Dictionary<int, string>>> ResolveDestinationRootsAsync(
        CreateDownloadTasksRequest request,
        IEnumerable<int> libraryIds
    )
    {
        var destinationOverride = request.CustomDestinationFolderPath;
        if (string.IsNullOrWhiteSpace(destinationOverride) && request.DestinationFolderPathId is int destinationId)
        {
            destinationOverride = await _dbContext.FolderPaths.Where(x => x.Id == destinationId)
                .Select(x => x.DirectoryPath).FirstOrDefaultAsync();
            if (destinationOverride is null)
                return ResultExtensions.EntityNotFound(nameof(FolderPath), destinationId);
        }

        var destinations = new Dictionary<int, string>();
        foreach (var libraryId in libraryIds)
        {
            var destination = string.IsNullOrWhiteSpace(destinationOverride)
                ? (await _dbContext.GetDestinationFolder(libraryId))?.DirectoryPath
                : destinationOverride;
            if (string.IsNullOrWhiteSpace(destination))
                return ResultExtensions.EntityNotFound(nameof(FolderPath), libraryId);
            destinations[libraryId] = NormalizeRootPath(destination);
        }
        return Result.Ok(destinations);
    }

    private static Dictionary<(int ServerId, int LibraryId), string> ResolveQueueRootNames(
        IEnumerable<PhotoDownloadSelection> selections,
        IEnumerable<DownloadTaskPhotoFolder> folders
    )
    {
        var existing = folders.Where(x => x.ParentId is null)
            .GroupBy(x => (
                x.PlexServerId,
                x.PlexLibraryId,
                DownloadRoot: NormalizeRootPath(x.DownloadRootPath),
                DestinationRoot: NormalizeRootPath(x.DestinationRootPath)))
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.CreatedAt).First().Title);
        var libraries = selections.DistinctBy(x => (x.MediaData.PlexServerId, x.MediaData.PlexLibraryId)).ToList();
        var names = new Dictionary<(int ServerId, int LibraryId), string>();
        foreach (var selection in libraries)
        {
            var scope = (selection.MediaData.PlexServerId, selection.MediaData.PlexLibraryId);
            if (existing.TryGetValue((scope.PlexServerId, scope.PlexLibraryId,
                    selection.DownloadRootPath, selection.DestinationRootPath), out var captured))
            {
                names[scope] = captured;
                continue;
            }

            var selectedCollision = libraries.Any(x =>
                (x.MediaData.PlexServerId, x.MediaData.PlexLibraryId) != scope
                && x.DestinationRootPath == selection.DestinationRootPath
                && string.Equals(x.RootName, selection.RootName, StringComparison.OrdinalIgnoreCase));
            var existingCollision = existing.Any(x =>
                string.Equals(x.Key.DestinationRoot, selection.DestinationRootPath, StringComparison.OrdinalIgnoreCase)
                && x.Key.DownloadRoot == selection.DownloadRootPath
                && string.Equals(x.Value, selection.RootName, StringComparison.OrdinalIgnoreCase)
                && (x.Key.PlexServerId, x.Key.PlexLibraryId) != scope);
            names[scope] = selectedCollision || existingCollision
                ? $"{selection.RootName} [{scope.PlexServerId}-{scope.PlexLibraryId}]"
                : selection.RootName;
        }
        return names;
    }

    private static PlexPhotoFolder? FindRoot(PlexPhotoFolder folder, IReadOnlyDictionary<int, PlexPhotoFolder> folders)
    {
        var current = folder;
        while (current.ParentId is int parentId)
        {
            if (!folders.TryGetValue(parentId, out var parent))
                return null;
            current = parent;
        }
        return current;
    }

    private static bool TryGetDestinationPath(
        DownloadTaskDirectory directory,
        string fileName,
        out string path
    )
    {
        path = string.Empty;
        if (!Path.IsPathFullyQualified(directory.DestinationRootPath)
            || !IsValidDestinationComponent(fileName)
            || !directory.PhotoFolder.Split('/').All(IsValidDestinationComponent))
            return false;
        try
        {
            var root = Path.GetFullPath(directory.DestinationRootPath);
            var full = Path.GetFullPath(
                directory.GetDestinationFilePath(DownloadTaskType.PhotoImage, fileName));
            var prefix = Path.EndsInDirectorySeparator(root) ? root : root + Path.DirectorySeparatorChar;
            if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return false;
            path = full.Replace(Path.DirectorySeparatorChar, '/');
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static bool IsValidDestinationComponent(string component)
    {
        if (string.IsNullOrWhiteSpace(component) || component is "." or ".."
            || component.Length > 255 || component.EndsWith('.') || component.EndsWith(' '))
            return false;
        foreach (var character in component)
            if (character is '/' or '\\' or '\0' or '<' or '>' or ':' or '"' or '|' or '?' or '*'
                || char.IsControl(character))
                return false;
        var extension = component.IndexOf('.');
        var name = (extension >= 0 ? component[..extension] : component).ToUpperInvariant();
        return name is not ("CON" or "PRN" or "AUX" or "NUL")
            && !(name.Length == 4 && (name.StartsWith("COM") || name.StartsWith("LPT"))
                && name[3] is >= '1' and <= '9');
    }

    private void LogInvalidDestination(PhotoDownloadSelection selection) =>
        _log.Here().Warning("Skipping photo {Title} (ID: {Id}) because its destination path is invalid or collides",
            selection.MediaData.Title, selection.MediaData.Id);

    private static PhotoIdentity GetIdentity(PlexPhotoMediaData file) =>
        new(file.PlexServerId, file.PlexLibraryId, file.PlexApiRatingKey, file.PlexApiMediaId, file.PlexApiPartId);

    private static PhotoIdentity GetIdentity(DownloadTaskPhotoImageFile file) =>
        new(file.PlexServerId, file.PlexLibraryId, file.PlexApiRatingKey, file.PlexApiMediaId, file.PlexApiPartId);

    private readonly record struct SelectedPhoto(PlexPhotoMediaData MediaData, bool KeepCompleted);

    private readonly record struct PhotoIdentity(int ServerId, int LibraryId, int RatingKey, int MediaId, int PartId);
}

internal sealed record PhotoDownloadSelection(
    PlexPhotoMediaData MediaData,
    string RelativeDirectory,
    string FileName,
    bool KeepCompletedInDownloadFolder,
    string DownloadRootPath,
    string DestinationRootPath,
    string RootName
)
{
    public string PhotoFolder => string.IsNullOrEmpty(RelativeDirectory)
        ? RootName
        : $"{RootName}/{RelativeDirectory}";

    public DownloadTaskDirectory CreateDirectoryMetadata() => new()
    {
        DownloadRootPath = DownloadRootPath,
        DestinationRootPath = DestinationRootPath,
        MovieFolder = string.Empty,
        TvShowFolder = string.Empty,
        SeasonFolder = string.Empty,
        MusicArtistFolder = string.Empty,
        MusicAlbumFolder = string.Empty,
        PhotoFolder = PhotoFolder,
        OtherVideoFolder = string.Empty,
        KeepCompletedInDownloadFolder = KeepCompletedInDownloadFolder,
    };
}

internal readonly record struct PhotoDownloadCandidate(
    PhotoDownloadSelection Selection,
    DownloadTaskDirectory DirectoryMeta
)
{
    public PlexPhotoMediaData MediaData => Selection.MediaData;
    public string RelativeDirectory => Selection.RelativeDirectory;
    public string FileName => Selection.FileName;
    public string RootName => Selection.RootName;
    public string PhotoFolder => DirectoryMeta.PhotoFolder;
}
