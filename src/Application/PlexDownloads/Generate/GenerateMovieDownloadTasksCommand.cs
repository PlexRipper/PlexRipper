using Data.Contracts;
using DownloadManager.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.PlexMediaExtensions;

namespace PlexRipper.Application;

/// <summary>
/// Creates <see cref="DownloadTaskMovie">DownloadTaskMovies</see> from <see cref="PlexMovie">PlexMovies</see> and inserts it into Database.
/// </summary>
/// <returns>The created <see cref="DownloadTask"/>.</returns>
public record GenerateMovieDownloadTasksCommand(List<DownloadMediaDTO> DownloadMedias) : IRequest<Result>;

public class GenerateMovieDownloadTasksCommandValidator : AbstractValidator<GenerateMovieDownloadTasksCommand>
{
    public GenerateMovieDownloadTasksCommandValidator()
    {
        RuleFor(x => x.DownloadMedias).NotNull();
        RuleFor(x => x.DownloadMedias).NotEmpty();
    }
}

public class GenerateMovieDownloadTasksCommandHandler : IRequestHandler<GenerateMovieDownloadTasksCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    public GenerateMovieDownloadTasksCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(GenerateMovieDownloadTasksCommand command, CancellationToken cancellationToken)
    {
        var plexMoviesList = command.DownloadMedias.FindAll(x => x.Type == PlexMediaType.Movie);
        var downloadFolder = await _dbContext.FolderPaths.GetDownloadFolderAsync(cancellationToken);

        var defaultDestinationDict = await _dbContext.FolderPaths.GetDefaultDestinationFolderDictionary(cancellationToken);

        _log.Debug("Creating {PlexMovieIdsCount} movie download tasks", plexMoviesList.SelectMany(x => x.MediaIds).ToList().Count);

        // Create downloadTasks
        var downloadTasks = new List<DownloadTaskMovie>();
        foreach (var downloadMediaDto in plexMoviesList)
        {
            var plexLibrary = await _dbContext.PlexLibraries.Include(x => x.PlexServer).GetAsync(downloadMediaDto.PlexLibraryId, cancellationToken);
            var plexServer = plexLibrary.PlexServer;

            var plexMovies = await _dbContext.PlexMovies
                .Where(x => downloadMediaDto.MediaIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var plexMovie in plexMovies)
            {
                // TODO Check for duplicate DownloadTasks already existing
                var movieDownloadTask = plexMovie.MapToDownloadTask();
                var downloadFolderPath = Path.Join(downloadFolder.DirectoryPath, "Movies", $"{movieDownloadTask.Title} ({movieDownloadTask.Year})", "/");

                // TODO Takes first entry which assumes its the highest quality one
                var movieData = plexMovie.MovieData.First();

                movieDownloadTask.Children.AddRange(movieData.MapToDownloadTask(plexMovie));

                // Set download and destination folder of each downloadable file
                // TODO Might need to be set when download starts to allow free FolderPath's change
                foreach (var downloadTaskMovieFile in movieDownloadTask.Children)
                    if (plexLibrary.DefaultDestinationId is not null)
                    {
                        downloadTaskMovieFile.DestinationFolderId = plexLibrary.DefaultDestinationId ?? default(int);
                        downloadTaskMovieFile.DownloadDirectory = downloadFolderPath;
                    }
                    else
                    {
                        var destination = defaultDestinationDict[movieDownloadTask.MediaType];
                        downloadTaskMovieFile.DestinationFolderId = destination.Id;
                        downloadTaskMovieFile.DownloadDirectory = downloadFolderPath;
                    }

                downloadTasks.Add(movieDownloadTask);
            }

            downloadTasks.SetRelationshipIds(plexServer.Id, plexLibrary.Id);

            _dbContext.DownloadTaskMovie.AddRange(downloadTasks);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }
}