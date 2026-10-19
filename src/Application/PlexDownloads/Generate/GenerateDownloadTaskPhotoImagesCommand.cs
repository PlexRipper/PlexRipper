namespace Reaparr.Application;

public record GenerateDownloadTaskPhotoImagesCommand : ICommand<Result<DownloadTaskCreationReport>>
{
    public GenerateDownloadTaskPhotoImagesCommand(CreateDownloadTasksRequest request) => Request = request;

    public GenerateDownloadTaskPhotoImagesCommand(List<DownloadMediaDTO> downloadMediaDtos)
        : this(new CreateDownloadTasksRequest(downloadMediaDtos)) { }

    public CreateDownloadTasksRequest Request { get; }
}

public class GenerateDownloadTaskPhotoImagesCommandValidator
    : AbstractValidator<GenerateDownloadTaskPhotoImagesCommand>
{
    public GenerateDownloadTaskPhotoImagesCommandValidator()
    {
        RuleFor(x => x.Request)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(x => x.Request.DownloadMedias).NotNull().NotEmpty();
                RuleForEach(x => x.Request.DownloadMedias)
                    .NotNull()
                    .SetValidator(new DownloadMediaDTOValidator());
                RuleForEach(x => x.Request.DownloadMedias)
                    .Must(x =>
                        x is null
                        || x.Type is not (PlexMediaType.PhotoAlbum or PlexMediaType.PhotoImage)
                        || x.Qualities is { Count: 0 }
                    )
                    .WithMessage("Photo selections must not include quality or part selectors.");
            });
    }
}

