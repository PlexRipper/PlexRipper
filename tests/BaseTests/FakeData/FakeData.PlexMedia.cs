using Bogus;
using PlexApi.Contracts;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    #region Base

    private static Faker<T> ApplyBasePlexMedia<T>(
        this Faker<T> faker,
        Seed seed,
        PlexMediaType mediaType,
        Action<FakeDataConfig>? options = null
    )
        where T : PlexMedia
    {
        return faker
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.Key, _ => GetUniqueNumber())
            .RuleFor(x => x.Title, f => f.PlexMedia().MediaTitle(mediaType))
            .RuleFor(x => x.Year, f => f.Random.Int(1900, 2030))
            .RuleFor(x => x.FullTitle, (_, x) => $"{x.Title} ({x.Year})")
            .RuleFor(x => x.SortIndex, _ => 0)
            .RuleFor(x => x.SearchTitle, (_, x) => x.Title.ToSearchTitle())
            .RuleFor(x => x.Duration, f => f.Random.Int(1000, 3000000))
            .RuleFor(x => x.MediaSize, f => f.Random.Long(1000, 30000000))
            .RuleFor(x => x.MetaDataKey, f => f.Random.Int(1, 10000))
            .RuleFor(x => x.HasThumb, f => f.Random.Bool())
            .RuleFor(x => x.HasArt, f => f.Random.Bool())
            .RuleFor(x => x.HasTheme, f => f.Random.Bool())
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
            .RuleFor(x => x.FullBannerUrl, _ => string.Empty)
            .RuleFor(
                x => x.Guid,
                _ => $"plex://{mediaType.ToPlexApiString()}/${Guid.NewGuid().ToString().Replace("-", "")}"
            )
            .RuleFor(x => x.Guid_IMDB, f => "imdb://tt" + f.Random.Int(10000, 99999))
            .RuleFor(x => x.Guid_TMDB, f => "tmdb://" + f.Random.Int(10000, 99999))
            .RuleFor(x => x.Guid_TVDB, f => "tvdb://" + f.Random.Int(10000, 99999))
            .RuleFor(x => x.MediaData, _ => new MediaDataContainer(GetPlexMediaData(seed, options).Generate(1)));
    }

    public static Faker<LibraryMediaItemMediaDTO> GetPlexMediaData(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<LibraryMediaItemMediaDTO>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.Id, 0)
            .RuleFor(x => x.Bitrate, f => f.Random.Int(1900, 2030))
            .RuleFor(x => x.Width, f => f.Random.Int(240, 10000))
            .RuleFor(x => x.Height, f => f.Random.Int(240, 10000))
            .RuleFor(x => x.VideoFrameRate, _ => "24p")
            .RuleFor(x => x.VideoProfile, _ => "high")
            .RuleFor(x => x.AudioCodec, _ => "dca")
            .RuleFor(x => x.AudioProfile, _ => "dts")
            .RuleFor(x => x.AspectRatio, f => f.Random.Float(1, 2))
            .RuleFor(x => x.VideoCodec, f => f.System.FileType())
            .RuleFor(x => x.AudioChannels, f => f.Random.Int(2, 5))
            .RuleFor(x => x.VideoResolution, f => f.PickRandom("sd", "720p", "1080p"))
            .RuleFor(x => x.Duration, f => f.Random.Int(50000, 55124400))
            .RuleFor(x => x.Container, f => f.System.FileType())
            .RuleFor(x => x.HasVoiceActivity, f => f.Random.Bool())
            .RuleFor(
                x => x.Parts,
                _ => GetPlexMediaPart(seed, options).Generate(config.IncludeMultiPartMovies ? 2 : 1)
            );
    }

    public static Faker<LibraryMediaItemPartDTO> GetPlexMediaPart(Seed seed, Action<FakeDataConfig>? options = null)
    {
        return new Faker<LibraryMediaItemPartDTO>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.Exists, true)
            .RuleFor(x => x.Accessible, f => f.Random.Bool())
            .RuleFor(x => x.Key, _ => DownloadFileUrl)
            .RuleFor(x => x.Duration, f => f.Random.Int(50000, 5512400))
            .RuleFor(x => x.AudioProfile, _ => "dts")
            .RuleFor(x => x.File, _ => "/file.mp4")
            .RuleFor(x => x.Size, _ => 50 * 1024)
            .RuleFor(x => x.Container, f => f.System.FileExt("video/mp4"))
            .RuleFor(x => x.VideoProfile, f => f.Random.Words(2))
            .RuleFor(x => x.Stream, _ => [])
            .RuleFor(x => x.Indexes, f => f.Random.Word());
    }

    #endregion

    #region PlexMovies

    public static Faker<PlexMovie> GetPlexMovies(Seed seed, Action<FakeDataConfig>? options = null)
    {
        return new Faker<PlexMovie>()
            .ApplyBasePlexMedia(seed, PlexMediaType.Movie, options)
            .StrictMode(true)
            .UseSeed(seed.Next())
            .RuleFor(x => x.Roles, () => [])
            .RuleFor(x => x.Genres, () => [])
            .RuleFor(x => x.Countries, () => [])
            .FinishWith(
                (_, movie) =>
                {
                    movie.FullTitle = $"{movie.Title} ({movie.Year})";

                    // TODO:Need quality selector in the case of multiple quality media
                    movie.MediaSize = movie.MetaDataList.First().Parts.Sum(x => x.Size);
                }
            );
    }

    #endregion

    #region PlexTvShows

    public static Faker<PlexTvShow> GetPlexTvShows(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<PlexTvShow>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .ApplyBasePlexMedia(seed, PlexMediaType.TvShow, options)
            .RuleFor(x => x.Roles, () => [])
            .RuleFor(x => x.Genres, () => [])
            .RuleFor(x => x.Countries, () => [])
            .RuleFor(x => x.Seasons, _ => GetPlexTvShowSeason(seed, options).Generate(config.TvShowSeasonCount))
            .RuleFor(x => x.ChildCount, _ => config.TvShowSeasonCount)
            .RuleFor(x => x.GrandChildCount, _ => config.TvShowSeasonCount * config.TvShowEpisodeCount)
            .FinishWith(
                (_, tvShow) =>
                {
                    foreach (var (season, seasonIndex) in tvShow.Seasons.Select((season, index) => (season, index)))
                    {
                        season.Title = $"{tvShow.Title} {seasonIndex + 1:D2}";
                        season.ParentKey = tvShow.Key;
                        season.ParentGuid = tvShow.Guid;
                        season.FullTitle = $"{tvShow.Title}/{season.Title}";

                        foreach (
                            var (episode, episodeIndex) in season.Episodes.Select((episode, index) => (episode, index))
                        )
                        {
                            var originalTitle = episode.Title;
                            episode.Title = $"S{seasonIndex + 1:D2}E{episodeIndex + 1:D2} - {originalTitle}";
                            episode.ParentKey = season.Key;
                            episode.ParentGuid = season.Guid;
                            episode.FullTitle = $"{tvShow.Title}/{season.Title}/{episode.Title}";
                        }
                    }

                    tvShow.MediaSize = tvShow.Seasons.Sum(season => season.MediaSize);
                }
            );
    }

    public static Faker<PlexTvShowSeason> GetPlexTvShowSeason(Seed seed, Action<FakeDataConfig>? options = null)
    {
        var config = FakeDataConfig.FromOptions(options);

        return new Faker<PlexTvShowSeason>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .ApplyBasePlexMedia(seed, PlexMediaType.Season, options)
            .RuleFor(x => x.Title, _ => "Season")
            .RuleFor(x => x.ParentKey, _ => GetUniqueNumber())
            .RuleFor(x => x.TvShowId, _ => 0)
            .RuleFor(x => x.TvShow, _ => null)
            .RuleFor(x => x.ParentGuid, _ => string.Empty)
            .RuleFor(x => x.Episodes, _ => GetPlexTvShowEpisode(seed, options).Generate(config.TvShowEpisodeCount))
            .FinishWith(
                (_, tvShowSeason) =>
                {
                    tvShowSeason.MediaSize = tvShowSeason.Episodes.Select(x => x.MediaSize).Sum();
                    foreach (var episode in tvShowSeason.Episodes)
                    {
                        episode.ParentGuid = tvShowSeason.Guid;
                    }
                }
            );
    }

    public static Faker<PlexTvShowEpisode> GetPlexTvShowEpisode(Seed seed, Action<FakeDataConfig>? options = null)
    {
        return new Faker<PlexTvShowEpisode>()
            .StrictMode(true)
            .UseSeed(seed.Next())
            .ApplyBasePlexMedia(seed, PlexMediaType.Episode, options)
            .RuleFor(x => x.Id, _ => 0)
            .RuleFor(x => x.ParentKey, _ => GetUniqueNumber())
            .RuleFor(x => x.TvShowId, _ => 0)
            .RuleFor(x => x.TvShow, _ => null)
            .RuleFor(x => x.TvShowSeasonId, _ => 0)
            .RuleFor(x => x.TvShowSeason, _ => null)
            .RuleFor(x => x.ParentGuid, _ => string.Empty)
            .FinishWith(
                (_, tvShowEpisode) =>
                {
                    foreach (var mediaData in tvShowEpisode.MetaDataList)
                    foreach (var mediaDataPart in mediaData.Parts)
                        mediaDataPart.File = $"{tvShowEpisode.Title}";

                    tvShowEpisode.MediaSize = tvShowEpisode
                        .MetaDataList.SelectMany(x => x.Parts.Select(y => y.Size))
                        .Sum();
                }
            );
    }

    #endregion
}
