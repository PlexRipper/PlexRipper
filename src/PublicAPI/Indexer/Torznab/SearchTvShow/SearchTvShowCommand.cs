using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Data.Contracts;
using Reaparr.Environment;

namespace Reaparr.PublicAPI.SearchTvShow;

public record SearchTvShowCommand : ICommand<TorznabMediaSearchResponseDTO>
{
    public required string Query { get; init; }

    public required int Season { get; init; }

    public required int Episode { get; init; }

    public required int Limit { get; init; }

    public required int Offset { get; init; }

    public required int TVDB_ID { get; init; }

    public required int TMDB_ID { get; init; }

    public required string IMDB_ID { get; init; }
}

public class SearchTvShowCommandValidator : AbstractValidator<SearchTvShowCommand>
{
    public SearchTvShowCommandValidator() { }
}

public class SearchTvShowCommandHandler : ICommandHandler<SearchTvShowCommand, TorznabMediaSearchResponseDTO>
{
    private readonly ILogger _log;
    private readonly IReaparrDbContext _dbContext;

    public SearchTvShowCommandHandler(ILogger log, IReaparrDbContext dbContext)
    {
        _log = log.ForContext<SearchTvShowCommandHandler>();
        _dbContext = dbContext;
    }

    public async Task<TorznabMediaSearchResponseDTO> ExecuteAsync(
        SearchTvShowCommand command,
        CancellationToken cancellationToken)
    {
        List<PlexTvShowEpisode> episodes;
        if (command is { Season: 0, Episode: 0, Query: "" })
        {
            episodes = await _dbContext.PlexTvShowEpisodes.Skip(command.Offset)
                .Take(command.Limit)
                .Include(x => x.TvShowSeason)
                .Include(x => x.TvShow)
                .Include(e => e.MediaDataList)
                .ThenInclude(x => x.Parts)
                .ToListAsync(cancellationToken);
        }
        else
        {
            episodes = await _dbContext.PlexTvShowEpisodes
                .Include(x => x.TvShowSeason)
                .Include(x => x.TvShow)
                .Where(e =>
                    e.TvShow!.Guid_IMDB == command.IMDB_ID
                    || e.TvShow.Guid_TMDB == command.TMDB_ID
                    || e.TvShow.Guid_TVDB == command.TVDB_ID)
                .Where(e => e.TvShowSeason!.SeasonNumber == command.Season)
                .Where(e => e.EpisodeNumber == command.Episode)
                .Include(e => e.MediaDataList)
                .ThenInclude(x => x.Parts)
                .ToListAsync(cancellationToken);
        }

        var items = new List<TorznabItem>();

        foreach (var episode in episodes)
        {
            var tvShow = episode.TvShow;
            var season = episode.TvShowSeason;
            if (tvShow is null)
            {
                _log.Error("TvShow is null for episode {EpisodeId}", episode.Id);
                continue;
            }

            if (season is null)
            {
                _log.Error("Season is null for episode {EpisodeId}", episode.Id);
                continue;
            }

            foreach (var mediaData in episode.MediaDataList)
            foreach (var part in mediaData.Parts)
            {
                var url = new TorrentMetadataDTO
                {
                    Type = PlexMediaType.Episode,
                    MediaId = episode.Id,
                    DataId = mediaData.Id,
                    Quality = mediaData.Quality,
                    LibraryId = part.PlexLibraryId,
                    ServerId = part.PlexServerId,
                }.ToUrl();

                _log.Here()
                    .Debug(
                        "Generated torrent URL for PlexTvShowEpisodeMediaDataPartId {PlexTvShowEpisodeMediaDataPartId}: {Url}",
                        part.Id, url);

                var item = new TorznabItem
                {
                    Title = Path.GetFileName(part.File),
                    PubDate = episode.AddedAt.ToString("R"),
                    Guid = new TorznabGuid { Value = url },
                    Link = url,
                    Size = part.Size,
                    Enclosure = new TorznabEnclosure
                    {
                        Url = url,
                        Length = part.Size,
                        Type = "application/x-bittorrent",
                    },
                };
                item.Attributes.Add(new TorznabAttr("season", season.SeasonNumber.ToString()));
                item.Attributes.Add(new TorznabAttr("episode", episode.EpisodeNumber.ToString()));
                item.Attributes.Add(new TorznabAttr("seeders", "100"));
                item.Attributes.Add(new TorznabAttr("peers", "100"));
                item.Attributes.Add(new TorznabAttr("type", "series"));
                item.Attributes.Add(new TorznabAttr("language", "English"));
                item.Attributes.Add(new TorznabAttr("downloadvolumefactor", "0.0")); // Freeleech 

                if (tvShow.Guid_TVDB is not null)
                    item.Attributes.Add(new TorznabAttr("tvdbid", tvShow.Guid_TVDB.ToString()));

                if (tvShow.Guid_TMDB is not null)
                    item.Attributes.Add(new TorznabAttr("tmdbid", tvShow.Guid_TMDB.ToString()));

                if (!string.IsNullOrEmpty(tvShow.Guid_IMDB))
                    item.Attributes.Add(new TorznabAttr("imdb", tvShow.Guid_IMDB));

                items.Add(item);
            }
        }

        return new TorznabMediaSearchResponseDTO
        {
            Channel = new TorznabChannel
            {
                Title = "Reaparr Indexer",
                Description = $"TV Search results for {command.Query}",
                Language = "en-us",
                Category = "search",
                Items = items,
            },
        };
    }
}