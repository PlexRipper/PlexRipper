using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public class FakeData
    {
        private static readonly Random _random = new Random();

        public static Faker<PlexServer> GetPlexServer(bool includeLibraries = false)
        {
            var _server = MockServer.GetPlexMockServer();
            var uri = new Uri(_server.Urls[0]);

            var plexServer = new Faker<PlexServer>()
                .UseSeed(_random.Next(1, 100))
                .RuleFor(x => x.Id, f => 1)
                .RuleFor(x => x.Name, f => f.Company.CompanyName())
                .RuleFor(x => x.Address, uri.Host)
                .RuleFor(x => x.Scheme, uri.Scheme)
                .RuleFor(x => x.Port, uri.Port)
                .RuleFor(x => x.Host, uri.Host)
                .RuleFor(x => x.CreatedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));

            if (includeLibraries)
            {
                int plexLibraryId = 1;
                var libraryTypes = new[] { PlexMediaType.Movie, PlexMediaType.TvShow };
                var plexLibraries = new Faker<PlexLibrary>()
                    .RuleFor(x => x.Id, f => plexLibraryId++)
                    .RuleFor(x => x.Title, f => f.Company.CompanyName())
                    .RuleFor(x => x.PlexServerId, f => 1)
                    .RuleFor(x => x.Type, f => f.PickRandom(libraryTypes));
                plexServer.RuleFor(x => x.PlexLibraries, f => plexLibraries.Generate(5).ToList());
            }

            return plexServer;
        }

        public static Faker<PlexLibrary> GetPlexLibrary(int serverId, int plexLibraryId, PlexMediaType type, int numberOfMedia = 0)
        {
            var plexLibrary = new Faker<PlexLibrary>()
                .RuleFor(x => x.Id, f => plexLibraryId)
                .RuleFor(x => x.Title, f => f.Company.CompanyName())
                .RuleFor(x => x.Type, f => type)
                .RuleFor(x => x.PlexServerId, f => serverId)
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

            if (numberOfMedia == 0)
            {
                return plexLibrary;
            }

            if (type == PlexMediaType.Movie)
            {
                var plexMovies = GetPlexMovies(plexLibraryId);
                plexLibrary.RuleFor(x => x.Movies, f => plexMovies.Generate(numberOfMedia).ToList());
            }

            if (type == PlexMediaType.TvShow)
            {
                var plexTvShows = GetPlexTvShows(plexLibraryId);
                plexLibrary.RuleFor(x => x.TvShows, f => plexTvShows.Generate(numberOfMedia).ToList());
            }

            return plexLibrary;
        }

        public static Faker<DownloadTask> CreateMovieDownloadTask()
        {
            var plexServer = GetPlexServer().Generate(1).First();
            var plexLibrary = GetPlexLibrary(plexServer.Id, 1, PlexMediaType.Movie).Generate(1).First();
            var mediaFile = MockServer.GetDefaultMovieMockMediaData();

            return new Faker<DownloadTask>()
                .StrictMode(true)
                .RuleFor(x => x.MediaType, f => PlexMediaType.Movie)
                .RuleFor(x => x.Key, f => _random.Next(0, 10000))
                .RuleFor(x => x.ServerToken, f => f.Random.Guid().ToString())
                .RuleFor(x => x.Created, f => f.Date.Recent(30))
                .RuleFor(x => x.PlexServer, f => plexServer)
                .RuleFor(x => x.PlexServerId, f => plexServer.Id)
                .RuleFor(x => x.PlexLibrary, f => plexLibrary)
                .RuleFor(x => x.PlexLibraryId, f => plexLibrary.Id)
                .RuleFor(x => x.MetaData, () => new()
                {
                    MovieTitle = mediaFile.ParentFolderName,
                    MediaData = new List<PlexMediaData>
                    {
                        new()
                        {
                            Parts = new List<PlexMediaDataPart>
                            {
                                new()
                                {
                                    Size = mediaFile.ByteSize,
                                    ObfuscatedFilePath = mediaFile.RelativeUrl,
                                    File = mediaFile.FileName,
                                },
                            },
                        },
                    },
                })
                .RuleFor(x => x.DownloadFolder, () => new FolderPath
                {
                    DirectoryPath = FileSystemPaths.RootDirectory,
                })
                .RuleFor(x => x.DownloadFolderId, _ => 1)
                .RuleFor(x => x.DestinationFolder, () => new FolderPath
                {
                    DirectoryPath = FileSystemPaths.RootDirectory,
                })
                .RuleFor(x => x.DestinationFolderId, _ => 2);
        }

        public static Faker<PlexMovie> GetPlexMovies(int plexLibraryId)
        {
            var mediaContainer = new[] { "mkv", "mp4" };
            var movieIds = new List<int>();

            return new Faker<PlexMovie>()
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.PlexLibraryId, f => plexLibraryId)
                .RuleFor(x => x.Key, f => GetUniqueId(1, 10000, movieIds))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));
        }

        public static Faker<PlexTvShow> GetPlexTvShows(int plexLibraryId)
        {
            var mediaContainer = new[] { "mkv", "mp4" };

            var tvShowIds = new List<int>();
            var seasonIds = new List<int>();
            var episodeIds = new List<int>();

            var episodes = new Faker<PlexTvShowEpisode>()
                .RuleFor(x => x.Key, f => GetUniqueId(1, 10000, episodeIds))
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.PlexLibraryId, f => plexLibraryId)
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));

            var seasonIndex = 1;
            var seasons = new Faker<PlexTvShowSeason>()
                .RuleFor(x => x.Title, f => $"Season {seasonIndex++}")
                .RuleFor(x => x.Key, f => GetUniqueId(1, 10000, seasonIds))
                .RuleFor(x => x.PlexLibraryId, f => plexLibraryId)
                .RuleFor(x => x.Episodes, f => episodes.Generate(f.Random.Int(6, 10)).ToList())
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));

            return new Faker<PlexTvShow>()
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.PlexLibraryId, f => plexLibraryId)
                .RuleFor(x => x.Key, f => GetUniqueId(1, 10000, tvShowIds))
                .RuleFor(x => x.Seasons, f => seasons.Generate(f.Random.Int(6, 10)).ToList())
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));
        }

        private static int GetUniqueId(int min, int max, List<int> alreadyGenerated)
        {
            while (true)
            {
                int value = _random.Next(min, max);
                if (!alreadyGenerated.Contains(value))
                {
                    alreadyGenerated.Add(value);
                    return value;
                }
            }
        }
    }
}