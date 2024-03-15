namespace PlexRipper.Domain;

public static partial class DbContextExtensions
{
    public static List<DownloadTaskGeneric> ToGeneric(this List<DownloadTaskMovie> downloadTaskMovies) =>
        downloadTaskMovies.Select(x => x.ToGeneric()).ToList();

    public static List<DownloadTaskGeneric> ToGeneric(this List<DownloadTaskTvShow> downloadTaskTvShows) =>
        downloadTaskTvShows.Select(x => x.ToGeneric()).ToList();

    public static List<DownloadTaskGeneric> ToGeneric(this List<DownloadTaskTvShowSeason> downloadTaskSeasons) =>
        downloadTaskSeasons.Select(x => x.ToGeneric()).ToList();

    public static List<DownloadTaskGeneric> ToGeneric(this List<DownloadTaskTvShowEpisode> downloadTaskEpisodes) =>
        downloadTaskEpisodes.Select(x => x.ToGeneric()).ToList();
}