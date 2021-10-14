using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Bogus.Extensions;
using Environment;
using PlexRipper.Application.Common;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public static class FakeData
    {
        private static readonly Random _random = new();

        public static Faker<PlexServer> GetPlexServer(bool includeLibraries = false, string serverUrl = "https://test-server.com")
        {
            var uri = new Uri(serverUrl);

            var serverId = 1;
            var plexServer = new Faker<PlexServer>()
                .UseSeed(_random.Next(1, 100))
                .RuleFor(p => p.Id, f => serverId++)
                .RuleFor(x => x.Name, f => f.Company.CompanyName())
                .RuleFor(x => x.Address, uri.Host)
                .RuleFor(x => x.Scheme, uri.Scheme)
                .RuleFor(x => x.Port, uri.Port)
                .RuleFor(x => x.Host, uri.Host)
                .RuleFor(x => x.CreatedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));

            if (includeLibraries)
            {
                plexServer.RuleFor(x => x.PlexLibraries, _ => GetPlexLibrary(serverId).GenerateBetween(2, 8));
            }

            return plexServer;
        }

        public static Faker<PlexLibrary> GetPlexLibrary(int serverId, PlexMediaType type = PlexMediaType.None, int numberOfMedia = 0)
        {
            int plexLibraryId = 1;
            var plexLibrary = new Faker<PlexLibrary>()
                .RuleFor(x => x.Id, _ => plexLibraryId++)
                .RuleFor(x => x.Title, f => f.Company.CompanyName())
                .RuleFor(x => x.Type, _ => type)
                .RuleFor(x => x.PlexServerId, _ => serverId)
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

            if (type == PlexMediaType.None)
            {
                plexLibrary
                    .RuleFor(x => x.Type, f => f.PickRandom(new[] { PlexMediaType.Movie, PlexMediaType.TvShow }));
            }

            if (numberOfMedia == 0)
            {
                return plexLibrary;
            }

            if (type == PlexMediaType.Movie)
            {
                var plexMovies = GetPlexMovies(plexLibraryId, serverId);
                plexLibrary.RuleFor(x => x.Movies, _ => plexMovies.Generate(numberOfMedia).ToList());
            }

            if (type == PlexMediaType.TvShow)
            {
                var plexTvShows = GetPlexTvShows(plexLibraryId, serverId);
                plexLibrary.RuleFor(x => x.TvShows, _ => plexTvShows.Generate(numberOfMedia).ToList());
            }

            return plexLibrary;
        }

        public static Faker<DownloadTask> GetMovieDownloadTask()
        {
            var plexServer = GetPlexServer().Generate(1).First();
            var plexLibrary = GetPlexLibrary(plexServer.Id, PlexMediaType.Movie).Generate(1).First();

            return new Faker<DownloadTask>()
                .StrictMode(true)
                .RuleFor(x => x.Id, f => f.Random.Int(1, 1000))
                .RuleFor(x => x.DownloadStatus, _ => DownloadStatus.Initialized)
                .RuleFor(x => x.Priority, _ => 0)
                .RuleFor(x => x.DataReceived, _ => 0)
                .RuleFor(x => x.DataTotal, f => f.Random.Long(1, 10000000))
                .RuleFor(x => x.DownloadWorkerTasks, _ => new())
                .RuleFor(x => x.MediaType, PlexMediaType.Movie)
                .RuleFor(x => x.Key, _ => _random.Next(0, 10000))
                .RuleFor(x => x.Created, f => f.Date.Recent(30))
                .RuleFor(x => x.PlexServer, _ => plexServer)
                .RuleFor(x => x.PlexServerId, _ => plexServer.Id)
                .RuleFor(x => x.PlexLibrary, _ => plexLibrary)
                .RuleFor(x => x.PlexLibraryId, _ => plexLibrary.Id)
                .RuleFor(x => x.DownloadFolder, () => new FolderPath
                {
                    DirectoryPath = PathSystem.RootDirectory,
                })
                .RuleFor(x => x.DownloadFolderId, _ => 1)
                .RuleFor(x => x.DestinationFolder, () => new FolderPath
                {
                    DirectoryPath = PathSystem.RootDirectory,
                })
                .RuleFor(x => x.DestinationFolderId, _ => 2)
                .FinishWith((_, u) =>
                {
                    u.DownloadWorkerTasks = new List<DownloadWorkerTask>
                    {
                        new(u, 1, 0, u.DataTotal),
                    };
                });
        }

        public static Faker<PlexMovie> GetPlexMovies(int plexLibraryId, int plexServerId, int movieQualities = 1, int movieParts = 1)
        {
            var movieIds = new List<int>();
            var movieKeys = new List<int>();

            return new Faker<PlexMovie>()
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.MediaData, _ => new PlexMediaContainer
                {
                    MediaData = GetPlexMediaData(movieParts).Generate(movieQualities),
                })
                .RuleFor(x => x.PlexServerId, _ => plexServerId)
                .RuleFor(x => x.PlexServer, _ => new PlexServer() { Id = plexLibraryId })
                .RuleFor(x => x.PlexLibraryId, _ => plexLibraryId)
                .RuleFor(x => x.PlexLibrary, _ => new PlexLibrary() { Id = plexLibraryId })
                .RuleFor(x => x.Key, _ => GetUniqueId(1, 10000, movieKeys))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));
        }

        public static Faker<PlexMediaData> GetPlexMediaData(int movieParts = 1)
        {
            return new Faker<PlexMediaData>()
                .RuleFor(x => x.Bitrate, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.MediaFormat, f => f.System.FileExt("video/mp4"))
                .RuleFor(x => x.Width, f => f.Random.Int(240, 10000))
                .RuleFor(x => x.Height, f => f.Random.Int(240, 10000))
                .RuleFor(x => x.VideoCodec, f => f.System.FileType())
                .RuleFor(x => x.AudioChannels, f => f.Random.Int(2, 5))
                .RuleFor(x => x.VideoResolution, f => f.PickRandom("sd", "720p", "1080p"))
                .RuleFor(x => x.Duration, f => f.Random.Long(50000, 55124400))
                .RuleFor(x => x.OptimizedForStreaming, f => f.Random.Bool())
                .RuleFor(x => x.Parts, f => GetPlexMediaPart().Generate(movieParts));
        }

        public static Faker<PlexMediaDataPart> GetPlexMediaPart()
        {
            return new Faker<PlexMediaDataPart>()
                .RuleFor(x => x.ObfuscatedFilePath, f => "/library/parts/65125/1193813456/file.avi")
                .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
                .RuleFor(x => x.File, f => "/KidsMovies/Fantastic Four 2/F4 Rise of the Silver Surfer.avi")
                .RuleFor(x => x.Size, f => f.Random.Long(50000, 55124400))
                .RuleFor(x => x.Container, f => f.System.FileExt("video/mp4"))
                .RuleFor(x => x.VideoProfile, f => f.Random.Words(2))
                .RuleFor(x => x.Indexes, f => f.Random.Word());
        }

        public static Faker<PlexTvShow> GetPlexTvShows(int plexLibraryId, int plexServerId)
        {
            var tvShowIds = new List<int>();
            var seasonIds = new List<int>();
            var episodeIds = new List<int>();

            var episodes = new Faker<PlexTvShowEpisode>()
                .RuleFor(x => x.Key, _ => GetUniqueId(1, 10000, episodeIds))
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.FullTitle, f => f.Lorem.Word())
                .RuleFor(x => x.PlexLibraryId, _ => plexLibraryId)
                .RuleFor(x => x.PlexServerId, _ => plexServerId)
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.MediaData, _ => new PlexMediaContainer
                {
                    MediaData = GetPlexMediaData(1).Generate(1),
                })
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));

            var seasonIndex = 1;
            var seasons = new Faker<PlexTvShowSeason>()
                .RuleFor(x => x.Title, _ => $"Season {seasonIndex++}")
                .RuleFor(x => x.FullTitle, f => $"{f.Lorem.Word()}/{f.Lorem.Word()}")
                .RuleFor(x => x.Key, _ => GetUniqueId(1, 10000, seasonIds))
                .RuleFor(x => x.PlexLibraryId, _ => plexLibraryId)
                .RuleFor(x => x.PlexServerId, _ => plexServerId)
                .RuleFor(x => x.Episodes, f => episodes.Generate(f.Random.Int(6, 10)).ToList())
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));

            return new Faker<PlexTvShow>()
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.FullTitle, f => $"{f.Lorem.Word()}/{f.Lorem.Word()}/{f.Lorem.Word()}")
                .RuleFor(x => x.PlexLibraryId, _ => plexLibraryId)
                .RuleFor(x => x.PlexServerId, _ => plexServerId)
                .RuleFor(x => x.Key, _ => GetUniqueId(1, 10000, tvShowIds))
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