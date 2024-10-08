using Application.Contracts;
using Application.Contracts.Validators;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

/// <summary>
/// Creates <see cref="DownloadTaskMovie">DownloadTaskMovies</see> from <see cref="PlexMovie">PlexMovies</see> and inserts it into Database.
/// </summary>
/// <returns>The created <see cref="DownloadTaskGeneric"/>.</returns>
public record GenerateDownloadTaskMoviesCommand(List<DownloadMediaDTO> DownloadMedias) : IRequest<Result>;

public class GenerateDownloadTaskMoviesCommandValidator : AbstractValidator<GenerateDownloadTaskMoviesCommand>
{
    public GenerateDownloadTaskMoviesCommandValidator()
    {
        RuleFor(x => x.DownloadMedias).NotNull();
        RuleFor(x => x.DownloadMedias).NotEmpty();
        RuleForEach(x => x.DownloadMedias).SetValidator(new DownloadMediaDTOValidator());
    }
}

public class GenerateDownloadTaskMoviesCommandHandler : IRequestHandler<GenerateDownloadTaskMoviesCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public GenerateDownloadTaskMoviesCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(GenerateDownloadTaskMoviesCommand command, CancellationToken cancellationToken)
    {
        var groupedList = command.DownloadMedias.MergeAndGroupList();
        var plexMoviesList = groupedList.FindAll(x => x.Type == PlexMediaType.Movie);
        if (!plexMoviesList.Any())
            return ResultExtensions.IsEmpty(nameof(plexMoviesList)).LogWarning();

        _log.Debug(
            "Creating {PlexMovieIdsCount} movie download tasks",
            plexMoviesList.SelectMany(x => x.MediaIds).ToList().Count
        );

        // Create downloadTasks
        var downloadTasks = new List<DownloadTaskMovie>();
        foreach (var downloadMediaDto in plexMoviesList)
        {
            var plexLibrary = await _dbContext
                .PlexLibraries.Include(x => x.PlexServer)
                .Include(x => x.DefaultDestination)
                .GetAsync(downloadMediaDto.PlexLibraryId, cancellationToken);
            if (plexLibrary is null)
            {
                ResultExtensions.EntityNotFound(nameof(PlexLibrary), downloadMediaDto.PlexLibraryId).LogError();
                continue;
            }

            var plexServer = plexLibrary.PlexServer!;

            var plexMovies = await _dbContext
                .PlexMovies.Where(x => downloadMediaDto.MediaIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var plexMovie in plexMovies)
            {
                // TODO Check for duplicate DownloadTasks already existing
                var movieDownloadTask = plexMovie.MapToDownloadTask();

                // TODO Takes first entry which assumes its the highest quality one
                var movieData = plexMovie.MovieData.First();

                // Map movieData to DownloadTaskMovieFile and add to movieDownloadTask
                movieDownloadTask.Children.AddRange(movieData.MapToDownloadTask(plexMovie));

                movieDownloadTask.Calculate();

                downloadTasks.Add(movieDownloadTask);
            }

            downloadTasks.SetRelationshipIds(plexServer.Id, plexLibrary.Id);

            _dbContext.DownloadTaskMovie.AddRange(downloadTasks);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}