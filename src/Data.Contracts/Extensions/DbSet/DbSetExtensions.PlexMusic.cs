namespace Reaparr.Data.Contracts;

public static partial class DbSetExtensions
{
    public static IQueryable<PlexMusicArtist> IncludeAll(this IQueryable<PlexMusicArtist> artists) =>
        artists
            .Include(x => x.PlexServer)
            .Include(x => x.PlexLibrary)
            .Include(x => x.Genres)
            .Include(x => x.Qualities)
            .Include(x => x.Albums)
                .ThenInclude(x => x.Qualities)
            .Include(x => x.Albums)
                .ThenInclude(x => x.Tracks)
                    .ThenInclude(x => x.MediaDataList);

    public static IQueryable<PlexMusicAlbum> IncludeAll(this IQueryable<PlexMusicAlbum> albums) =>
        albums
            .Include(x => x.PlexServer)
            .Include(x => x.PlexLibrary)
            .Include(x => x.PlexArtist)
            .Include(x => x.Qualities)
            .Include(x => x.Tracks)
                .ThenInclude(x => x.MediaDataList);
}
