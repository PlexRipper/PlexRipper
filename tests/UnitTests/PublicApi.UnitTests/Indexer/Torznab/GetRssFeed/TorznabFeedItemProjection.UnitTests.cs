using System.Globalization;
using Reaparr.Application.Contracts;

namespace Reaparr.PublicAPI.UnitTests;

public class TorznabFeedItemProjectionUnitTests
{
    [Test]
    public void ShouldKeepStableId_WhenRuntimeValuesChange()
    {
        // Arrange
        var projection = CreateProjection();
        var otherIntegration = new IntegrationIdentity(IntegrationType.Sonarr, Guid.NewGuid());

        // Act
        var first = projection.ToTorznabItem(
            new IntegrationIdentity(IntegrationType.Radarr, Guid.NewGuid()),
            "first-key",
            "http://first-host"
        );
        var second = projection.ToTorznabItem(otherIntegration, "second-key", "http://second-host");

        // Assert
        first.Guid.Value.ShouldBe(second.Guid.Value);
        first.Link.ShouldNotBe(second.Link);
        first.Guid.Value.ShouldStartWith("reaparr-");
        first.Guid.Value.Length.ShouldBe(72);
    }

    [Test]
    public void ShouldChangeStableId_WhenPlexIdentityChanges()
    {
        // Arrange
        var projection = CreateProjection();

        // Act
        var ids = new[]
        {
            projection.CreateStableId(),
            (
                projection with
                {
                    PlexServerMachineIdentifier = "444f1d4d82d020fa33821f80578c574f155cf14a",
                }
            ).CreateStableId(),
            (projection with { MediaType = PlexMediaType.Episode }).CreateStableId(),
            (projection with { PlexApiRatingKey = projection.PlexApiRatingKey + 1 }).CreateStableId(),
            (projection with { PlexApiMediaId = projection.PlexApiMediaId + 1 }).CreateStableId(),
            (projection with { PlexApiPartId = projection.PlexApiPartId + 1 }).CreateStableId(),
        };

        // Assert
        ids.Distinct().Count().ShouldBe(ids.Length);
    }

    [Test]
    public void ShouldKeepStableId_WhenCurrentCultureUsesNonLatinDigits()
    {
        // Arrange
        var projection = CreateProjection();
        var originalCulture = CultureInfo.CurrentCulture;

        // Act
        string invariantId;
        string alternateCultureId;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            invariantId = projection.CreateStableId();
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
            alternateCultureId = projection.CreateStableId();
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }

        // Assert
        alternateCultureId.ShouldBe(invariantId);
    }

    [Test]
    public void ShouldEmitNumericParentAndSubcategoryAttributes()
    {
        // Arrange
        var projection = CreateProjection() with { GenreTypes = [PlexGenreType.Foreign] };

        // Act
        var item = projection.ToTorznabItem(
            new IntegrationIdentity(IntegrationType.Radarr, Guid.NewGuid()),
            "key",
            "http://localhost"
        );

        // Assert
        item.Attributes.Where(x => x.Name == "category").Select(x => x.Value).ShouldBe(["2000", "2040", "2010", "2070"]);
    }

    [Test]
    public void ShouldEmitAllMatchingTvGenreCategories()
    {
        // Arrange
        var projection = CreateProjection() with
        {
            MediaType = PlexMediaType.Episode,
            GenreTypes =
            [
                PlexGenreType.Foreign,
                PlexGenreType.Anime,
                PlexGenreType.Documentary,
                PlexGenreType.Sport,
            ],
        };

        // Act
        var item = projection.ToTorznabItem(
            new IntegrationIdentity(IntegrationType.Sonarr, Guid.NewGuid()),
            "key",
            "http://localhost"
        );

        // Assert
        item.Attributes.Where(x => x.Name == "category").Select(x => x.Value).ShouldBe([
            "5000", "5040", "5020", "5060", "5070", "5080",
        ]);
    }

    [Test]
    public void ShouldIgnoreUnknownAndGroupGenreTypes()
    {
        // Arrange
        var projection = CreateProjection() with
        {
            MediaType = PlexMediaType.Episode,
            GenreTypes = [PlexGenreType.Unknown, PlexGenreType.Group],
        };

        // Act
        var item = projection.ToTorznabItem(
            new IntegrationIdentity(IntegrationType.Sonarr, Guid.NewGuid()),
            "key",
            "http://localhost"
        );

        // Assert
        item.Attributes.Where(x => x.Name == "category").Select(x => x.Value).ShouldBe(["5000", "5040"]);
    }

    private static TorznabFeedItemProjection CreateProjection() =>
        new()
        {
            MediaType = PlexMediaType.Movie,
            MediaId = 1,
            DataId = 2,
            PlexServerId = 3,
            PlexServerMachineIdentifier = "333f1d4d82d020fa33821f80578c574f155cf14a",
            PlexLibraryId = 4,
            PlexApiRatingKey = 5,
            PlexApiMediaId = 6,
            PlexApiPartId = 7,
            Title = "Movie.2026.1080p",
            AddedAt = new DateTime(2026, 9, 12, 9, 0, 0, DateTimeKind.Utc),
            Size = 1024,
            Quality = VideoQuality.FullHD,
            VideoResolution = VideoQuality.FullHD,
            Source = ReleaseSource.WebDl,
            VideoCodec = "h264",
            AudioCodec = "aac",
            SeasonNumber = 0,
            EpisodeNumber = 0,
            TvdbId = 0,
            TmdbId = 0,
            ImdbId = string.Empty,
            GenreTypes = [],
        };
}
