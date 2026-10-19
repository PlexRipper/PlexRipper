namespace Reaparr.Application;

public record GetPhotoDownloadPreviewCommand(List<DownloadMediaDTO> DownloadMedias)
    : ICommand<Result<IEnumerable<DownloadPreview>>>;

public class GetPhotoDownloadPreviewCommandValidator : AbstractValidator<GetPhotoDownloadPreviewCommand>
{
    public GetPhotoDownloadPreviewCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.DownloadMedias).NotNull();
        RuleForEach(x => x.DownloadMedias)
            .NotNull()
            .SetValidator(new DownloadMediaDTOValidator());
        RuleForEach(x => x.DownloadMedias)
            .Must(x => x is null || x.Type is PlexMediaType.PhotoAlbum or PlexMediaType.PhotoImage)
            .WithMessage("Photo preview selections must contain only photo media.");
        RuleForEach(x => x.DownloadMedias)
            .Must(x => x is null || x.Qualities is { Count: 0 })
            .WithMessage("Photo selections must not include quality or part selectors.");
    }
}

public class GetPhotoDownloadPreviewCommandHandler
    : ICommandHandler<GetPhotoDownloadPreviewCommand, Result<IEnumerable<DownloadPreview>>>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public GetPhotoDownloadPreviewCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GetPhotoDownloadPreviewCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<DownloadPreview>>> ExecuteAsync(
        GetPhotoDownloadPreviewCommand command,
        CancellationToken cancellationToken
    )
    {
        if (command.DownloadMedias.Count == 0)
            return Result.Ok(Enumerable.Empty<DownloadPreview>());
        var preparation = await PhotoDownloadPlanning.PrepareAsync(
            _dbContext, new CreateDownloadTasksRequest(command.DownloadMedias), _log, cancellationToken);
        if (preparation.IsCancelledOrFailed)
            return preparation.ToResult<IEnumerable<DownloadPreview>>().LogIfFailed();
        if (preparation.Value.Count == 0)
            return Result.Ok(Enumerable.Empty<DownloadPreview>());

        var roots = await _dbContext.DownloadTaskPhotoFolders
            .Where(x => x.ParentId == null).ToListAsync(cancellationToken);
        var files = await _dbContext.DownloadTaskPhotoImageFiles.ToListAsync(cancellationToken);
        var accepted = PhotoDownloadPlanning.Accept(preparation.Value, roots, files, _log);
        return Result.Ok(CreatePreviews(accepted));
    }

    private static IEnumerable<DownloadPreview> CreatePreviews(IEnumerable<PhotoDownloadFile> files)
    {
        var roots = new Dictionary<(int ServerId, int LibraryId), DownloadPreview>();
        var folders = new Dictionary<(int ServerId, int LibraryId, string Path), DownloadPreview>();
        foreach (var file in files)
        {
            var media = file.Photo.MediaData;
            var scope = (media.PlexServerId, media.PlexLibraryId);
            if (!roots.TryGetValue(scope, out var root))
            {
                root = CreatePreview(media.PlexLibraryId, file.RootName, PlexMediaType.PhotoAlbum, 0);
                roots[scope] = root;
            }
            var parent = root;
            var path = string.Empty;
            foreach (var segment in file.Photo.RelativeDirectory.Split('/', StringSplitOptions.RemoveEmptyEntries))
            {
                path = string.IsNullOrEmpty(path) ? segment : $"{path}/{segment}";
                var key = (scope.PlexServerId, scope.PlexLibraryId, path);
                if (!folders.TryGetValue(key, out var child))
                {
                    child = CreatePreview(media.PlexPhotoFolderId!.Value, segment, PlexMediaType.PhotoAlbum, 0);
                    folders[key] = child;
                    parent.Children.Add(child);
                }
                parent = child;
            }
            parent.Children.Add(CreatePreview(media.Id, file.Photo.FileName, PlexMediaType.PhotoImage, media.Size));
        }
        foreach (var root in roots.Values)
            AggregateAndSort(root);
        return roots.Values.OrderByNatural(x => x.Title);
    }

    private static DownloadPreview CreatePreview(int id, string title, PlexMediaType type, long size) =>
        new()
        {
            Id = id,
            Title = title,
            MediaType = type,
            Size = size,
            ChildCount = 0,
            TvShowId = default,
            SeasonId = default,
            ArtistId = default,
            AlbumId = default,
            Qualities = [],
            Children = [],
        };

    private static void AggregateAndSort(DownloadPreview folder)
    {
        foreach (var child in folder.Children)
        {
            if (child.MediaType == PlexMediaType.PhotoAlbum)
                AggregateAndSort(child);
            folder.Size += child.Size;
            folder.ChildCount += child.MediaType == PlexMediaType.PhotoImage ? 1 : child.ChildCount;
        }
        var sorted = folder.Children.OrderByNatural(x => x.Title).ToList();
        folder.Children.Clear();
        folder.Children.AddRange(sorted);
    }
}
