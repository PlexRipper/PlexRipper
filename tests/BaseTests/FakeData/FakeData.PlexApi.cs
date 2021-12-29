using System;
using System.Collections.Generic;
using Bogus;
using PlexRipper.Domain;

namespace PlexRipper.BaseTests
{
    public static partial class FakeData
    {
        private static readonly Random _random = new();

        public static Faker<PlexServer> GetPlexServer(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var uri = config.MockServerConfig?.ServerUri ?? new Uri("https://test-server.com");

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
                .RuleFor(x => x.PlexLibraries, _ => new List<PlexLibrary>());
        }

        public static Faker<PlexLibrary> GetPlexLibrary(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig
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
                .RuleFor(x => x.Movies, _ => new List<PlexMovie>())
                .RuleFor(x => x.TvShows, _ => new List<PlexTvShow>())
                .RuleFor(x => x.PlexAccountLibraries, _ => new List<PlexAccountLibrary>())
                .RuleFor(x => x.DownloadTasks, _ => new List<DownloadTask>());
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