using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using Bogus.Extensions;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public static partial class FakeData
    {
        private static readonly Random _random = new();

        public static Faker<PlexServer> GetPlexServer(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var uri = new Uri(config.ServerUrl);

            return new Faker<PlexServer>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(p => p.Id, _ => 0)
                .RuleFor(x => x.Name, f => f.Company.CompanyName())
                .RuleFor(x => x.Address, uri.Host)
                .RuleFor(x => x.Scheme, uri.Scheme)
                .RuleFor(x => x.Port, uri.Port)
                .RuleFor(x => x.Host, uri.Host)
                .RuleFor(x => x.CreatedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .RuleFor(x => x.Version, _ => "1.24.3.5033-757abe6b4")
                .RuleFor(x => x.LocalAddresses, f => f.Internet.Ip())
                .RuleFor(x => x.MachineIdentifier, _ => Guid.NewGuid().ToString())
                .RuleFor(x => x.OwnerId, f => f.Random.Int(1000, 100000))
                .RuleFor(x => x.ServerFixApplyDNSFix, f => f.Random.Bool())
                .RuleFor(x => x.PlexAccountServers, _ => new List<PlexAccountServer>())
                .RuleFor(x => x.ServerStatus, _ => new List<PlexServerStatus>())
                .RuleFor(x => x.AccessToken, _ => "DO NOT USE")
                .RuleFor(x => x.PlexLibraries, _ =>  new List<PlexLibrary>());
        }

        public static Faker<PlexLibrary> GetPlexLibrary(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig()
            {
                LibraryType = PlexMediaType.Movie,
            };

            return new Faker<PlexLibrary>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.Id, _ => 0)
                .RuleFor(x => x.Key, f => f.Random.Int(1, 10000).ToString())
                .RuleFor(x => x.Title, f => f.Company.CompanyName())
                .RuleFor(x => x.Type, _ => config.LibraryType)
                .RuleFor(x => x.PlexServerId, f => f.Random.Int(1, 10000))
                .RuleFor(x => x.PlexServer, _ => new PlexServer())
                .RuleFor(x => x.CreatedAt, f => f.Date.Past(4))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent())
                .RuleFor(x => x.ScannedAt, f => f.Date.Recent())
                .RuleFor(x => x.SyncedAt, f => f.Date.Recent())
                .RuleFor(x => x.Uuid, _ => Guid.NewGuid())
                .RuleFor(x => x.LibraryLocationId, f => f.Random.Int(1, 10000))
                .RuleFor(x => x.LibraryLocationPath, f => f.System.DirectoryPath())
                .RuleFor(x => x.MetaData, _ => new PlexLibraryMetaData())
                .RuleFor(x => x.DefaultDestination, _ => new FolderPath())
                .RuleFor(x => x.DefaultDestinationId, f => f.Random.Int(1, 5))
                .RuleFor(x => x.Movies, _ =>  new List<PlexMovie>())
                .RuleFor(x => x.TvShows, _ =>  new List<PlexTvShow>())
                .RuleFor(x => x.PlexAccountLibraries, _ => new List<PlexAccountLibrary>())
                .RuleFor(x => x.DownloadTasks, _ => new List<DownloadTask>());
        }

        #region PlexMovies

        public static Faker<PlexMovie> GetPlexMovies(UnitTestDataConfig config = null, int movieQualities = 1, int movieParts = 1)
        {
            config ??= new UnitTestDataConfig();

            var movieIds = new List<int>();
            var movieKeys = new List<int>();

            return new Faker<PlexMovie>()
                .UseSeed(config.Seed)
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.MediaData, _ => new PlexMediaContainer
                {
                    MediaData = GetPlexMediaData(config).Generate(movieQualities),
                })
                .RuleFor(x => x.PlexServerId, _ => 0)
                .RuleFor(x => x.PlexServer, _ => null)
                .RuleFor(x => x.PlexLibraryId, _ => 0)
                .RuleFor(x => x.PlexLibrary, _ => null)
                .RuleFor(x => x.Key, _ => GetUniqueId(movieKeys, config))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .FinishWith((_, movie) =>
                {
                    movie.FullTitle = $"{movie.Title} ({movie.Year})";
                    // TODO Need quality selector in the case of multiple quality media
                    movie.MediaSize = movie.MovieData.First().Parts.Sum(x => x.Size);
                });
        }

        #endregion

        public static Faker<PlexMediaData> GetPlexMediaData(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<PlexMediaData>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.Bitrate, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.MediaFormat, f => f.System.FileExt("video/mp4"))
                .RuleFor(x => x.Width, f => f.Random.Int(240, 10000))
                .RuleFor(x => x.Height, f => f.Random.Int(240, 10000))
                .RuleFor(x => x.VideoFrameRate, _ => "24p")
                .RuleFor(x => x.VideoProfile, _ => "high")
                .RuleFor(x => x.AudioCodec, _ => "dca")
                .RuleFor(x => x.AudioProfile, _ => "dts")
                .RuleFor(x => x.Protocol, _ => "unknown")
                .RuleFor(x => x.Selected, f => f.Random.Bool())
                .RuleFor(x => x.AspectRatio, _ => 1.78)
                .RuleFor(x => x.VideoCodec, f => f.System.FileType())
                .RuleFor(x => x.AudioChannels, f => f.Random.Int(2, 5))
                .RuleFor(x => x.VideoResolution, f => f.PickRandom("sd", "720p", "1080p"))
                .RuleFor(x => x.Duration, f => f.Random.Long(50000, 55124400))
                .RuleFor(x => x.OptimizedForStreaming, f => f.Random.Bool())
                .RuleFor(x => x.Parts, f => GetPlexMediaPart().GenerateBetween(1, config.IncludeMultiPartMovies ? 2 : 1));
        }

        public static Faker<PlexMediaDataPart> GetPlexMediaPart(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            return new Faker<PlexMediaDataPart>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.ObfuscatedFilePath, f => "/library/parts/65125/1193813456/file.avi")
                .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
                .RuleFor(x => x.AudioProfile, _ => "dts")
                .RuleFor(x => x.HasThumbnail, f => f.Random.Int(0, 1).ToString())
                .RuleFor(x => x.HasChapterTextStream, f => f.Random.Bool())
                .RuleFor(x => x.File, f => "/KidsMovies/Fantastic Four 2/F4 Rise of the Silver Surfer.avi")
                .RuleFor(x => x.Size, f => f.Random.Long(50000, 55124400))
                .RuleFor(x => x.Container, f => f.System.FileExt("video/mp4"))
                .RuleFor(x => x.VideoProfile, f => f.Random.Words(2))
                .RuleFor(x => x.Indexes, f => f.Random.Word());
        }

        #region PlexTvShows

        public static Faker<PlexTvShow> GetPlexTvShows(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var tvShowKeys = new List<int>();

            return new Faker<PlexTvShow>()
                .UseSeed(config.Seed)
                .RuleFor(x => x.Id, _ => 0)
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.SortTitle, f => $"{f.Lorem.Word()}")
                .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
                .RuleFor(x => x.PlexServerId, _ => 0)
                .RuleFor(x => x.PlexServer, _ => null)
                .RuleFor(x => x.PlexLibraryId, _ => 0)
                .RuleFor(x => x.PlexLibrary, _ => null)
                .RuleFor(x => x.PlexTvShowGenres, _ => new List<PlexTvShowGenre>())
                .RuleFor(x => x.PlexTvShowRoles, _ => new List<PlexTvShowRole>())
                .RuleFor(x => x.Key, _ => GetUniqueId(tvShowKeys, config))
                .RuleFor(x => x.Seasons, f => GetPlexTvShowSeason(config).GenerateBetween(1, config.TvShowSeasonCountMax))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .FinishWith((_, tvShow) =>
                {
                    foreach (var tvShowSeason in tvShow.Seasons)
                    {
                        tvShowSeason.ParentKey = tvShow.Key;
                        tvShowSeason.FullTitle = $"{tvShow.Title}/{tvShowSeason.Title}";

                        foreach (var episode in tvShowSeason.Episodes)
                        {
                            episode.FullTitle = $"{tvShow.Title}/{tvShowSeason.Title}/{episode.Title}";
                        }
                    }

                    tvShow.MediaSize = tvShow.Seasons.Select(x => x.MediaSize).Sum();
                });
        }

        #endregion

        public static Faker<PlexTvShowSeason> GetPlexTvShowSeason(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var seasonIndex = 1;
            var seasonKeys = new List<int>();
            return new Faker<PlexTvShowSeason>()
                .UseSeed(config.Seed)
                .RuleFor(x => x.Id, _ => 0)
                .RuleFor(x => x.ParentKey, _ => GetUniqueId(seasonKeys, config))
                .RuleFor(x => x.Key, f => f.Random.Int(1, 10000000))
                .RuleFor(x => x.Title, _ => $"Season {seasonIndex++}")
                .RuleFor(x => x.PlexServerId, _ => 0)
                .RuleFor(x => x.PlexServer, _ => null)
                .RuleFor(x => x.PlexLibraryId, _ => 0)
                .RuleFor(x => x.PlexLibrary, _ => null)
                .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
                .RuleFor(x => x.TvShowId, _ => 0)
                .RuleFor(x => x.Episodes, f => GetPlexTvShowEpisode(config).GenerateBetween(1, config.TvShowEpisodeCountMax))
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .FinishWith((f, tvShowSeason) =>
                {
                    foreach (var episode in tvShowSeason.Episodes)
                    {
                        episode.FullTitle = $"{tvShowSeason.Title}/{episode.Title}";
                    }

                    tvShowSeason.MediaSize = tvShowSeason.Episodes.Select(x => x.MediaSize).Sum();
                });
        }

        public static Faker<PlexTvShowEpisode> GetPlexTvShowEpisode(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var episodeKeys = new List<int>();
            return new Faker<PlexTvShowEpisode>()
                .UseSeed(config.Seed)
                .RuleFor(x => x.Id, _ => 0)
                .RuleFor(x => x.ParentKey, _ => GetUniqueId(episodeKeys, config))
                .RuleFor(x => x.Key, f => f.Random.Int(1, 10000000))
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.PlexServerId, _ => 0)
                .RuleFor(x => x.PlexServer, _ => null)
                .RuleFor(x => x.PlexLibraryId, _ => 0)
                .RuleFor(x => x.PlexLibrary, _ => null)
                .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
                .RuleFor(x => x.TvShowId, _ => 0)
                .RuleFor(x => x.TvShow, _ => null)
                .RuleFor(x => x.TvShowSeasonId, _ => 0)
                .RuleFor(x => x.TvShowSeason, _ => null)
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.MediaData, _ => new PlexMediaContainer
                {
                    MediaData = GetPlexMediaData().Generate(1),
                })
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .FinishWith((f, tvShowEpisode) =>
                {
                    tvShowEpisode.MediaSize = tvShowEpisode.EpisodeData.SelectMany(x => x.Parts.Select(y => y.Size)).Sum();
                });
        }

        public static Faker<FolderPath> GetFolderPaths(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var ids = 0;
            return new Faker<FolderPath>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.Id, _ => ids++)
                .RuleFor(x => x.DisplayName, f => f.Random.Word())
                .RuleFor(x => x.FolderType, f => f.Random.Enum<FolderType>())
                .RuleFor(x => x.MediaType, f => f.Random.Enum<PlexMediaType>())
                .RuleFor(x => x.DirectoryPath, f => f.System.DirectoryPath())
                .RuleFor(x => x.PlexLibraries, _ => new List<PlexLibrary>());
        }

        private static int GetUniqueId(List<int> alreadyGenerated, UnitTestDataConfig config = null)
        {
            var rnd = new Random(config.Seed);
            while (true)
            {
                int value = rnd.Next(1, 10000000);
                if (!alreadyGenerated.Contains(value))
                {
                    alreadyGenerated.Add(value);
                    return value;
                }
            }
        }
    }
}