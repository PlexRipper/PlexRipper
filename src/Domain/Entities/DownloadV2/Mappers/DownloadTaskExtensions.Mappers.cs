using PlexRipper.Domain.DownloadV2;

namespace PlexRipper.Domain;

public static class DownloadTaskExtensions_Mappers
{
    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovie downloadTaskMovie)
    {
        return new DownloadTaskGeneric()
        {
            Id = downloadTaskMovie.Id,
            Key = downloadTaskMovie.Key,
            MediaType = downloadTaskMovie.MediaType,
            DownloadTaskType = downloadTaskMovie.DownloadTaskType,
            IsDownloadable = downloadTaskMovie.IsDownloadable,
            Children = downloadTaskMovie.Children.Select(x => x.ToGeneric()).ToList(),
        };
    }

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskMovieFile downloadTaskMovieFile) => new()
    {
        Id = downloadTaskMovieFile.Id,
        Key = downloadTaskMovieFile.Key,
        MediaType = downloadTaskMovieFile.MediaType,
        DownloadTaskType = downloadTaskMovieFile.DownloadTaskType,
        IsDownloadable = downloadTaskMovieFile.IsDownloadable,
    };

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShow downloadTaskTvShow)
    {
        return new DownloadTaskGeneric()
        {
            Id = downloadTaskTvShow.Id,
            Key = downloadTaskTvShow.Key,
            MediaType = downloadTaskTvShow.MediaType,
            DownloadTaskType = downloadTaskTvShow.DownloadTaskType,
            IsDownloadable = downloadTaskTvShow.IsDownloadable,
            Children = downloadTaskTvShow.Children.Select(x => x.ToGeneric()).ToList(),
        };
    }

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowSeason downloadTaskTvShowSeason)
    {
        return new DownloadTaskGeneric()
        {
            Id = downloadTaskTvShowSeason.Id,
            Key = downloadTaskTvShowSeason.Key,
            MediaType = downloadTaskTvShowSeason.MediaType,
            DownloadTaskType = downloadTaskTvShowSeason.DownloadTaskType,
            IsDownloadable = downloadTaskTvShowSeason.IsDownloadable,
            Children = downloadTaskTvShowSeason.Children.Select(x => x.ToGeneric()).ToList(),
        };
    }

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisode downloadTaskTvShowEpisode)
    {
        return new DownloadTaskGeneric()
        {
            Id = downloadTaskTvShowEpisode.Id,
            Key = downloadTaskTvShowEpisode.Key,
            MediaType = downloadTaskTvShowEpisode.MediaType,
            DownloadTaskType = downloadTaskTvShowEpisode.DownloadTaskType,
            IsDownloadable = downloadTaskTvShowEpisode.IsDownloadable,
            Children = downloadTaskTvShowEpisode.Children.Select(x => x.ToGeneric()).ToList(),
        };
    }

    public static DownloadTaskGeneric ToGeneric(this DownloadTaskTvShowEpisodeFile downloadTaskTvShowEpisodeFile) => new()
    {
        Id = downloadTaskTvShowEpisodeFile.Id,
        Key = downloadTaskTvShowEpisodeFile.Key,
        MediaType = downloadTaskTvShowEpisodeFile.MediaType,
        DownloadTaskType = downloadTaskTvShowEpisodeFile.DownloadTaskType,
        IsDownloadable = downloadTaskTvShowEpisodeFile.IsDownloadable,
    };
}