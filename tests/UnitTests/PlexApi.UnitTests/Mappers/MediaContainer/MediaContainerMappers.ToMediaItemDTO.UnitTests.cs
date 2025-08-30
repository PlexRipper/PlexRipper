using LukeHagar.PlexAPI.SDK.Models.Requests;
using NodaTime;
using Reaparr.BaseTests;

namespace Reaparr.PlexApi.UnitTests
{
    public class MediaContainerMappersUnitTests : BaseUnitTest
    {
        public MediaContainerMappersUnitTests(ITestOutputHelper output)
            : base(output) { }

        [Fact]
        public void ShouldMapAllPropertiesCorrectly_WhenApiResponseHasMovieValues()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var addedAtUnixTime = now.ToUnixTimeSeconds();
            var updatedAtUnixTime = now.AddDays(1).ToUnixTimeSeconds();

            var sourceData = new GetMediaMetaDataMetadata
            {
                RatingKey = "123",
                Key = "/library/metadata/123",
                Type = GetMediaMetaDataType.Movie,
                Title = "Test Movie",
                Summary = "Test summary",
                Year = 2023,
                OriginalTitle = "Original Test Movie",
                ChildCount = 5,
                Studio = "Test Studio",
                ContentRating = "PG-13",
                Duration = 5400000, // 90 minutes in milliseconds
                Thumb = "/thumb/path",
                Art = "/art/path",
                Theme = "/theme/path",
                Guid = "plex://movie/guid",
                AddedAt = addedAtUnixTime,
                UpdatedAt = updatedAtUnixTime,
                OriginallyAvailableAt = LocalDate.FromDateOnly(new DateOnly(2023, 01, 01)),
                AudienceRating = 8.5f,
                Rating = 9.0f,
                GrandparentTitle = "Series Title",
                ParentTitle = "Season 1",
                ParentGuid = "plex://season/guid",
                ParentRatingKey = "456",

                // Collections of nested objects
                Media =
                [
                    new GetMediaMetaDataMedia
                    {
                        Id = 789,
                        Duration = 5400000,
                        Bitrate = 5000,
                        Width = 1920,
                        Height = 1080,
                        AspectRatio = 1.78f,
                        AudioChannels = 6,
                        AudioCodec = "aac",
                        VideoCodec = "h264",
                        VideoResolution = "1080p",
                        Container = "mp4",
                        VideoFrameRate = "24p",
                        VideoProfile = "high",
                        AudioProfile = "lc",
                        HasVoiceActivity = true,
                        Part =
                        [
                            new GetMediaMetaDataPart
                            {
                                Id = 101,
                                Accessible = true,
                                Exists = true,
                                Key = "/library/parts/101",
                                Indexes = "sd",
                                Duration = 5400000,
                                File = "/movies/testmovie.mp4",
                                Size = 5000000000,
                                Container = "mp4",
                                VideoProfile = "high",
                                AudioProfile = "lc",
                                Stream =
                                [
                                    new GetMediaMetaDataStream
                                    {
                                        Default = true,
                                        Codec = "h264",
                                        Index = 0,
                                        Bitrate = 4000,
                                        Language = "English",
                                        LanguageTag = "en",
                                        LanguageCode = "eng",
                                        Selected = true,
                                    },
                                ],
                            },
                        ],
                    },
                ],
                Genre = [new GetMediaMetaDataGenre { Tag = "Action" }, new GetMediaMetaDataGenre { Tag = "Sci-Fi" }],
                Country = [new GetMediaMetaDataCountry { Tag = "USA" }],
                Role = [new GetMediaMetaDataRole { Tag = "Actor Name" }],
                Ratings =
                [
                    new GetMediaMetaDataRatings
                    {
                        Image = "imdb://image",
                        Type = "imdb",
                        Value = 8.7f,
                    },
                ],
                Guids = [new GetMediaMetaDataGuids { Id = "imdb://tt1234567" }],
            };

            // Act
            var result = sourceData.ToMediaItemDTO();

            // Assert
            result.ShouldNotBeNull();
            result.RatingKey.ShouldBe(sourceData.RatingKey);
            result.Key.ShouldBe(sourceData.Key);
            result.Title.ShouldBe(sourceData.Title);
            result.Summary.ShouldBe(sourceData.Summary);
            result.Year.ShouldBe(sourceData.Year ?? 0);
            result.OriginalTitle.ShouldBe(sourceData.OriginalTitle);
            result.ChildCount.ShouldBe(sourceData.ChildCount);
            result.Studio.ShouldBe(sourceData.Studio);
            result.ContentRating.ShouldBe(sourceData.ContentRating);

