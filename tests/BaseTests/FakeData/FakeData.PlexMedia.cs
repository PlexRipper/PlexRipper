using Bogus;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    #region Base

    private static Faker<T> ApplyBasePlexMedia<T>(
        this Faker<T> faker,
        int seed = 0,
        Action<FakeDataConfig>? options = null
    )
        where T : PlexMedia
    {
        var title = new Faker().Company.CompanyName();

        return faker
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.Key, _ => GetUniqueNumber())
            .RuleFor(x => x.Title, _ => title)
            .RuleFor(x => x.FullTitle, _ => title)
            .RuleFor(x => x.SortTitle, _ => title.ToSortTitle())
            .RuleFor(x => x.SearchTitle, _ => title.ToSearchTitle())
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
            .RuleFor(x => x.PlexServerId, _ => 0)
            .RuleFor(x => x.PlexServer, _ => null)
            .RuleFor(x => x.PlexLibraryId, _ => 0)
            .RuleFor(x => x.PlexLibrary, _ => null)
            .RuleFor(x => x.FullThumbUrl, _ => string.Empty)
            .RuleFor(x => x.FullBannerUrl, _ => string.Empty)
            .RuleFor(x => x.Guid, _ => string.Empty)
            .RuleFor(
                x => x.MediaData,
                _ => new PlexMediaContainer { MediaData = GetPlexMediaData(seed, options).Generate(1) }
            );
    }

    public static Faker<PlexMediaData> GetPlexMediaData(int seed = 0, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<PlexMediaData>()
            .StrictMode(true)
            .UseSeed(seed)
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
            .RuleFor(
                x => x.Parts,
                _ => GetPlexMediaPart(seed, options).GenerateBetween(1, config.IncludeMultiPartMovies ? 2 : 1)
            );
    }

    public static Faker<PlexMediaDataPart> GetPlexMediaPart(int seed = 0, Action<FakeDataConfig>? options = null)
    {
        return new Faker<PlexMediaDataPart>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.ObfuscatedFilePath, _ => PlexMockServerConfig.FileUrl)
            .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
            .RuleFor(x => x.AudioProfile, _ => "dts")
            .RuleFor(x => x.HasThumbnail, f => f.Random.Int(0, 1).ToString())
            .RuleFor(x => x.HasChapterTextStream, f => f.Random.Bool())
            .RuleFor(x => x.File, _ => "/fake_download.mp4")
            .RuleFor(x => x.Size, _ => 50 * 1024)
            .RuleFor(x => x.Container, f => f.System.FileExt("video/mp4"))
            .RuleFor(x => x.VideoProfile, f => f.Random.Words(2))
            .RuleFor(x => x.Indexes, f => f.Random.Word());
    }

    #endregion

    #region PlexMovies

    public static Faker<PlexMovie> GetPlexMovies(int seed = 0, Action<FakeDataConfig>? options = null)
    {
        var movieIds = new List<int>();
        var movieKeys = new List<int>();

        return new Faker<PlexMovie>()
            .ApplyBasePlexMedia(seed, options)
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(x => x.PlexMovieGenres, _ => new List<PlexMovieGenre>())
            .RuleFor(x => x.PlexMovieRoles, _ => new List<PlexMovieRole>())
            .FinishWith(
                (_, movie) =>
                {
                    movie.FullTitle = $"{movie.Title} ({movie.Year})";

                    // TODO Need quality selector in the case of multiple quality media
                    movie.MediaSize = movie.MovieData.First().Parts.Sum(x => x.Size);
                }
            );
    }

    #endregion

    #region PlexTvShows

    public static Faker<PlexTvShow> GetPlexTvShows(int seed = 0, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<PlexTvShow>()
            .StrictMode(true)
            .UseSeed(seed)
            .ApplyBasePlexMedia(seed, options)
            .RuleFor(x => x.PlexTvShowGenres, _ => new List<PlexTvShowGenre>())
            .RuleFor(x => x.PlexTvShowRoles, _ => new List<PlexTvShowRole>())
            .RuleFor(x => x.Seasons, _ => GetPlexTvShowSeason(seed, options).Generate(config.TvShowSeasonCount))
            .FinishWith(
                (_, tvShow) =>
                {
                    for (var seasonIndex = 0; seasonIndex < tvShow.Seasons.Count; seasonIndex++)
                    {
                        tvShow.Seasons[seasonIndex].Title = $"{tvShow.Title} {seasonIndex + 1:D2}";
                        tvShow.Seasons[seasonIndex].ParentKey = tvShow.Key;
                        tvShow.Seasons[seasonIndex].FullTitle = $"{tvShow.Title}/{tvShow.Seasons[seasonIndex].Title}";

                        for (
                            var episodeIndex = 0;
                            episodeIndex < tvShow.Seasons[seasonIndex].Episodes.Count;
                            episodeIndex++
                        )
                        {
                            var title = tvShow.Seasons[seasonIndex].Episodes[episodeIndex].Title;
                            tvShow.Seasons[seasonIndex].Episodes[episodeIndex].Title =
                                $"S{seasonIndex + 1:D2}E{episodeIndex + 1:D2} - {title}";
                            tvShow.Seasons[seasonIndex].Episodes[episodeIndex].ParentKey = tvShow
                                .Seasons[seasonIndex]
                                .Key;
                            tvShow.Seasons[seasonIndex].Episodes[episodeIndex].FullTitle =
                                $"{tvShow.Title}/{tvShow.Seasons[seasonIndex].Title}/{tvShow.Seasons[seasonIndex].Episodes[episodeIndex].Title}";
                        }
                    }

                    tvShow.MediaSize = tvShow.Seasons.Select(x => x.MediaSize).Sum();
                }
            );
    }

    public static Faker<PlexTvShowSeason> GetPlexTvShowSeason(int seed = 0, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<PlexTvShowSeason>()
            .StrictMode(true)
            .UseSeed(seed)
            .ApplyBasePlexMedia(seed, options)
            .RuleFor(x => x.Title, _ => "Season")
            .RuleFor(x => x.ParentKey, _ => GetUniqueNumber())
            .RuleFor(x => x.TvShowId, _ => 0)
            .RuleFor(x => x.TvShow, _ => null)
            .RuleFor(x => x.Episodes, _ => GetPlexTvShowEpisode(seed, options).Generate(config.TvShowEpisodeCount))
            .FinishWith(
                (_, tvShowSeason) =>
                {
                    tvShowSeason.MediaSize = tvShowSeason.Episodes.Select(x => x.MediaSize).Sum();
                }
            );
    }

    public static Faker<PlexTvShowEpisode> GetPlexTvShowEpisode(int seed = 0, Action<FakeDataConfig>? options = null)
    {
        return new Faker<PlexTvShowEpisode>()
            .StrictMode(true)
            .UseSeed(seed)
            .ApplyBasePlexMedia(seed, options)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.ParentKey, _ => GetUniqueNumber())
            .RuleFor(x => x.TvShowId, _ => 0)
            .RuleFor(x => x.TvShow, _ => null)
            .RuleFor(x => x.TvShowSeasonId, _ => 0)
            .RuleFor(x => x.TvShowSeason, _ => null)
            .FinishWith(
                (_, tvShowEpisode) =>
                {
                    foreach (var mediaData in tvShowEpisode.EpisodeData)
                    foreach (var mediaDataPart in mediaData.Parts)
                        mediaDataPart.File = $"{tvShowEpisode.Title}";

                    tvShowEpisode.MediaSize = tvShowEpisode
                        .EpisodeData.SelectMany(x => x.Parts.Select(y => y.Size))
                        .Sum();
                }
            );
    }

    #endregion
}
