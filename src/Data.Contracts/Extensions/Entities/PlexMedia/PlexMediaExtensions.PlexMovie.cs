using Reaparr.Domain;

namespace Reaparr.Data.Contracts;

public static partial class PlexMediaExtensions
{
    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(this ICollection<PlexMovie> movies, int plexServerId, int plexLibraryId)
    {
        foreach (var movie in movies)
        {
            movie.PlexLibraryId = plexLibraryId;
            movie.PlexServerId = plexServerId;
            movie.MediaDataList.SetRelationshipIds(plexServerId, plexLibraryId, movie.Id);
        }
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(
        this ICollection<PlexMovieMediaData> list,
        int plexServerId,
        int plexLibraryId,
        int plexMovieId
    )
    {
        foreach (var mediaData in list)
        {
            mediaData.PlexLibraryId = plexLibraryId;
            mediaData.PlexServerId = plexServerId;
            mediaData.PlexMovieId = plexMovieId;
            mediaData.Parts.SetRelationshipIds(plexServerId, plexLibraryId, plexMovieId, mediaData.Id);
        }
    }

    /// <summary>
    /// This will set the relationship ids for the download tasks and it's children.
    /// </summary>
    public static void SetRelationshipIds(
        this ICollection<PlexMovieMediaDataPart> parts,
        int plexServerId,
        int plexLibraryId,
        int plexMovieId,
        int plexMovieMediaDataId
    )
    {
        foreach (var part in parts)
        {
            part.PlexLibraryId = plexLibraryId;
            part.PlexServerId = plexServerId;
            part.PlexMovieId = plexMovieId;
            part.PlexMovieMediaDataId = plexMovieMediaDataId;
        }
    }
}
