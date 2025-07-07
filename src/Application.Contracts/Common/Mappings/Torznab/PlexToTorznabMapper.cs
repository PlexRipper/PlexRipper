using Domain.Entities;

namespace PlexRipper.Application.Contracts;

public static class PlexToTorznabMapper
{
    public static TorznabItem ToTorznabItem(this PlexMovie movie, string downloadUrl, string baseUrl)
    {
        var item = new TorznabItem
        {
            Title = GetMovieTitle(movie),
            Guid = $"{baseUrl}/torznab/download/{movie.Id}",
            Link = downloadUrl,
            Description = movie.Summary ?? "",
            PubDate = movie.AddedAt.ToString("r"),
            Size = movie.MediaSize ?? 0,
            Enclosure = new TorznabEnclosure
            {
                Url = downloadUrl,
                Length = movie.MediaSize ?? 0,
                Type = "application/x-bittorrent"
            },
            Attributes = new List<TorznabAttribute>
            {
                new() { Name = "category", Value = "2000" }, // Movies
                new() { Name = "size", Value = (movie.MediaSize ?? 0).ToString() },
                new() { Name = "year", Value = movie.Year?.ToString() ?? "" },
                new() { Name = "genre", Value = string.Join(",", movie.Genre ?? new List<string>()) },
                new() { Name = "imdb", Value = movie.ImdbId ?? "" },
                new() { Name = "tmdb", Value = movie.TmdbId?.ToString() ?? "" },
                new() { Name = "resolution", Value = GetResolution(movie) },
                new() { Name = "language", Value = "en" },
                new() { Name = "seeders", Value = "1" },
                new() { Name = "peers", Value = "0" }
            }
        };

        return item;
    }

    public static TorznabItem ToTorznabItem(this PlexTvShowEpisode episode, string downloadUrl, string baseUrl)
    {
        var item = new TorznabItem
        {
            Title = GetEpisodeTitle(episode),
            Guid = $"{baseUrl}/torznab/download/{episode.Id}",
            Link = downloadUrl,
            Description = episode.Summary ?? "",
            PubDate = episode.AddedAt.ToString("r"),
            Size = episode.MediaSize ?? 0,
            Enclosure = new TorznabEnclosure
            {
                Url = downloadUrl,
                Length = episode.MediaSize ?? 0,
                Type = "application/x-bittorrent"
            },
            Attributes = new List<TorznabAttribute>
            {
                new() { Name = "category", Value = "5000" }, // TV
                new() { Name = "size", Value = (episode.MediaSize ?? 0).ToString() },
                new() { Name = "season", Value = episode.ParentIndex?.ToString() ?? "0" },
                new() { Name = "episode", Value = episode.Index?.ToString() ?? "0" },
                new() { Name = "tvdb", Value = episode.TvdbId?.ToString() ?? "" },
                new() { Name = "resolution", Value = GetResolution(episode) },
                new() { Name = "language", Value = "en" },
                new() { Name = "seeders", Value = "1" },
                new() { Name = "peers", Value = "0" }
            }
        };

        return item;
    }

    public static TorznabItem ToTorznabItem(this PlexTvShowSeason season, string downloadUrl, string baseUrl)
    {
        var item = new TorznabItem
        {
            Title = GetSeasonTitle(season),
            Guid = $"{baseUrl}/torznab/download/{season.Id}",
            Link = downloadUrl,
            Description = season.Summary ?? "",
            PubDate = season.AddedAt.ToString("r"),
            Size = season.MediaSize ?? 0,
            Enclosure = new TorznabEnclosure
            {
                Url = downloadUrl,
                Length = season.MediaSize ?? 0,
                Type = "application/x-bittorrent"
            },
            Attributes = new List<TorznabAttribute>
            {
                new() { Name = "category", Value = "5000" }, // TV
                new() { Name = "size", Value = (season.MediaSize ?? 0).ToString() },
                new() { Name = "season", Value = season.Index?.ToString() ?? "0" },
                new() { Name = "language", Value = "en" },
                new() { Name = "seeders", Value = "1" },
                new() { Name = "peers", Value = "0" }
            }
        };

        return item;
    }

    private static string GetMovieTitle(PlexMovie movie)
    {
        var title = movie.Title ?? "Unknown Movie";
        if (movie.Year.HasValue)
            title += $" ({movie.Year})";
        
        var resolution = GetResolution(movie);
        if (!string.IsNullOrEmpty(resolution))
            title += $" [{resolution}]";
            
        return title;
    }

    private static string GetEpisodeTitle(PlexTvShowEpisode episode)
    {
        var title = episode.TvShow?.Title ?? "Unknown Show";
        
        if (episode.ParentIndex.HasValue && episode.Index.HasValue)
            title += $" S{episode.ParentIndex:D2}E{episode.Index:D2}";
            
        if (!string.IsNullOrEmpty(episode.Title))
            title += $" {episode.Title}";
            
        var resolution = GetResolution(episode);
        if (!string.IsNullOrEmpty(resolution))
            title += $" [{resolution}]";
            
        return title;
    }

    private static string GetSeasonTitle(PlexTvShowSeason season)
    {
        var title = season.TvShow?.Title ?? "Unknown Show";
        
        if (season.Index.HasValue)
            title += $" Season {season.Index}";
        else
            title += " Complete Season";
            
        return title;
    }

    private static string GetResolution(PlexMediaSlim media)
    {
        // This is a simplified resolution detection
        // In a real implementation, you'd analyze media info
        if (media.MediaSize > 8_000_000_000) // > 8GB
            return "1080p";
        else if (media.MediaSize > 3_000_000_000) // > 3GB  
            return "720p";
        else
            return "480p";
    }
}