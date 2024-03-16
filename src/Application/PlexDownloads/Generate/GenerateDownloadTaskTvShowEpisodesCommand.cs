using Application.Contracts;
using Data.Contracts;
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
        RuleForEach(x => x.DownloadMedias).SetValidator(new DownloadMediaDTOValidator());
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
        try
        {
            var groupedList = command.DownloadMedias.MergeAndGroupList();
            var plexEpisodeList = groupedList.FindAll(x => x.Type == PlexMediaType.Episode);
            if (!plexEpisodeList.Any())
                return ResultExtensions.IsEmpty(nameof(plexEpisodeList)).LogWarning();

            _log.Debug("Creating {PlexEpisodeIdsCount} episodes download tasks", plexEpisodeList
                .SelectMany(x => x.MediaIds)
                .ToList()
                .Count);

            _downloadFolder = await _dbContext.FolderPaths.GetDownloadFolderAsync(cancellationToken);
            _defaultDestinationDict = await _dbContext.FolderPaths.GetDefaultDestinationFolderDictionary(cancellationToken);

            foreach (var downloadMediaDto in plexEpisodeList)
            {
                var plexLibrary = await _dbContext.PlexLibraries
                    .Include(x => x.PlexServer)
                    .Include(x => x.DefaultDestination)
                    .GetAsync(downloadMediaDto.PlexLibraryId, cancellationToken);
                var destinationFolder = await GetDestinationFolder(plexLibrary, cancellationToken);

                var plexEpisodes = await _dbContext.PlexTvShowEpisodes
                    .AsTracking()
                    .IncludeAll()
                    .Where(x => downloadMediaDto.MediaIds.Contains(x.Id))
                    .ToListAsync(cancellationToken);

                var tvShowDownloads = new List<DownloadTaskTvShow>();

                foreach (var tvShowEpisode in plexEpisodes)
                {
                    var plexTvShow = tvShowEpisode.TvShow;
                    var plexSeason = tvShowEpisode.TvShowSeason;

                    // Check if the tvShowDownloadTask has already been created this run
                    var downloadTaskTvShow = tvShowDownloads.FirstOrDefault(x => x.Key == plexTvShow.Key);

                    // Check if the tvShowDownloadTask has already been created ever
                    if (downloadTaskTvShow is null)
                    {
                        downloadTaskTvShow = await _dbContext.GetDownloadTaskTvShowByMediaKeyQuery(plexTvShow.PlexServerId, plexTvShow.Key, cancellationToken);
                        if (downloadTaskTvShow is not null)
                            tvShowDownloads.Add(downloadTaskTvShow);
                    }

                    if (downloadTaskTvShow is null)
                    {
                        downloadTaskTvShow = plexTvShow.MapToDownloadTask();
                        tvShowDownloads.Add(downloadTaskTvShow);
                        _dbContext.DownloadTaskTvShow.Add(downloadTaskTvShow);
                    }

                    // Check if the SeasonDownloadTask has already been created
                    var downloadTaskTvShowSeason = downloadTaskTvShow.Children.FirstOrDefault(x => x.Key == plexSeason.Key);
                    if (downloadTaskTvShowSeason is null)
                    {
                        downloadTaskTvShowSeason = plexSeason.MapToDownloadTask();
                        downloadTaskTvShowSeason.ParentId = downloadTaskTvShow.Id;
                        downloadTaskTvShow.Children.Add(downloadTaskTvShowSeason);
                        _dbContext.DownloadTaskTvShowSeason.Add(downloadTaskTvShowSeason);
                    }

                    // Check if the tvShowEpisodesDownloadTask has already been created
                    var episodeDownloadTask = downloadTaskTvShowSeason.Children.FirstOrDefault(x => x.Key == plexSeason.Key);
                    if (episodeDownloadTask is null)
                    {
                        episodeDownloadTask = tvShowEpisode.MapToDownloadTask();
                        episodeDownloadTask.ParentId = downloadTaskTvShowSeason.Id;
                        downloadTaskTvShowSeason.Children.Add(episodeDownloadTask);
                        _dbContext.DownloadTaskTvShowEpisode.Add(episodeDownloadTask);
                    }

                    // TODO Quality Selector needs to be implemented here
                    var episodeData = tvShowEpisode.EpisodeData.First();

                    // Map movieData to DownloadTaskMovieFile and add to movieDownloadTask
                    var downloadFiles = episodeData.MapToDownloadTask(tvShowEpisode);
                    episodeDownloadTask.Children.AddRange(downloadFiles);
                    _dbContext.DownloadTaskTvShowEpisodeFile.AddRange(downloadFiles);

                    var downloadFolderPath = GetDownloadFolderPath(episodeDownloadTask);
                    var destinationFolderPath = GetDestinationFolderPath(destinationFolder.DirectoryPath, episodeDownloadTask);

                    // Set download and destination folder of each downloadable file
                    // TODO Might need to be set when download starts to allow free FolderPath's change
                    foreach (var downloadTaskTvShowEpisodeFile in episodeDownloadTask.Children)
                    {
                        downloadTaskTvShowEpisodeFile.DestinationDirectory = destinationFolderPath;
                        downloadTaskTvShowEpisodeFile.DownloadDirectory = downloadFolderPath;
                    }
                }
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception ex)
        {
            return Result.Fail(new ExceptionalError(ex)).LogError();
        }
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