            // Duration is in milliseconds and we want seconds
            result.Duration.ShouldBe(sourceData.Duration / 1000);

            result.Thumb.ShouldBe(sourceData.Thumb);
            result.Art.ShouldBe(sourceData.Art);
            result.Theme.ShouldBe(sourceData.Theme);
            result.Guid.ShouldBe(sourceData.Guid);

            // DateTime conversions
            result.AddedAt.ShouldBe(
                DateTimeExtensions.FromUnixTime(sourceData.AddedAt),
                tolerance: TimeSpan.FromSeconds(1)
            );
            result.UpdatedAt.ShouldBe(
                DateTimeExtensions.FromUnixTime(sourceData.UpdatedAt ?? 0),
                tolerance: TimeSpan.FromSeconds(1)
            );

            result.OriginallyAvailableAt.ShouldNotBeNull();
            result.OriginallyAvailableAt.ShouldBe(sourceData.OriginallyAvailableAt.ToString());

            result.GrandparentTitle.ShouldBe(sourceData.GrandparentTitle);
            result.ParentTitle.ShouldBe(sourceData.ParentTitle);
            result.ParentGuid.ShouldBe(sourceData.ParentGuid);
            result.ParentRatingKey.ShouldBe(sourceData.ParentRatingKey);
            result.AudienceRating.ShouldBe(sourceData.AudienceRating);
            result.Rating.ShouldBe(sourceData.Rating);

            // Collections
            result.Genre.Count.ShouldBe(sourceData.Genre.Count);
            result.Genre.First().Name.ShouldBe(sourceData.Genre.First().Tag);

            result.Country.Count.ShouldBe(sourceData.Country.Count);
            result.Country.First().Name.ShouldBe(sourceData.Country.First().Tag);

            result.Role.Count.ShouldBe(sourceData.Role.Count);
            result.Role.First().Name.ShouldBe(sourceData.Role.First().Tag);

            result.Ratings.Count.ShouldBe(sourceData.Ratings.Count);
            result.Ratings.First().Type.ShouldBe(sourceData.Ratings.First().Type);
            result.Ratings.First().Value.ShouldBe(sourceData.Ratings.First().Value);

            result.Guids.Count.ShouldBe(sourceData.Guids.Count);
            result.Guids.First().Id.ShouldBe(sourceData.Guids.First().Id);

            // Media
            result.Media.Count.ShouldBe(1);
            var media = result.Media.First();
            var sourceMedia = sourceData.Media.First();

            media.Id.ShouldBe(sourceMedia.Id);
            media.Duration.ShouldBe(sourceMedia.Duration!.Value);
            media.Bitrate.ShouldBe(sourceMedia.Bitrate!.Value);
            media.Width.ShouldBe(sourceMedia.Width!.Value);
            media.Height.ShouldBe(sourceMedia.Height!.Value);
            media.HasVoiceActivity.ShouldBe(sourceMedia.HasVoiceActivity!.Value);

            // Parts
            media.Parts.Count.ShouldBe(1);
            var part = media.Parts.First();
            var sourcePart = sourceMedia.Part!.First();

            part.Id.ShouldBe(sourcePart.Id);
            part.Key.ShouldBe(sourcePart.Key);
            part.File.ShouldBe(sourcePart.File);

            // Streams
            part.Stream.Count.ShouldBe(1);
            var stream = part.Stream.First();
            var sourceStream = sourcePart.Stream!.First();

            stream.Id.ShouldBe(sourceStream.Id);
            stream.Codec.ShouldBe(sourceStream.Codec);
            stream.Language.ShouldBe(sourceStream.Language);
        }

