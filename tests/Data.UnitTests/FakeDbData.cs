using System;
using System.Linq;
using Bogus;
using PlexRipper.Domain;

namespace Data.UnitTests
{
    public static class FakeDbData
    {
        public static Faker<PlexServer> GetPlexServer(bool includeLibraries = false)
        {
            var rnd = new Random();
            int serverId = 1;

            var plexServer = new Faker<PlexServer>()
                .UseSeed(rnd.Next(1, 100))
                .RuleFor(x => x.Id, f => serverId)
                .RuleFor(x => x.Name, f => f.Company.CompanyName())
                .RuleFor(x => x.Address, f => f.Internet.Ip())
                .RuleFor(x => x.Scheme, f => "http")
                .RuleFor(x => x.Port, f => f.Random.Int(1000, 65000))
                .RuleFor(x => x.Host, f => f.Internet.Ip())
                .RuleFor(x => x.CreatedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));

            if (includeLibraries)
            {
                int plexLibraryId = 1;
                var libraryTypes = new[] { PlexMediaType.Movie, PlexMediaType.TvShow };
                var plexLibraries = new Faker<PlexLibrary>()
                    .RuleFor(x => x.Id, f => plexLibraryId++)
                    .RuleFor(x => x.Title, f => f.Company.CompanyName())
                    .RuleFor(x => x.PlexServerId, f => serverId)
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

        public static Faker<PlexMovie> GetPlexMovies(int plexLibraryId)
        {
            var mediaContainer = new[] { "mkv", "mp4" };

            var plexMovieDataPart = new Faker<PlexMovieDataPart>()
                .RuleFor(x => x.Container, f => f.PickRandom(mediaContainer))
                .RuleFor(x => x.File, f => f.System.FileName())
                .RuleFor(x => x.ObfuscatedFilePath, f => f.Random.Int(1, 500).ToString());

            var plexMovieData = new Faker<PlexMovieData>()
                .RuleFor(x => x.MediaFormat, f => f.PickRandom(mediaContainer))
                .RuleFor(x => x.Width, f => f.Random.Int(0, 10) * 100)
                .RuleFor(x => x.Height, f => f.Random.Int(0, 10) * 100)
                .RuleFor(x => x.Parts, f => plexMovieDataPart.Generate(f.Random.Int(1, 3)).ToList());

            return new Faker<PlexMovie>()
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.PlexLibraryId, f => plexLibraryId)
                .RuleFor(x => x.RatingKey, f => f.Random.Int(1, 100000))
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .RuleFor(x => x.PlexMovieDatas, f => plexMovieData.Generate(2).ToList());
        }

        public static Faker<PlexTvShow> GetPlexTvShows(int plexLibraryId)
        {
            var mediaContainer = new[] { "mkv", "mp4" };

            var episodeDataPart = new Faker<PlexTvShowEpisodeDataPart>()
                .RuleFor(x => x.Container, f => f.PickRandom(mediaContainer))
                .RuleFor(x => x.File, f => f.System.FileName())
                .RuleFor(x => x.ObfuscatedFilePath, f => f.Random.Int(1, 500).ToString());

            var episodeData = new Faker<PlexTvShowEpisodeData>()
                .RuleFor(x => x.MediaFormat, f => f.PickRandom(mediaContainer))
                .RuleFor(x => x.Width, f => f.Random.Int(0, 10) * 100)
                .RuleFor(x => x.Height, f => f.Random.Int(0, 10) * 100)
                .RuleFor(x => x.Parts, f => episodeDataPart.Generate(f.Random.Int(1, 3)).ToList());

            var episodes = new Faker<PlexTvShowEpisode>()
                .RuleFor(x => x.RatingKey, f => f.Random.Int(1, 10000))
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.PlexLibraryId, f => plexLibraryId)
                .RuleFor(x => x.EpisodeData, f => episodeData.Generate(f.Random.Int(1, 3)).ToList())
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));

            var seasonIndex = 1;
            var seasons = new Faker<PlexTvShowSeason>()
                .RuleFor(x => x.Title, f => $"Season {seasonIndex++}")
                .RuleFor(x => x.RatingKey, f => f.Random.Int(1, 1000))
                .RuleFor(x => x.PlexLibraryId, f => plexLibraryId)
                .RuleFor(x => x.Episodes, f => episodes.Generate(f.Random.Int(5, 25)).ToList())
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));

            return new Faker<PlexTvShow>()
                .RuleFor(x => x.Title, f => f.Lorem.Word())
                .RuleFor(x => x.PlexLibraryId, f => plexLibraryId)
                .RuleFor(x => x.RatingKey, f => f.Random.Int(1, 100000))
                .RuleFor(x => x.Seasons, f => seasons.Generate(f.Random.Int(1, 10)).ToList())
                .RuleFor(x => x.AddedAt, f => f.Date.Past(10, DateTime.Now))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30));
        }
    }
}