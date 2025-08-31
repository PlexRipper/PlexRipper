namespace Reaparr.BaseTests;

public static class TestPlexLibraryExtensions
{
    public static List<PlexLibrary> ToApiLibraries(this List<PlexLibrary> plexLibraries, DateTime updatedTime)
    {
        foreach (var plexLibrary in plexLibraries)
        {
            plexLibrary.Id = 0;
            plexLibrary.SetNull();
            plexLibrary.UpdatedAt = updatedTime;
            plexLibrary.SyncedAt = null;
            plexLibrary.DefaultDestinationId = null;
            plexLibrary.SetMovieMetaData(0, 0);
            plexLibrary.SetTvShowMetaData(0, 0, 0, 0);
        }

        return plexLibraries;
    }
}
