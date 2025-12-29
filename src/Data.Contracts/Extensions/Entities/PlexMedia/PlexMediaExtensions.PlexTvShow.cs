using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public static partial class PlexMediaExtensions
{
    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(this ICollection<PlexTvShow> plexTvShows, int plexServerId, int plexLibraryId)
    {
        foreach (var tvShow in plexTvShows)
        {
            tvShow.PlexLibraryId = plexLibraryId;
            tvShow.PlexServerId = plexServerId;
            tvShow.Seasons.SetRelationshipIds(plexServerId, plexLibraryId, tvShow.Id);
        }
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(
        this ICollection<PlexTvShowSeason> seasons,
        int plexServerId,
        int plexLibraryId,
        int plexTvShowId
    )
    {
        foreach (var season in seasons)
        {
            season.PlexLibraryId = plexLibraryId;
            season.PlexServerId = plexServerId;
            season.TvShowId = plexTvShowId;
            season.Episodes.SetRelationshipIds(plexServerId, plexLibraryId, plexTvShowId, season.Id);
        }
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(
        this ICollection<PlexTvShowEpisode> episodes,
        int plexServerId,
        int plexLibraryId,
        int plexTvShowId,
        int plexTvShowSeasonId
    )
    {
        foreach (var episode in episodes)
        {
            episode.PlexLibraryId = plexLibraryId;
            episode.PlexServerId = plexServerId;
            episode.TvShowId = plexTvShowId;
            episode.TvShowSeasonId = plexTvShowSeasonId;
            episode.MediaDataList.SetRelationshipIds(plexServerId, plexLibraryId, episode.Id);
        }
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(
        this ICollection<PlexTvShowEpisodeMediaData> list,
        int plexServerId,
        int plexLibraryId,
        int plexTvShowEpisodeId
    )
    {
        foreach (var mediaData in list)
        {
            mediaData.PlexLibraryId = plexLibraryId;
            mediaData.PlexServerId = plexServerId;
            mediaData.PlexTvShowEpisodeId = plexTvShowEpisodeId;
        }
    }
}