public class GenerateDownloadTaskPhotoImagesCommandHandler
    : ICommandHandler<GenerateDownloadTaskPhotoImagesCommand, Result<DownloadTaskCreationReport>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public GenerateDownloadTaskPhotoImagesCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GenerateDownloadTaskPhotoImagesCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result<DownloadTaskCreationReport>> ExecuteAsync(
        GenerateDownloadTaskPhotoImagesCommand command,
        CancellationToken cancellationToken
    )
    {
        var request = command.Request;
        var photoSelections = request
            .DownloadMedias
            .Where(x => x.Type is PlexMediaType.PhotoAlbum or PlexMediaType.PhotoImage)
            .ToList();
        if (photoSelections.Count == 0)
            return ResultExtensions.IsEmpty(nameof(photoSelections)).LogWarning();
        if (request.Integration is not null)
            return ResultExtensions.Create400BadRequestResult(
                "Photo downloads do not support Sonarr or Radarr integrations."
            ).LogError();

        var scopeKeys = photoSelections.Select(x => (x.PlexServerId, x.PlexLibraryId)).Distinct().ToList();
        var validLibraries = await _dbContext
            .PlexLibraries.Where(x => scopeKeys.Select(y => y.PlexLibraryId).Contains(x.Id) && x.Type == PlexMediaType.PhotoAlbum)
            .Select(x => new { x.Id, x.PlexServerId, x.Title, ServerName = x.PlexServer!.Name })
            .ToListAsync(cancellationToken);
        var validLibraryKeys = validLibraries.Select(x => (x.PlexServerId, x.Id)).ToHashSet();
        if (scopeKeys.Any(scope => !validLibraryKeys.Contains((scope.PlexServerId, scope.PlexLibraryId))))
            return ResultExtensions.Create400BadRequestResult(
                "The selected photo library does not match the requested server or media type."
            ).LogError();

        var libraryByScope = validLibraries.ToDictionary(x => (x.PlexServerId, x.Id));
        var selectedFiles = new List<SelectedPhoto>();
        foreach (var scopeSelections in photoSelections.GroupBy(x => (x.PlexServerId, x.PlexLibraryId)))
        {
            var (serverId, libraryId) = scopeSelections.Key;
            var albumIds = scopeSelections.Where(x => x.Type == PlexMediaType.PhotoAlbum)
                .SelectMany(x => x.MediaIds).Distinct().ToList();
            var albums = await _dbContext.PlexPhotoAlbums
                .Where(x => albumIds.Contains(x.Id) && x.PlexServerId == serverId && x.PlexLibraryId == libraryId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
            if (albums.Count != albumIds.Count)
                return ResultExtensions.Create400BadRequestResult(
                    "Every selected photo album must belong to the requested library and server."
                ).LogError();

            var memberships = await _dbContext.PlexPhotoAlbumMemberships
                .Include(x => x.PlexPhotoMediaData)
                .Where(x => albumIds.Contains(x.PlexPhotoAlbumId)
                    && x.PlexPhotoMediaData!.PlexServerId == serverId
                    && x.PlexPhotoMediaData.PlexLibraryId == libraryId)
                .ToListAsync(cancellationToken);
            var filesByAlbum = memberships.ToLookup(x => x.PlexPhotoAlbumId, x => x.PlexPhotoMediaData!);
            var directIds = scopeSelections.Where(x => x.Type == PlexMediaType.PhotoImage)
                .SelectMany(x => x.MediaIds).Distinct().ToList();
            var directFiles = await _dbContext.PlexPhotoMediaData
                .Where(x => directIds.Contains(x.Id) && x.PlexServerId == serverId && x.PlexLibraryId == libraryId)
                .ToDictionaryAsync(x => x.Id, cancellationToken);
            var choices = new List<(PlexPhotoMediaData File, bool KeepCompleted, bool Direct)>();
            foreach (var selection in scopeSelections)
            {
                foreach (var id in selection.MediaIds.Distinct())
                {
                    if (selection.Type == PlexMediaType.PhotoAlbum)
                        choices.AddRange(filesByAlbum[id].Select(file =>
                            (file, selection.KeepCompletedInDownloadFolder, false)));
                    else if (directFiles.TryGetValue(id, out var file))
                        choices.Add((file, selection.KeepCompletedInDownloadFolder, true));
                }
            }

            foreach (var fileChoices in choices.GroupBy(x => x.File.Id))
            {
                // Direct selection overrides album options; peers must agree, regardless of request order.
                var effectiveChoices = fileChoices.Any(x => x.Direct)
                    ? fileChoices.Where(x => x.Direct)
                    : fileChoices;
                var options = effectiveChoices.Select(x => x.KeepCompleted).Distinct().ToList();
                if (options.Count > 1)
                    return ResultExtensions.Create400BadRequestResult(
                        "Conflicting photo KeepCompletedInDownloadFolder values were supplied."
                    ).LogError();
                selectedFiles.Add(new(fileChoices.First().File, options[0]));
            }
        }

        if (selectedFiles.Count == 0)
            return Result.Ok(new DownloadTaskCreationReport());

        var scopes = selectedFiles.Select(x => (ServerId: x.MediaData.PlexServerId, LibraryId: x.MediaData.PlexLibraryId))
            .Distinct().ToList();

        var serverIds = scopes.Select(x => x.ServerId).Distinct().ToList();
        var libraryIds = scopes.Select(x => x.LibraryId).Distinct().ToList();
        var scopeSet = scopes.ToHashSet();
        var allFolders = await _dbContext
            .PlexPhotoFolders.Where(x => serverIds.Contains(x.PlexServerId) && libraryIds.Contains(x.PlexLibraryId))
            .ToListAsync(cancellationToken);
        allFolders = allFolders.Where(x => scopeSet.Contains((x.PlexServerId, x.PlexLibraryId))).ToList();
        var folderById = allFolders.ToDictionary(x => x.Id);
        var activeRootsByScope = allFolders
            .Where(x => x.ParentId is null && x.IsActive == true)
            .GroupBy(x => (x.PlexServerId, x.PlexLibraryId))
            .ToDictionary(x => x.Key, x => x.ToList());

        var destinationResult = await ResolveDestinationRootsAsync(scopes, request, cancellationToken);
        if (destinationResult.IsFailed)
            return destinationResult.ToResult().LogIfFailed();
        var destinationRoots = destinationResult.Value;
        var downloadRootPath = CanonicalizePhotoRootPath((await _dbContext.GetDownloadFolder(null)).DirectoryPath);

        var prepared = new List<PreparedPhoto>();
        foreach (var selected in selectedFiles.OrderBy(x => x.MediaData.Id))
        {
            var mediaData = selected.MediaData;
            var scope = (mediaData.PlexServerId, mediaData.PlexLibraryId);
            if (
                mediaData.PlexPhotoFolderId is not int folderId
                || !folderById.TryGetValue(folderId, out var folder)
                || !activeRootsByScope.TryGetValue(scope, out var roots)
            )
            {
                _log.Here().Warning(
                    "Skipping photo {Title} (ID: {Id}) because it has no active catalog folder",
                    mediaData.Title,
                    mediaData.Id
                );
                continue;
            }

            var root = FindRoot(folder, folderById);
            if (root is null || !roots.Any(x => x.Id == root.Id))
            {
                _log.Here().Warning(
                    "Skipping photo {Title} (ID: {Id}) because its catalog folder root is inactive",
                    mediaData.Title,
                    mediaData.Id
                );
                continue;
            }
            var sourceRelativePath = folder.RelativePath.Trim('/');
            var fileName = mediaData.OriginalFilename.GetFileName();
            string relativeDirectory;
            if (roots.Count <= 1)
                relativeDirectory = sourceRelativePath;
            else if (string.IsNullOrEmpty(sourceRelativePath))
                relativeDirectory = $"root-{root.Id}";
            else
                relativeDirectory = $"root-{root.Id}/{sourceRelativePath}";
            if (!libraryByScope.TryGetValue(scope, out var library))
                continue;
            prepared.Add(
                new PreparedPhoto(
                    mediaData,
                    relativeDirectory,
                    fileName,
                    selected.KeepCompletedInDownloadFolder,
                    downloadRootPath,
                    destinationRoots[scope],
                    library.ServerName.SanitizeFolderName(),
                    library.Title.SanitizeFolderName()
                )
            );
        }

        if (prepared.Count == 0)
            return Result.Ok(new DownloadTaskCreationReport());

        var destinationPaths = destinationRoots.Values.Distinct().ToList();
        var transactionResult = await _dbContext.ExecuteTransactionAsync(
            async (ctx, txCt) =>
            {
                var existingFolders = await ctx
                    .DownloadTaskPhotoFolders.AsTracking()
                    .Where(x => serverIds.Contains(x.PlexServerId) && libraryIds.Contains(x.PlexLibraryId))
                    .ToListAsync(txCt);
                existingFolders = existingFolders
                    .Where(x => scopeSet.Contains((x.PlexServerId, x.PlexLibraryId)))
                    .ToList();
                var rootNameFolders = await ctx
                    .DownloadTaskPhotoFolders.AsTracking()
                    .Where(x => x.ParentId == null)
                    .ToListAsync(txCt);
                rootNameFolders = rootNameFolders
                    .Where(x =>
                        destinationPaths.Contains(CanonicalizePhotoRootPath(x.DestinationRootPath))
                        && string.Equals(
                            CanonicalizePhotoRootPath(x.DownloadRootPath),
                            downloadRootPath,
                            StringComparison.Ordinal
                        )
                    )
                    .ToList();
                existingFolders = existingFolders
                    .Concat(rootNameFolders)
                    .DistinctBy(x => x.Id)
                    .OrderBy(x => x.CreatedAt)
                    .ToList();
                var existingFiles = await ctx
                    .DownloadTaskPhotoImageFiles.AsTracking()
                    .ToListAsync(txCt);

                var rootNamesByScope = ResolveRootNames(prepared, existingFolders);
                var existingIdentities = existingFiles.Select(x => (
                    Identity: ToIdentity(x),
                    DownloadRoot: CanonicalizePhotoRootPath(x.DirectoryMeta.DownloadRootPath),
                    DestinationRoot: CanonicalizePhotoRootPath(x.DirectoryMeta.DestinationRootPath)
                )).ToHashSet();
                var existingPathOwners = new Dictionary<string, HashSet<PhotoIdentity>>(StringComparer.OrdinalIgnoreCase);
                foreach (var file in existingFiles.Where(x => x.DownloadStatus != DownloadStatus.Deleted))
                {
                    if (
                        TryGetCollisionKey(
                            file.DirectoryMeta.DestinationRootPath,
                            file.DirectoryMeta.PhotoFolder,
                            file.FileName,
                            out var pathKey
                        )
                    )
                    {
                        if (!existingPathOwners.TryGetValue(pathKey, out var owners))
                        {
                            owners = [];
                            existingPathOwners[pathKey] = owners;
                        }
                        owners.Add(ToIdentity(file));
                    }
                }

                var candidates = new List<PreparedCandidate>();
                foreach (var photo in prepared)
                {
                    var media = photo.MediaData;
                    var identity = new PhotoIdentity(media.PlexServerId, media.PlexLibraryId,
                        media.PlexApiRatingKey, media.PlexApiMediaId, media.PlexApiPartId);
                    if (existingIdentities.Contains((identity, photo.DownloadRootPath, photo.DestinationRoot)))
                        continue;
                    var rootName = rootNamesByScope[(media.PlexServerId, media.PlexLibraryId, photo.DownloadRootPath)];
                    var photoFolder = string.IsNullOrEmpty(photo.RelativeDirectory)
                        ? rootName
                        : $"{rootName}/{photo.RelativeDirectory}";
                    var valid = TryGetCollisionKey(photo.DestinationRoot, photoFolder, photo.FileName, out var pathKey);
                    candidates.Add(new(photo, rootName, photoFolder, identity, pathKey, valid));
                }

                var accepted = new List<PreparedCandidate>();
                foreach (var pathGroup in candidates.GroupBy(x => x.PathKey, StringComparer.OrdinalIgnoreCase))
                {
                    // Reject every new participant in a collision, not just whichever was processed last.
                    var collides = pathGroup.Select(x => x.Identity).Distinct().Skip(1).Any();
                    foreach (var candidate in pathGroup)
                    {
                        if (candidate.PathIsValid && !collides
                            && !(existingPathOwners.TryGetValue(candidate.PathKey, out var owners)
                                && owners.Any(owner => owner != candidate.Identity)))
                        {
                            accepted.Add(candidate);
                            continue;
                        }
                        _log.Here().Warning(
                            "Skipping photo {Title} (ID: {Id}) because its destination path is invalid or collides",
                            candidate.Photo.MediaData.Title,
                            candidate.Photo.MediaData.Id
                        );
                    }
                }
                if (accepted.Count == 0)
                    return new DownloadTaskCreationReport();

                var existingFolderByKey = existingFolders
                    .GroupBy(x =>
                        new FolderKey(
                            x.PlexServerId,
                            x.PlexLibraryId,
                            x.ParentId,
                            x.RelativePath,
                            CanonicalizePhotoRootPath(x.DownloadRootPath),
                            CanonicalizePhotoRootPath(x.DestinationRootPath)
                        )
                    )
                    .ToDictionary(x => x.Key, x => x.OrderBy(y => y.CreatedAt).First());
                var createdFolders = 0;
                var filesToCreate = new List<DownloadTaskPhotoImageFile>();
                DownloadTaskPhotoFolder GetOrCreateFolder(
                    PreparedPhoto photo, DownloadTaskPhotoFolder? parent, string title, string relativePath)
                {
                    var key = new FolderKey(photo.MediaData.PlexServerId, photo.MediaData.PlexLibraryId,
                        parent?.Id, relativePath, downloadRootPath, photo.DestinationRoot);
                    if (existingFolderByKey.TryGetValue(key, out var existing))
                        return existing;
                    var folder = new DownloadTaskPhotoFolder
                    {
                        Id = Guid.NewGuid(),
                        PlexApiRatingKey = 0,
                        PlexServerId = key.ServerId,
                        PlexLibraryId = key.LibraryId,
                        Title = title,
                        FullTitle = parent is null ? title : $"{parent.FullTitle}/{title}",
                        Year = 0,
                        DownloadStatus = DownloadStatus.Queued,
                        CreatedAt = DateTime.UtcNow,
                        Parent = parent,
                        ParentId = parent?.Id,
                        RelativePath = relativePath,
                        DownloadRootPath = downloadRootPath,
                        DestinationRootPath = photo.DestinationRoot,
                        DataReceived = 0,
                        FileDataTransferred = 0,
                        DataTotal = 0,
                        DownloadSpeed = 0,
                        FileTransferSpeed = 0,
                        Children = [],
                        Files = [],
                    };
                    parent?.Children.Add(folder);
                    ctx.DownloadTaskPhotoFolders.Add(folder);
                    existingFolders.Add(folder);
                    existingFolderByKey.Add(key, folder);
                    createdFolders++;
                    return folder;
                }

                foreach (var candidate in accepted)
                {
                    var photo = candidate.Photo;
                    var parent = GetOrCreateFolder(photo, null, candidate.RootName, string.Empty);
                    var relativeSegments = candidate.PhotoFolder
                        .Split('/', StringSplitOptions.RemoveEmptyEntries)
                        .Skip(1);
                    var cumulative = string.Empty;
                    foreach (var segment in relativeSegments)
                    {
                        cumulative = string.IsNullOrEmpty(cumulative) ? segment : $"{cumulative}/{segment}";
                        parent = GetOrCreateFolder(photo, parent, segment, cumulative);
                    }

                    var file = photo.MediaData.MapToDownloadTask(
                        parent,
                        request,
                        downloadRootPath,
                        photo.DestinationRoot,
                        candidate.PhotoFolder,
                        photo.KeepCompletedInDownloadFolder
                    );
                    parent.Files.Add(file);
                    UpdatePhotoFolderAggregates(parent, file.DataTotal, existingFolders);
                    ctx.DownloadTaskPhotoImageFiles.Add(file);
                    filesToCreate.Add(file);
                }

                await ctx.SaveChangesAsync(txCt);
                foreach (var file in filesToCreate)
                {
                    ctx.DownloadTaskPhotoImageFileLogs.Add(
                        new DownloadTaskPhotoImageFileLog
                        {
                            Status = DownloadStatus.Queued,
                            LogLevel = NotificationLevel.Information,
                            Message = $"DownloadTask {file.FileName} was queued for downloading",
                            DownloadTaskFileId = file.Id,
                            CreatedAt = DateTime.UtcNow,
                        }
                    );
                }
                await ctx.SaveChangesAsync(txCt);

                return new DownloadTaskCreationReport
                {
                    PhotoFolders = createdFolders,
                    PhotoImages = filesToCreate.Count,
                };
            },
            cancellationToken
        );
        return transactionResult.IsFailed ? transactionResult.LogIfFailed() : transactionResult;
    }

    private async Task<Result<Dictionary<(int ServerId, int LibraryId), string>>> ResolveDestinationRootsAsync(
        IReadOnlyCollection<(int ServerId, int LibraryId)> scopes,
        CreateDownloadTasksRequest request,
        CancellationToken cancellationToken
    )
    {
        if (!string.IsNullOrWhiteSpace(request.CustomDestinationFolderPath))
            return Result.Ok(
                scopes.ToDictionary(x => x, _ => CanonicalizePhotoRootPath(request.CustomDestinationFolderPath))
            );

        string? explicitDestination = null;
        if (request.DestinationFolderPathId is int destinationId)
            explicitDestination = await _dbContext.FolderPaths
                .Where(x => x.Id == destinationId)
                .Select(x => x.DirectoryPath)
                .FirstOrDefaultAsync(cancellationToken);
        if (request.DestinationFolderPathId is not null && explicitDestination is null)
            return ResultExtensions.EntityNotFound(nameof(FolderPath), request.DestinationFolderPathId.Value);

        var result = new Dictionary<(int ServerId, int LibraryId), string>();
        foreach (var scope in scopes)
        {
            var destination = explicitDestination ?? (await _dbContext.GetDestinationFolder(scope.LibraryId))?.DirectoryPath;
            if (string.IsNullOrWhiteSpace(destination))
                return ResultExtensions.EntityNotFound(nameof(FolderPath), scope.LibraryId);
            result[scope] = CanonicalizePhotoRootPath(destination);
        }
        return Result.Ok(result);
    }

    private static Dictionary<(int ServerId, int LibraryId, string DownloadRootPath), string> ResolveRootNames(
        IEnumerable<PreparedPhoto> photos,
        IEnumerable<DownloadTaskPhotoFolder> existingFolders
    )
    {
        var existingRoots = existingFolders
            .Where(x => x.ParentId is null)
            .GroupBy(x => (
                x.PlexServerId,
                x.PlexLibraryId,
                DownloadRootPath: CanonicalizePhotoRootPath(x.DownloadRootPath),
                DestinationRootPath: CanonicalizePhotoRootPath(x.DestinationRootPath)
            ))
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.CreatedAt).First().Title);
        var requested = photos
            .DistinctBy(x => (x.MediaData.PlexServerId, x.MediaData.PlexLibraryId, x.DownloadRootPath, x.DestinationRoot))
            .ToList();
        var result = new Dictionary<(int ServerId, int LibraryId, string DownloadRootPath), string>();
        foreach (var photo in requested)
        {
            var scope = (
                ServerId: photo.MediaData.PlexServerId,
                LibraryId: photo.MediaData.PlexLibraryId,
                DownloadRootPath: photo.DownloadRootPath
            );
            if (
                existingRoots.TryGetValue(
                    (scope.ServerId, scope.LibraryId, scope.DownloadRootPath, photo.DestinationRoot),
                    out var existing
                )
            )
            {
                result[scope] = existing;
                continue;
            }

            var baseName = $"{photo.ServerName} - {photo.LibraryName}".SanitizeFolderName();
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = $"server-{scope.ServerId}-library-{scope.LibraryId}";
            var sameContext = requested
                .Where(x =>
                    x.DestinationRoot == photo.DestinationRoot && x.DownloadRootPath == photo.DownloadRootPath
                )
                .ToList();
            var collidesWithExisting = existingRoots.Any(x =>
                string.Equals(x.Key.DestinationRootPath, photo.DestinationRoot, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.Key.DownloadRootPath, photo.DownloadRootPath, StringComparison.Ordinal)
                && string.Equals(x.Value, baseName, StringComparison.OrdinalIgnoreCase)
                && (x.Key.PlexServerId != scope.ServerId || x.Key.PlexLibraryId != scope.LibraryId)
            );
            var collidesWithSelection = sameContext.Count(x =>
                string.Equals(
                    $"{x.ServerName} - {x.LibraryName}".SanitizeFolderName(),
                    baseName,
                    StringComparison.OrdinalIgnoreCase
                )
            ) > 1;
            result[scope] = collidesWithExisting || collidesWithSelection
                ? $"{baseName} [{scope.ServerId}-{scope.LibraryId}]"
                : baseName;
        }
        return result;
    }

    private static void UpdatePhotoFolderAggregates(
        DownloadTaskPhotoFolder folder,
        long dataTotal,
        IEnumerable<DownloadTaskPhotoFolder> knownFolders
    )
    {
        folder.DataTotal += dataTotal;
        folder.DownloadStatus = DownloadStatus.Queued;
        var current = folder;
        var visited = new HashSet<Guid>();
        while (current.ParentId is Guid parentId && visited.Add(parentId))
        {
            var parent = current.Parent;
            if (parent is null || parent.Id != parentId)
                parent = knownFolders.FirstOrDefault(x => x.Id == parentId);
            if (parent is null)
                break;
            parent.DataTotal += dataTotal;
            parent.DownloadStatus = DownloadStatus.Queued;
            current = parent;
        }
    }
    private static PlexPhotoFolder? FindRoot(
        PlexPhotoFolder folder,
        IReadOnlyDictionary<int, PlexPhotoFolder> folderById
    )
    {
        var current = folder;
        var visited = new HashSet<int>();
        while (current.ParentId is int parentId)
        {
            if (!visited.Add(current.Id) || !folderById.TryGetValue(parentId, out current!))
                return null;
        }
        return current;
    }

    private static string CanonicalizePhotoRootPath(string path)
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

    private static bool TryGetCollisionKey(
        string destinationRoot,
        string relativeDirectory,
        string fileName,
        out string key
    )
    {
        key = string.Empty;
        if (
            string.IsNullOrWhiteSpace(destinationRoot)
            || !Path.IsPathFullyQualified(destinationRoot)
            || !TryValidateDestinationComponents(relativeDirectory, fileName)
        )
            return false;
        try
        {
            var root = Path.GetFullPath(destinationRoot);
            var full = Path.GetFullPath(Path.Combine(root, relativeDirectory.Replace('/', Path.DirectorySeparatorChar), fileName));
            var rootWithSeparator = root.EndsWith(Path.DirectorySeparatorChar) ? root : root + Path.DirectorySeparatorChar;
            if (
                !full.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(full, root, StringComparison.OrdinalIgnoreCase)
            )
                return false;
            key = full.Replace(Path.DirectorySeparatorChar, '/').Replace(Path.AltDirectorySeparatorChar, '/');
            return true;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return false;
        }
    }

    private static bool TryValidateDestinationComponents(string relativeDirectory, string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Contains('/') || fileName.Contains('\\'))
            return false;
        if (!IsValidDestinationComponent(fileName))
            return false;
        return string.IsNullOrEmpty(relativeDirectory)
            || relativeDirectory.Split('/', StringSplitOptions.None).All(IsValidDestinationComponent);
    }

    private static bool IsValidDestinationComponent(string component)
    {
        if (string.IsNullOrWhiteSpace(component) || component is "." or ".." || component.Length > 255)
            return false;
        if (component.EndsWith('.') || component.EndsWith(' '))
            return false;
        foreach (var character in component)
            if (character is '/' or '\\' or '\0' or '<' or '>' or ':' or '"' or '|' or '?' or '*' || char.IsControl(character))
                return false;
        var extensionIndex = component.IndexOf('.');
        var name = (extensionIndex >= 0 ? component[..extensionIndex] : component).ToUpperInvariant();
        return name is not ("CON" or "PRN" or "AUX" or "NUL")
            && !(name.Length == 4 && (name.StartsWith("COM") || name.StartsWith("LPT")) && name[3] is >= '1' and <= '9');
    }

    private static PhotoIdentity ToIdentity(DownloadTaskPhotoImageFile file) =>
        new(file.PlexServerId, file.PlexLibraryId, file.PlexApiRatingKey, file.PlexApiMediaId, file.PlexApiPartId);

    private sealed record SelectedPhoto(PlexPhotoMediaData MediaData, bool KeepCompletedInDownloadFolder);

    private sealed record PreparedPhoto(
        PlexPhotoMediaData MediaData,
        string RelativeDirectory,
        string FileName,
        bool KeepCompletedInDownloadFolder,
        string DownloadRootPath,
        string DestinationRoot,
        string ServerName,
        string LibraryName
    );

    private sealed record PreparedCandidate(
        PreparedPhoto Photo,
        string RootName,
        string PhotoFolder,
        PhotoIdentity Identity,
        string PathKey,
        bool PathIsValid
    );

    private readonly record struct FolderKey(
        int ServerId,
        int LibraryId,
        Guid? ParentId,
        string RelativePath,
        string DownloadRootPath,
        string DestinationRootPath
    );

    private readonly record struct PhotoIdentity(
        int ServerId,
        int LibraryId,
        int RatingKey,
        int MediaId,
        int PartId
    );
}