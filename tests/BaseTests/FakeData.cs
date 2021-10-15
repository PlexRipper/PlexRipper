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

        private static int _plexTvShowId = 1;

        private static int _plexSeasonId = 1;

        private static int _plexEpisodeId = 1;

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
                .StrictMode(true)
                .RuleFor(x => x.Id, _ => plexLibraryId++)
                .RuleFor(x => x.Title, f => f.Company.CompanyName())
                .RuleFor(x => x.Type, _ => type)
                .RuleFor(x => x.PlexServerId, _ => serverId)
                .RuleFor(x => x.PlexServer, f => new PlexServer())
                .RuleFor(x => x.CreatedAt, f => f.Date.Past(4))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent())
                .RuleFor(x => x.ScannedAt, f => f.Date.Recent())
                .RuleFor(x => x.SyncedAt, f => f.Date.Recent())
                .RuleFor(x => x.Uuid, _ => Guid.NewGuid())
                .RuleFor(x => x.LibraryLocationId, f => f.Random.Int(1, 100))
                .RuleFor(x => x.LibraryLocationPath, f => f.System.DirectoryPath())
                .RuleFor(x => x.MetaData, _ => new PlexLibraryMetaData())
                .RuleFor(x => x.DefaultDestination, _ => new FolderPath())
                .RuleFor(x => x.DefaultDestinationId, f => f.Random.Int(1, 5))
                .RuleFor(x => x.Movies, _ => new List<PlexMovie>())
                .RuleFor(x => x.TvShows, _ => new List<PlexTvShow>())
                .RuleFor(x => x.PlexAccountLibraries, f => new List<PlexAccountLibrary>())
                .RuleFor(x => x.DownloadTasks, _ => new List<DownloadTask>());

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
            var tvShowKeys = new List<int>();

            return new Faker<PlexTvShow>()
                .RuleFor(x => x.Id, _ => _plexTvShowId++)
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.SortTitle, f => $"{f.Lorem.Word()}")
                .RuleFor(x => x.PlexLibraryId, _ => plexLibraryId)
                .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
                .RuleFor(x => x.PlexServerId, _ => plexServerId)
                .RuleFor(x => x.PlexTvShowGenres, _ => new List<PlexTvShowGenre>())
                .RuleFor(x => x.PlexTvShowRoles, _ => new List<PlexTvShowRole>())
                .RuleFor(x => x.Key, _ => GetUniqueId(1, 10000, tvShowKeys))
                .RuleFor(x => x.Seasons,
                    f => GetPlexTvShowSeason(plexServerId, plexLibraryId, _plexTvShowId, tvShowKeys.Last()).Generate(f.Random.Int(6, 10)).ToList())
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .FinishWith((f, tvShow) =>
                {
                    foreach (var tvShowSeason in tvShow.Seasons)
                    {
                        tvShowSeason.TvShow = tvShow;
                        tvShowSeason.TvShowId = tvShow.Id;
                        tvShowSeason.FullTitle = $"{tvShow.Title}/{tvShowSeason.Title}";

                        foreach (var episode in tvShowSeason.Episodes)
                        {
                            episode.TvShow = tvShow;
                            episode.TvShowId = tvShow.Id;
                            episode.FullTitle = $"{tvShow.Title}/{tvShowSeason.Title}/{episode.Title}";
                        }
                    }

                    tvShow.MediaSize = tvShow.Seasons.Select(x => x.MediaSize).Sum();
                });
        }

        public static Faker<PlexTvShowSeason> GetPlexTvShowSeason(int plexServerId, int plexLibraryId, int plexTvShowId, int parentKey)
        {
            var seasonIndex = 1;
            var seasonKeys = new List<int>();
            return new Faker<PlexTvShowSeason>()
                .RuleFor(x => x.Id, _ => _plexSeasonId++)
                .RuleFor(x => x.ParentKey, _ => parentKey)
                .RuleFor(x => x.Title, _ => $"Season {seasonIndex++}")
                .RuleFor(x => x.Key, _ => GetUniqueId(1, 10000, seasonKeys))
                .RuleFor(x => x.PlexLibraryId, _ => plexLibraryId)
                .RuleFor(x => x.PlexServerId, _ => plexServerId)
                .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
                .RuleFor(x => x.TvShowId, _ => plexTvShowId)
                .RuleFor(x => x.Episodes,
                    f => GetPlexTvShowEpisode(
                            plexServerId,
                            plexLibraryId,
                            plexTvShowId,
                            _plexSeasonId,
                            seasonKeys.Last())
                        .Generate(f.Random.Int(6, 10)).ToList())
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .FinishWith((f, tvShowSeason) =>
                {
                    foreach (var episode in tvShowSeason.Episodes)
                    {
                        episode.TvShowSeason = tvShowSeason;
                        episode.TvShowSeasonId = tvShowSeason.Id;
                        episode.FullTitle = $"{tvShowSeason.Title}/{episode.Title}";
                    }

                    tvShowSeason.MediaSize = tvShowSeason.Episodes.Select(x => x.MediaSize).Sum();
                });
        }

        public static Faker<PlexTvShowEpisode> GetPlexTvShowEpisode(int plexServerId, int plexLibraryId, int plexTvShowId, int plexTvShowSeasonId,
            int parentKey)
        {
            var episodeKeys = new List<int>();
            return new Faker<PlexTvShowEpisode>()
                .RuleFor(x => x.Id, _ => _plexEpisodeId++)
                .RuleFor(x => x.ParentKey, _ => parentKey)
                .RuleFor(x => x.Key, _ => GetUniqueId(1, 10000, episodeKeys))
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.PlexLibraryId, _ => plexLibraryId)
                .RuleFor(x => x.PlexServerId, _ => plexServerId)
                .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
                .RuleFor(x => x.TvShowId, _ => plexTvShowId)
                .RuleFor(x => x.TvShowSeasonId, _ => plexTvShowSeasonId)
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.MediaData, _ => new PlexMediaContainer
                {
                    MediaData = GetPlexMediaData(1).Generate(1),
                })
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .FinishWith((f, tvShowEpisode) =>
                {
                    tvShowEpisode.MediaSize = tvShowEpisode.EpisodeData.SelectMany(x => x.Parts.Select(y => y.Size)).Sum();
                });
        }

        public static Faker<FolderPath> GetFolderPaths()
        {
            var ids = 0;
            return new Faker<FolderPath>()
                .StrictMode(true)
                .RuleFor(x => x.Id, _ => ids++)
                .RuleFor(x => x.DisplayName, f => f.Random.Word())
                .RuleFor(x => x.FolderType, f => f.Random.Enum<FolderType>())
                .RuleFor(x => x.MediaType, f => f.Random.Enum<PlexMediaType>())
                .RuleFor(x => x.DirectoryPath, f => f.System.DirectoryPath())
                .RuleFor(x => x.PlexLibraries, _ => new List<PlexLibrary>());
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