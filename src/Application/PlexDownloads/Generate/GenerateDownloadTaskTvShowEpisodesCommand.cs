using Data.Contracts;
using DownloadManager.Contracts;
using DownloadManager.Contracts.Extensions;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Domain.PlexMediaExtensions;

namespace PlexRipper.Application;

public record GenerateDownloadTaskTvShowEpisodesCommand(List<DownloadMediaDTO> DownloadMedias) : IRequest<Result>;

public class GenerateDownloadTaskTvShowEpisodesCommandValidator : AbstractValidator<GenerateDownloadTaskTvShowEpisodesCommand>
{
    public GenerateDownloadTaskTvShowEpisodesCommandValidator()
    {
        RuleFor(x => x.DownloadMedias).NotNull();
        RuleFor(x => x.DownloadMedias).NotEmpty();
    }
}

public class GenerateDownloadTaskTvShowEpisodesCommandHandler : IRequestHandler<GenerateDownloadTaskTvShowEpisodesCommand, Result>
{
    private readonly ILog _log;
    private readonly IPlexRipperDbContext _dbContext;

    private FolderPath _downloadFolder;
    private Dictionary<PlexMediaType, FolderPath> _defaultDestinationDict;

    public GenerateDownloadTaskTvShowEpisodesCommandHandler(ILog log, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(GenerateDownloadTaskTvShowEpisodesCommand command, CancellationToken cancellationToken)
    {
        var groupedList = command.DownloadMedias.MergeAndGroupList();
        var plexEpisodeList = groupedList.FindAll(x => x.Type == PlexMediaType.Episode);
        if (!plexEpisodeList.Any())
            return ResultExtensions.IsEmpty(nameof(plexEpisodeList)).LogWarning();

        _downloadFolder = await _dbContext.FolderPaths.GetDownloadFolderAsync(cancellationToken);
        _defaultDestinationDict = await _dbContext.FolderPaths.GetDefaultDestinationFolderDictionary(cancellationToken);

        var episodesIds = new List<DownloadMediaDTO>();

        foreach (var downloadMediaDto in plexEpisodeList)
        {
            var plexLibrary = await _dbContext.PlexLibraries
                .Include(x => x.PlexServer)
                .Include(x => x.DefaultDestination)
                .GetAsync(downloadMediaDto.PlexLibraryId, cancellationToken);
            var plexServer = plexLibrary.PlexServer;
            var destinationFolder = await GetDestinationFolder(plexLibrary, cancellationToken);

            var plexEpisodes = await _dbContext.PlexTvShowEpisodes.IncludeAll()
                .Where(x => downloadMediaDto.MediaIds.Contains(x.Id))
                .ToListAsync(cancellationToken);

            var episodesToInsert = new List<DownloadTaskTvShowEpisode>();
            foreach (var tvShowEpisode in plexEpisodes)
            {
                var plexTvShow = tvShowEpisode.TvShow;
                var plexSeason = tvShowEpisode.TvShowSeason;

                // Check if the tvShowDownloadTask has already been created
                var downloadTaskTvShow = await _dbContext.GetDownloadTaskTvShowByMediaKeyQuery(plexTvShow.PlexServerId, plexTvShow.Key, cancellationToken);
                if (downloadTaskTvShow is null)
                {
                    // Insert the tvShowDownloadTask into the database
                    downloadTaskTvShow = plexTvShow.MapToDownloadTask();
                    _dbContext.DownloadTaskTvShow.Add(downloadTaskTvShow);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                }

                // Check if the SeasonDownloadTask has already been created
                var downloadTaskTvShowSeason = downloadTaskTvShow.Children
                    .Find(x => x.PlexServerId == plexServer.Id && x.Key == plexSeason.Key);
                if (downloadTaskTvShowSeason is null)
                {
                    var seasonDownloadTask = plexSeason.MapToDownloadTask();
                    seasonDownloadTask.ParentId = downloadTaskTvShow.Id;
                    _dbContext.DownloadTaskTvShowSeason.Add(seasonDownloadTask);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    downloadTaskTvShow.Children.Add(seasonDownloadTask);
                }

                // Check if the tvShowEpisodesDownloadTask has already been created
                var episodeDownloadTask = downloadTaskTvShowSeason.Children.FirstOrDefault(x => x.Key == plexSeason.Key && x.PlexServerId == plexServer.Id);
                if (episodeDownloadTask is null)
                {
                    episodeDownloadTask = tvShowEpisode.MapToDownloadTask();
                    episodeDownloadTask.ParentId = downloadTaskTvShowSeason.Id;
                    downloadTaskTvShowSeason.Children.Add(episodeDownloadTask);
                    episodesToInsert.Add(episodeDownloadTask);
                }

                var episodeData = tvShowEpisode.EpisodeData.First();

                // Map movieData to DownloadTaskMovieFile and add to movieDownloadTask
                episodeDownloadTask.Children.AddRange(episodeData.MapToDownloadTask(tvShowEpisode));

                var downloadFolderPath = GetDownloadFolderPath(episodeDownloadTask);
                var destinationFolderPath = GetDestinationFolderPath(destinationFolder.DirectoryPath, episodeDownloadTask);

                // Set download and destination folder of each downloadable file
                // TODO Might need to be set when download starts to allow free FolderPath's change
                foreach (var downloadTaskTvShowEpisodeFile in episodeDownloadTask.Children)
                {
                    downloadTaskTvShowEpisodeFile.DestinationDirectory = destinationFolderPath;
                    downloadTaskTvShowEpisodeFile.DownloadDirectory = downloadFolderPath;
                }

                episodeDownloadTask.Calculate();
            }

            // Insert all episodes into the database
            episodesToInsert.SetRelationshipIds(plexServer.Id, plexLibrary.Id);
            _dbContext.DownloadTaskTvShowEpisode.AddRange(episodesToInsert);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Result.Ok();
    }

    private string GetDownloadFolderPath(DownloadTaskTvShowEpisode downloadTaskTvShowEpisode) => Path.Join(_downloadFolder.DirectoryPath, "Movies",
        $"{downloadTaskTvShowEpisode.Title} ({downloadTaskTvShowEpisode.Year})", "/");

    private string GetDestinationFolderPath(string destinationFolder, DownloadTaskTvShowEpisode downloadTaskEpisode) =>
        Path.Join(destinationFolder, $"{downloadTaskEpisode.Title} ({downloadTaskEpisode.Year})", "/");

    private async Task<FolderPath> GetDestinationFolder(PlexLibrary library, CancellationToken cancellationToken = default)
    {
        if (library.DefaultDestinationId is null || library.DefaultDestinationId == 0)
            return _defaultDestinationDict[library.Type];

        return await _dbContext.FolderPaths.GetAsync(library.DefaultDestinationId ?? 0, cancellationToken);
    }
}