using Bogus;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public interface IFakeData
    {
        Faker<PlexServer> GetPlexServer(bool includeLibraries = false);

        Faker<PlexLibrary> GetPlexLibrary(int serverId, int plexLibraryId, PlexMediaType type, int numberOfMedia = 0);

        Faker<DownloadTask> GetMovieDownloadTask();

        Faker<PlexMovie> GetPlexMovies(int plexLibraryId);

        Faker<PlexTvShow> GetPlexTvShows(int plexLibraryId);
    }
}