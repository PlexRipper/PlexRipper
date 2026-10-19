namespace Reaparr.Application;

public record GetMusicDownloadPreviewCommand(List<DownloadMediaDTO> DownloadMedias)
    : ICommand<Result<IEnumerable<DownloadPreview>>>;

public class GetMusicDownloadPreviewCommandValidator : AbstractValidator<GetMusicDownloadPreviewCommand>
{
    public GetMusicDownloadPreviewCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.DownloadMedias).NotNull();
        RuleForEach(x => x.DownloadMedias).NotNull().SetValidator(new DownloadMediaDTOValidator());
        RuleForEach(x => x.DownloadMedias)
            .Must(x =>
                x is null || x.Type is PlexMediaType.MusicArtist or PlexMediaType.MusicAlbum or PlexMediaType.MusicTrack
            )
            .WithMessage("Music preview selections must contain only music media.");
    }
}

public class GetMusicDownloadPreviewCommandHandler
    : ICommandHandler<GetMusicDownloadPreviewCommand, Result<IEnumerable<DownloadPreview>>>
{
    private readonly IReaparrDbContext _dbContext;

    public GetMusicDownloadPreviewCommandHandler(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Result<IEnumerable<DownloadPreview>>> ExecuteAsync(
        GetMusicDownloadPreviewCommand command,
        CancellationToken cancellationToken
    ) => CreateMusicPreviews(command.DownloadMedias, cancellationToken);

    private async Task<Result<IEnumerable<DownloadPreview>>> CreateMusicPreviews(
        List<DownloadMediaDTO> downloadMedias,
        CancellationToken cancellationToken
    )
    {
        var selections = downloadMedias
            .MergeAndGroupList()
            .OrderBy(x =>
                x.Type == PlexMediaType.MusicArtist ? 0
                : x.Type == PlexMediaType.MusicAlbum ? 1
                : 2
            );
        var tracksById = new Dictionary<int, PlexMusicTrack>();
        var sourceSelections = new Dictionary<int, DownloadMediaDTO>();
        foreach (var selection in selections)
        {
            var library = await _dbContext.PlexLibraries.GetAsync(selection.PlexLibraryId, cancellationToken);
            if (
                library is null
                || library.Type != PlexMediaType.MusicArtist
                || library.PlexServerId != selection.PlexServerId
            )
                return ResultExtensions.Create400BadRequestResult(
                    "The selected library does not match the requested server or media type."
                );

            var ids = selection.MediaIds.Distinct().ToList();
            int selectedCount;
            List<PlexMusicTrack> tracks;
            switch (selection.Type)
            {
                case PlexMediaType.MusicArtist:
                    var artists = await _dbContext
                        .PlexMusicArtists.Where(x =>
                            ids.Contains(x.Id)
                            && x.PlexLibraryId == library.Id
                            && x.PlexServerId == library.PlexServerId
                        )
                        .Include(x => x.Albums)
                            .ThenInclude(x => x.Tracks)
                                .ThenInclude(x => x.MediaDataList)
                        .ToListAsync(cancellationToken);
                    selectedCount = artists.Count;
                    tracks = artists.SelectMany(x => x.Albums).SelectMany(x => x.Tracks).ToList();
                    break;
                case PlexMediaType.MusicAlbum:
                    var albums = await _dbContext
                        .PlexMusicAlbums.Where(x =>
                            ids.Contains(x.Id)
                            && x.PlexLibraryId == library.Id
                            && x.PlexServerId == library.PlexServerId
                        )
                        .Include(x => x.PlexArtist)
                        .Include(x => x.Tracks)
                            .ThenInclude(x => x.MediaDataList)
                        .ToListAsync(cancellationToken);
                    selectedCount = albums.Count;
                    tracks = albums.SelectMany(x => x.Tracks).ToList();
                    break;
                default:
                    tracks = await _dbContext
                        .PlexMusicTracks.Where(x =>
                            ids.Contains(x.Id)
                            && x.PlexLibraryId == library.Id
                            && x.PlexServerId == library.PlexServerId
                        )
                        .Include(x => x.PlexAlbum)
                            .ThenInclude(x => x!.PlexArtist)
                        .Include(x => x.MediaDataList)
                        .ToListAsync(cancellationToken);
                    selectedCount = tracks.Count;
                    break;
            }

            if (
                selectedCount != ids.Count
                || tracks.Any(x => x.PlexLibraryId != library.Id || x.PlexServerId != library.PlexServerId)
            )
                return ResultExtensions.Create400BadRequestResult(
                    "Every selected media item must belong to the requested library and server."
                );

            foreach (var track in tracks)
            {
                tracksById[track.Id] = track;
                sourceSelections[track.Id] =
                    sourceSelections.TryGetValue(track.Id, out var previous)
                    && selection.Qualities.All(x => x.MediaId != track.Id)
                    && previous.Qualities.Any(x => x.MediaId == track.Id)
                        ? selection with
                        {
                            Qualities = previous.Qualities.Where(x => x.MediaId == track.Id).ToList(),
                        }
                        : selection;
            }
        }

        var previews = new List<DownloadPreview>();
        var artistPreviews = new Dictionary<int, DownloadPreview>();
        var albumPreviews = new Dictionary<int, DownloadPreview>();
        foreach (var track in tracksById.Values)
        {
            var data = track.MediaDataList;
            var groups = data.GroupBy(x => x.PlexApiMediaId).ToList();
            var selector = sourceSelections[track.Id].Qualities.FirstOrDefault(x => x.MediaId == track.Id);
            var selectedData = data.FirstOrDefault(x => x.Id == selector?.DataId);
            if (selectedData is not null)
                groups.RemoveAll(x => x.Key != selectedData.PlexApiMediaId);

            var album = track.PlexAlbum;
            var artist = album?.PlexArtist;
            if (
                album is null
                || artist is null
                || album.PlexLibraryId != track.PlexLibraryId
                || album.PlexServerId != track.PlexServerId
                || artist.PlexLibraryId != track.PlexLibraryId
                || artist.PlexServerId != track.PlexServerId
            )
                return ResultExtensions.Create400BadRequestResult("The selected track hierarchy is invalid.");

            var preview = track.ProjectToDownloadPreviewMapper(groups, selectedData?.Id);

            if (!artistPreviews.TryGetValue(artist.Id, out var artistPreview))
            {
                artistPreview = artist.ProjectToDownloadPreviewMapper();
                artistPreviews.Add(artist.Id, artistPreview);
                previews.Add(artistPreview);
            }
            if (!albumPreviews.TryGetValue(album.Id, out var albumPreview))
            {
                albumPreview = album.ProjectToDownloadPreviewMapper();
                albumPreviews.Add(album.Id, albumPreview);
                artistPreview.Children.Add(albumPreview);
                artistPreview.ChildCount++;
            }
            albumPreview.Children.Add(preview);
            albumPreview.ChildCount++;
            albumPreview.Size += preview.Size;
            artistPreview.Size += preview.Size;
        }

        foreach (var parent in albumPreviews.Values.Concat(artistPreviews.Values))
        {
            var children = parent.Children.OrderByNatural(x => x.Title).ToList();
            parent.Children.Clear();
            parent.Children.AddRange(children);
        }

        return Result.Ok<IEnumerable<DownloadPreview>>(previews);
    }
}
