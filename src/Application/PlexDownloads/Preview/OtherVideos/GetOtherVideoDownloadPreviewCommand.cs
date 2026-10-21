namespace Reaparr.Application;

public record GetOtherVideoDownloadPreviewCommand(List<DownloadMediaDTO> DownloadMedias)
    : ICommand<Result<IEnumerable<DownloadPreview>>>;

public class GetOtherVideoDownloadPreviewCommandValidator : AbstractValidator<GetOtherVideoDownloadPreviewCommand>
{
    public GetOtherVideoDownloadPreviewCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.DownloadMedias).NotNull();
        RuleForEach(x => x.DownloadMedias).NotNull().SetValidator(new DownloadMediaDTOValidator());
        RuleForEach(x => x.DownloadMedias)
            .Must(x => x is null || x.Type == PlexMediaType.OtherVideos)
            .WithMessage("Other video preview selections must contain only other video media.");
    }
}

public class GetOtherVideoDownloadPreviewCommandHandler
    : ICommandHandler<GetOtherVideoDownloadPreviewCommand, Result<IEnumerable<DownloadPreview>>>
{
    private readonly IReaparrDbContext _dbContext;

    public GetOtherVideoDownloadPreviewCommandHandler(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Result<IEnumerable<DownloadPreview>>> ExecuteAsync(
        GetOtherVideoDownloadPreviewCommand command,
        CancellationToken cancellationToken
    ) => CreateOtherVideoPreviews(command.DownloadMedias, cancellationToken);

    private async Task<Result<IEnumerable<DownloadPreview>>> CreateOtherVideoPreviews(
        List<DownloadMediaDTO> downloadMedias,
        CancellationToken cancellationToken
    )
    {
        var selections = downloadMedias.MergeAndGroupList();
        var videosById = new Dictionary<int, PlexOtherVideo>();
        var sourceSelections = new Dictionary<int, DownloadMediaDTO>();
        foreach (var selection in selections)
        {
            var library = await _dbContext.PlexLibraries.GetAsync(selection.PlexLibraryId, cancellationToken);
            if (
                library is null
                || library.Type != PlexMediaType.OtherVideos
                || library.PlexServerId != selection.PlexServerId
            )
                return ResultExtensions.Create400BadRequestResult(
                    "The selected library does not match the requested server or media type."
                );

            var ids = selection.MediaIds.Distinct().ToList();
            var videos = await _dbContext
                .PlexOtherVideos.Where(x =>
                    ids.Contains(x.Id) && x.PlexLibraryId == library.Id && x.PlexServerId == library.PlexServerId
                )
                .Include(x => x.MediaDataList)
                .ToListAsync(cancellationToken);
            if (
                videos.Count != ids.Count
                || videos.Any(x => x.PlexLibraryId != library.Id || x.PlexServerId != library.PlexServerId)
            )
                return ResultExtensions.Create400BadRequestResult(
                    "Every selected media item must belong to the requested library and server."
                );

            foreach (var video in videos)
            {
                videosById[video.Id] = video;
                sourceSelections[video.Id] =
                    sourceSelections.TryGetValue(video.Id, out var previous)
                    && selection.Qualities.All(x => x.MediaId != video.Id)
                    && previous.Qualities.Any(x => x.MediaId == video.Id)
                        ? selection with
                        {
                            Qualities = previous.Qualities.Where(x => x.MediaId == video.Id).ToList(),
                        }
                        : selection;
            }
        }

        var previews = new List<DownloadPreview>();
        foreach (var video in videosById.Values)
        {
            var data = video.MediaDataList;
            var groups = data.GroupBy(x => x.PlexApiMediaId).ToList();
            var selector = sourceSelections[video.Id].Qualities.FirstOrDefault(x => x.MediaId == video.Id);
            var selectedData = data.FirstOrDefault(x => x.Id == selector?.DataId);
            if (selectedData is not null)
                groups.RemoveAll(x => x.Key != selectedData.PlexApiMediaId);

            previews.Add(video.ProjectToDownloadPreviewMapper(groups));
        }

        return Result.Ok<IEnumerable<DownloadPreview>>(previews);
    }
}
