namespace Reaparr.Application;

public record GetMovieDownloadPreviewCommand(List<DownloadMediaDTO> DownloadMedias)
    : ICommand<Result<IEnumerable<DownloadPreview>>>;

public class GetMovieDownloadPreviewCommandValidator : AbstractValidator<GetMovieDownloadPreviewCommand>
{
    public GetMovieDownloadPreviewCommandValidator()
    {
        RuleFor(x => x).NotNull();
        RuleFor(x => x.DownloadMedias).NotNull();
        RuleForEach(x => x.DownloadMedias).NotNull().SetValidator(new DownloadMediaDTOValidator());
        RuleForEach(x => x.DownloadMedias)
            .Must(x => x is null || x.Type == PlexMediaType.Movie)
            .WithMessage("Movie preview selections must contain only movie media.");
    }
}

public class GetMovieDownloadPreviewCommandHandler
    : ICommandHandler<GetMovieDownloadPreviewCommand, Result<IEnumerable<DownloadPreview>>>
{
    private readonly IReaparrDbContext _dbContext;

    public GetMovieDownloadPreviewCommandHandler(IReaparrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Result<IEnumerable<DownloadPreview>>> ExecuteAsync(
        GetMovieDownloadPreviewCommand command,
        CancellationToken cancellationToken
    ) => CreateMoviePreviews(command.DownloadMedias, cancellationToken);

    private async Task<Result<IEnumerable<DownloadPreview>>> CreateMoviePreviews(
        List<DownloadMediaDTO> downloadMedias,
        CancellationToken cancellationToken
    )
    {
        var moviesDownloadMedia = downloadMedias.Merge(PlexMediaType.Movie);
        if (!moviesDownloadMedia.Any() || !moviesDownloadMedia.Any(x => x.MediaIds.Any()))
            return Result.Ok(Enumerable.Empty<DownloadPreview>());

        var previews = new List<DownloadPreview>();
        var movieQualities = moviesDownloadMedia.SelectMany(x => x.Qualities).ToList();
        var movieIdsWithQuality = movieQualities.Select(x => x.MediaId).ToHashSet();

        // Fetch movies with specific qualities
        if (movieQualities.Any())
        {
            var movieMediaDataIds = movieQualities.Select(x => x.DataId).ToHashSet();

            var resultWithQualities = await _dbContext
                .PlexMovies.Include(x => x.MediaDataList.Where(y => movieMediaDataIds.Contains(y.Id)))
                .Where(x => movieIdsWithQuality.Contains(x.Id))
                .ProjectToDownloadPreview()
                .ToListAsync(cancellationToken);

            previews.AddRange(resultWithQualities);
        }

        // Fetch movies without specific qualities (use best available quality)
        var movieIds = moviesDownloadMedia.SelectMany(x => x.MediaIds).Except(movieIdsWithQuality).ToHashSet();
        if (movieIds.Any())
        {
            var result = await _dbContext
                .PlexMovies.Include(x => x.MediaDataList)
                .Where(x => movieIds.Contains(x.Id))
                .ProjectToDownloadPreview()
                .ToListAsync(cancellationToken);

            previews.AddRange(result);
        }

        var sortedPreviews = previews.OrderByNatural(x => x.Title);
        return Result.Ok(sortedPreviews);
    }
}