        [Fact]
        public void ShouldMapAllPropertiesCorrectly_WhenApiResponseHasTvShowValues()
        {
            // Arrange
            var now = DateTimeOffset.UtcNow;
            var addedAtUnixTime = now.ToUnixTimeSeconds();
            var updatedAtUnixTime = now.AddDays(1).ToUnixTimeSeconds();

            var sourceData = new GetMediaMetaDataMetadata
            {
                RatingKey = "456",
                Key = "/library/metadata/456",
                Type = GetMediaMetaDataType.TvShow,
                Title = "Test TV Show",
                Summary = "Test TV show summary",
                Year = 2020,
                OriginalTitle = "Original Test TV Show",
                ChildCount = 8, // Seasons count
                Studio = "Test TV Studio",
                ContentRating = "TV-MA",
                Thumb = "/tv/thumb/path",
                Art = "/tv/art/path",
                Theme = "/tv/theme/path",
                Guid = "plex://show/guid",
                AddedAt = addedAtUnixTime,
                UpdatedAt = updatedAtUnixTime,
                OriginallyAvailableAt = LocalDate.FromDateOnly(new DateOnly(2020, 05, 15)),
                AudienceRating = 9.1f,
                Rating = 9.3f,
                LeafCount = 42, // Episode count
                ViewedLeafCount = 12, // Viewed episodes

                // Collections of nested objects
                Media = [], // TV Shows typically don't have direct media, episodes do
                Genre = [new GetMediaMetaDataGenre { Tag = "Drama" }, new GetMediaMetaDataGenre { Tag = "Mystery" }],
                Country = [new GetMediaMetaDataCountry { Tag = "United Kingdom" }],
                Role =
                [
                    new GetMediaMetaDataRole { Tag = "Lead Actor" },
                    new GetMediaMetaDataRole { Tag = "Supporting Actor" },
                ],
                Ratings =
                [
                    new GetMediaMetaDataRatings
                    {
                        Image = "imdb://image/show",
                        Type = "imdb",
                        Value = 9.2f,
                    },
                    new GetMediaMetaDataRatings
                    {
                        Image = "tmdb://image/show",
                        Type = "tmdb",
                        Value = 8.9f,
                    },
                ],
                Guids =
                [
                    new GetMediaMetaDataGuids { Id = "imdb://tt7654321" },
                    new GetMediaMetaDataGuids { Id = "tmdb://12345" },
                ],
            };

            // Act
            var result = sourceData.ToMediaItemDTO();

            // Assert
            result.ShouldNotBeNull();
            result.RatingKey.ShouldBe(sourceData.RatingKey);
            result.Key.ShouldBe(sourceData.Key);
            result.Title.ShouldBe(sourceData.Title);
            result.Summary.ShouldBe(sourceData.Summary);
            result.Year.ShouldBe(sourceData.Year ?? 0);
            result.OriginalTitle.ShouldBe(sourceData.OriginalTitle);
            result.ChildCount.ShouldBe(sourceData.ChildCount);
            result.Studio.ShouldBe(sourceData.Studio);
            result.ContentRating.ShouldBe(sourceData.ContentRating);

            // Duration is in milliseconds and we want seconds, but TV shows typically don't have duration at the show level
            result.Duration.ShouldBe(0);

            result.Thumb.ShouldBe(sourceData.Thumb);
            result.Art.ShouldBe(sourceData.Art);
            result.Theme.ShouldBe(sourceData.Theme);
            result.Guid.ShouldBe(sourceData.Guid);

            // DateTime conversions
            result.AddedAt.ShouldBe(
                DateTimeExtensions.FromUnixTime(sourceData.AddedAt),
                tolerance: TimeSpan.FromSeconds(1)
            );
            result.UpdatedAt.ShouldBe(
                DateTimeExtensions.FromUnixTime(sourceData.UpdatedAt ?? 0),
                tolerance: TimeSpan.FromSeconds(1)
            );

            // Parse the string date to NodaTime.LocalDate if not null
            result.OriginallyAvailableAt.ShouldNotBeNull();
            result.OriginallyAvailableAt.ShouldBe(sourceData.OriginallyAvailableAt.ToString());

            result.AudienceRating.ShouldBe(sourceData.AudienceRating);
            result.Rating.ShouldBe(sourceData.Rating);

            // Collections
            result.Genre.Count.ShouldBe(sourceData.Genre.Count);
            result.Genre.First().Name.ShouldBe(sourceData.Genre.First().Tag);
            result.Genre.Last().Name.ShouldBe(sourceData.Genre.Last().Tag);

            result.Country.Count.ShouldBe(sourceData.Country.Count);
            result.Country.First().Name.ShouldBe(sourceData.Country.First().Tag);

            result.Role.Count.ShouldBe(sourceData.Role.Count);
            result.Role.First().Name.ShouldBe(sourceData.Role.First().Tag);
            result.Role.Last().Name.ShouldBe(sourceData.Role.Last().Tag);

            result.Ratings.Count.ShouldBe(sourceData.Ratings.Count);
            result.Ratings.First().Type.ShouldBe(sourceData.Ratings.First().Type);
            result.Ratings.First().Value.ShouldBe(sourceData.Ratings.First().Value);
            result.Ratings.Last().Type.ShouldBe(sourceData.Ratings.Last().Type);
            result.Ratings.Last().Value.ShouldBe(sourceData.Ratings.Last().Value);

            result.Guids.Count.ShouldBe(sourceData.Guids.Count);
            result.Guids.First().Id.ShouldBe(sourceData.Guids.First().Id);
            result.Guids.Last().Id.ShouldBe(sourceData.Guids.Last().Id);

            // Media list should be empty for TV show at show level
            result.Media.Count.ShouldBe(0);
        }
    }
}
