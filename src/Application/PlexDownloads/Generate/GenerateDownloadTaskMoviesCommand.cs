using Data.Contracts;
using DownloadManager.Contracts;
using DownloadManager.Contracts.Extensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.PlexMediaExtensions;

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
    }
}

public class GenerateDownloadTaskMoviesCommandHandler : IRequestHandler<GenerateDownloadTaskMoviesCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    private FolderPath _downloadFolder;
    private Dictionary<PlexMediaType, FolderPath> _defaultDestinationDict;

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

        _downloadFolder = await _dbContext.FolderPaths.GetDownloadFolderAsync(cancellationToken);
        _defaultDestinationDict = await _dbContext.FolderPaths.GetDefaultDestinationFolderDictionary(cancellationToken);

        _log.Debug("Creating {PlexMovieIdsCount} movie download tasks", plexMoviesList
            .SelectMany(x => x.MediaIds)
            .ToList()
            .Count);

        // Create downloadTasks
        var downloadTasks = new List<DownloadTaskMovie>();
        foreach (var downloadMediaDto in plexMoviesList)
        {
            var plexLibrary = await _dbContext.PlexLibraries
                .Include(x => x.PlexServer)
                .Include(x => x.DefaultDestination)
                .GetAsync(downloadMediaDto.PlexLibraryId, cancellationToken);
            var plexServer = plexLibrary.PlexServer;
            var destinationFolder = await GetDestinationFolder(plexLibrary, cancellationToken);

            var plexMovies = await _dbContext.PlexMovies
                .Where(x => downloadMediaDto.MediaIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            foreach (var plexMovie in plexMovies)
            {
                // TODO Check for duplicate DownloadTasks already existing
                var movieDownloadTask = plexMovie.MapToDownloadTask();
                var downloadFolderPath = GetDownloadFolderPath(movieDownloadTask);
                var destinationFolderPath = GetDestinationFolderPath(destinationFolder.DirectoryPath, movieDownloadTask);

                // TODO Takes first entry which assumes its the highest quality one
                var movieData = plexMovie.MovieData.First();

                // Map movieData to DownloadTaskMovieFile and add to movieDownloadTask
                movieDownloadTask.Children.AddRange(movieData.MapToDownloadTask(plexMovie));

                // Set download and destination folder of each downloadable file
                // TODO Might need to be set when download starts to allow free FolderPath's change
                foreach (var downloadTaskMovieFile in movieDownloadTask.Children)
                {
                    downloadTaskMovieFile.DestinationDirectory = destinationFolderPath;
                    downloadTaskMovieFile.DownloadDirectory = downloadFolderPath;
                }

                movieDownloadTask.Calculate();

                downloadTasks.Add(movieDownloadTask);
            }

            downloadTasks.SetRelationshipIds(plexServer.Id, plexLibrary.Id);

            _dbContext.DownloadTaskMovie.AddRange(downloadTasks);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private string GetDownloadFolderPath(DownloadTaskMovie downloadTaskMovie) =>
        Path.Join(_downloadFolder.DirectoryPath, "Movies", $"{downloadTaskMovie.Title} ({downloadTaskMovie.Year})", "/");

    private string GetDestinationFolderPath(string destinationFolder, DownloadTaskMovie downloadTaskMovie) =>
        Path.Join(destinationFolder, $"{downloadTaskMovie.Title} ({downloadTaskMovie.Year})", "/");

    private async Task<FolderPath> GetDestinationFolder(PlexLibrary library, CancellationToken cancellationToken = default)
    {
        if (library.DefaultDestinationId is null || library.DefaultDestinationId == 0)
            return _defaultDestinationDict[library.Type];

        return await _dbContext.FolderPaths.GetAsync(library.DefaultDestinationId ?? 0, cancellationToken);
    }
}