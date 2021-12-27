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
        #region Base

        private static Faker<T> ApplyBasePlexMedia<T>(this Faker<T> faker, UnitTestDataConfig config = null) where T : PlexMedia
        {
            config ??= new UnitTestDataConfig();

            return faker
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.Id, _ => 0)
                .RuleFor(x => x.Key, f => f.Random.Int(1, 10000))
                .RuleFor(x => x.Title, f => f.Company.CompanyName())
                .RuleFor(x => x.FullTitle, f => f.Company.CompanyName())
                .RuleFor(x => x.SortTitle, f => f.Company.CompanyName())
                .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
                .RuleFor(x => x.Duration, f => f.Random.Int(1000, 3000000))
                .RuleFor(x => x.MediaSize, f => f.Random.Long(1000, 30000000))
                .RuleFor(x => x.MetaDataKey, f => f.Random.Int(1, 10000))
                .RuleFor(x => x.HasThumb, f => f.Random.Bool())
                .RuleFor(x => x.HasArt, f => f.Random.Bool())
                .RuleFor(x => x.HasBanner, f => f.Random.Bool())
                .RuleFor(x => x.HasTheme, f => f.Random.Bool())
                .RuleFor(x => x.Index, f => f.Random.Int(1, 10000))
                .RuleFor(x => x.Studio, f => f.Company.CompanyName())
                .RuleFor(x => x.Summary, f => f.Lorem.Sentences(2))
                .RuleFor(x => x.ContentRating, f => f.Lorem.Word())
                .RuleFor(x => x.Rating, f => f.Random.Double(0.1))
                .RuleFor(x => x.ChildCount, f => f.Random.Int(1, 10))
                .RuleFor(x => x.AddedAt, f => f.Date.Recent(30))
                .RuleFor(x => x.UpdatedAt, f => f.Date.Recent(30))
                .RuleFor(x => x.OriginallyAvailableAt, f => f.Date.Recent(30))
                .RuleFor(x => x.MediaData, _ => new PlexMediaContainer
                {
                    MediaData = GetPlexMediaData(config).Generate(1),
                })
                .RuleFor(x => x.PlexServerId, _ => 0)
                .RuleFor(x => x.PlexServer, _ => null)
                .RuleFor(x => x.PlexLibraryId, _ => 0)
                .RuleFor(x => x.PlexLibrary, _ => null);
        }

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

        #endregion

        #region PlexMovies

        public static Faker<PlexMovie> GetPlexMovies(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var movieIds = new List<int>();
            var movieKeys = new List<int>();

            return new Faker<PlexMovie>()
                .ApplyBasePlexMedia(config)
                .StrictMode(true)
                .UseSeed(config.Seed)
                .RuleFor(x => x.PlexMovieGenres, _ => new List<PlexMovieGenre>())
                .RuleFor(x => x.PlexMovieRoles, _ => new List<PlexMovieRole>())
                .FinishWith((_, movie) =>
                {
                    movie.FullTitle = $"{movie.Title} ({movie.Year})";

                    // TODO Need quality selector in the case of multiple quality media
                    movie.MediaSize = movie.MovieData.First().Parts.Sum(x => x.Size);
                });
        }

        #endregion

        #region PlexTvShows

        public static Faker<PlexTvShow> GetPlexTvShows(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var tvShowKeys = new List<int>();

            return new Faker<PlexTvShow>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .ApplyBasePlexMedia(config)
                .RuleFor(x => x.PlexTvShowGenres, _ => new List<PlexTvShowGenre>())
                .RuleFor(x => x.PlexTvShowRoles, _ => new List<PlexTvShowRole>())
                .RuleFor(x => x.Seasons, f => GetPlexTvShowSeason(config).GenerateBetween(1, config.TvShowSeasonCountMax))
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

        public static Faker<PlexTvShowSeason> GetPlexTvShowSeason(UnitTestDataConfig config = null)
        {
            config ??= new UnitTestDataConfig();

            var seasonKeys = new List<int>();
            return new Faker<PlexTvShowSeason>()
                .StrictMode(true)
                .UseSeed(config.Seed)
                .ApplyBasePlexMedia(config)
                .RuleFor(x => x.ParentKey, _ => GetUniqueId(seasonKeys, config))
                .RuleFor(x => x.TvShowId, _ => 0)
                .RuleFor(x => x.TvShow, _ => null)
                .RuleFor(x => x.Episodes, f => GetPlexTvShowEpisode(config).GenerateBetween(1, config.TvShowEpisodeCountMax))
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
                .StrictMode(true)
                .UseSeed(config.Seed)
                .ApplyBasePlexMedia(config)
                .RuleFor(x => x.Id, _ => 0)
                .RuleFor(x => x.ParentKey, _ => GetUniqueId(episodeKeys, config))
                .RuleFor(x => x.Key, f => f.Random.Int(1, 10000000))
                .RuleFor(x => x.TvShowId, _ => 0)
                .RuleFor(x => x.TvShow, _ => null)
                .RuleFor(x => x.TvShowSeasonId, _ => 0)
                .RuleFor(x => x.TvShowSeason, _ => null)
                .FinishWith((f, tvShowEpisode) =>
                {
                    tvShowEpisode.MediaSize = tvShowEpisode.EpisodeData.SelectMany(x => x.Parts.Select(y => y.Size)).Sum();
                });
        }

        #endregion
    }
}