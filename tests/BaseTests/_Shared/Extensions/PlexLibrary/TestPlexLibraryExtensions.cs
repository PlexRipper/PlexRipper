namespace Reaparr.BaseTests;

public static class TestPlexLibraryExtensions
{
    public static List<PlexLibrary> ToApiLibraries(this List<PlexLibrary> plexLibraries, DateTime updatedTime)
    {
        return plexLibraries
            .Select(plexLibrary => new PlexLibrary()
            {
                Id = 0,
                Uuid = plexLibrary.Uuid,
                Title = plexLibrary.Title,
                Type = plexLibrary.Type,
                Language = plexLibrary.Language,
                UpdatedAt = updatedTime,
                SyncedAt = null,
                DefaultDestinationId = null,
                CreatedAt = plexLibrary.CreatedAt,
                PlexServerId = plexLibrary.PlexServerId,
                Key = plexLibrary.Key,
                ScannedAt = plexLibrary.ScannedAt,
                MovieCount = 0,
                TvShowCount = 0,
                SeasonCount = 0,
                EpisodeCount = 0,
                MediaSize = 0,
            })
            .ToList();
    }
}
