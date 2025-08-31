using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Application.Contracts.Validators;
using Reaparr.Data.Contracts;
using Serilog;

namespace Reaparr.Application;

/// <summary>
/// Creates <see cref="DownloadTaskMovie">DownloadTaskMovies</see> from <see cref="PlexMovie">PlexMovies</see> and inserts it into Database.
/// </summary>
/// <returns>The created <see cref="DownloadTaskGeneric"/>.</returns>
public record GenerateDownloadTaskMoviesCommand : ICommand<Result>
{
    public GenerateDownloadTaskMoviesCommand(CreateDownloadTasksRequest request)
    {
        Request = request;
    }

    public GenerateDownloadTaskMoviesCommand(List<DownloadMediaDTO> downloadMediaDtos)
    {
        Request = new CreateDownloadTasksRequest(downloadMediaDtos);
    }

    public CreateDownloadTasksRequest Request { get; }
}

public class GenerateDownloadTaskMoviesCommandValidator : AbstractValidator<GenerateDownloadTaskMoviesCommand>
{
    public GenerateDownloadTaskMoviesCommandValidator()
    {
        RuleFor(x => x.Request.DownloadMedias).NotNull();
        RuleFor(x => x.Request.DownloadMedias).NotEmpty();
        RuleForEach(x => x.Request.DownloadMedias).SetValidator(new DownloadMediaDTOValidator());
    }
}

public class GenerateDownloadTaskMoviesCommandHandler : ICommandHandler<GenerateDownloadTaskMoviesCommand, Result>
{
    private readonly Serilog.ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public GenerateDownloadTaskMoviesCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<GenerateDownloadTaskMoviesCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<Result> ExecuteAsync(
        GenerateDownloadTaskMoviesCommand command,
        CancellationToken cancellationToken
    )
    {
        var groupedList = command.Request.DownloadMedias.MergeAndGroupList();
        var request = command.Request;
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
                .IncludeAll()
                .ToListAsync(cancellationToken);

            foreach (var plexMovie in plexMovies)
            {
                // TODO: Check for duplicate DownloadTasks already existing
                var movieDownloadTask = plexMovie.MapToDownloadTask();

                var movieData = SelectMovieQuality(plexMovie, downloadMediaDto);
                if (movieData is null)
                {
                    _log.Error(
                        "Failed to select quality for movie {MovieTitle} (ID: {MovieId})",
                        plexMovie.Title,
                        plexMovie.Id
                    );
                    continue;
                }

                // Map movieData to DownloadTaskMovieFile and add to movieDownloadTask
                movieDownloadTask.Children.AddRange(movieData.MapToDownloadTask(plexMovie, request));

                movieDownloadTask.Calculate();

                downloadTasks.Add(movieDownloadTask);
            }

            downloadTasks.SetRelationshipIds(plexServer.Id, plexLibrary.Id);

            _dbContext.DownloadTaskMovie.AddRange(downloadTasks);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    /// <summary>
    /// Selects the appropriate movie quality from the available media data list.
    /// First attempts to use the requested quality, then falls back to the best available quality.
    /// </summary>
    /// <param name="plexMovie">The movie containing media data options</param>
    /// <param name="downloadMediaDto">The download request containing quality preferences</param>
    /// <returns>The selected movie media data, or null if no suitable quality is found</returns>
    private PlexMovieMediaData? SelectMovieQuality(PlexMovie plexMovie, DownloadMediaDTO downloadMediaDto)
    {
        if (!plexMovie.MediaDataList.Any())
        {
            _log.Warning(
                "Movie {MovieTitle} (ID: {MovieId}) has no media data available",
                plexMovie.Title,
                plexMovie.Id
            );
            return null;
        }

        // Try to find the specifically requested quality
        var requestedQuality = downloadMediaDto.Qualities.FirstOrDefault(x => x.MediaId == plexMovie.Id);
        if (requestedQuality is not null)
        {
            var specificQuality = plexMovie.MediaDataList.FirstOrDefault(x => x.Id == requestedQuality.DataId);
            if (specificQuality is not null)
            {
                _log.Debug(
                    "Selected requested quality {Quality} for movie {MovieTitle} (DataId: {DataId})",
                    requestedQuality.Quality,
                    plexMovie.Title,
                    requestedQuality.DataId
                );
                return specificQuality;
            }
        }

        // Fall back to the best available quality
        var bestQuality = plexMovie.MediaDataList.PickMediaQuality();
        if (bestQuality is not null)
        {
            _log.Debug(
                "Selected best available quality {Quality} for movie {MovieTitle} (DataId: {DataId})",
                bestQuality.Quality,
                plexMovie.Title,
                bestQuality.Id
            );
        }
        else
        {
            _log.Error(
                "No suitable quality found for movie {MovieTitle} (ID: {MovieId}) from {AvailableCount} media data options",
                plexMovie.Title,
                plexMovie.Id,
                plexMovie.MediaDataList.Count
            );
        }

        return bestQuality;
    }
}
