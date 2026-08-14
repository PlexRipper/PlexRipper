using Microsoft.Extensions.DependencyInjection;

namespace Reaparr.Application.UnitTests;

public class GetAllMediaByTypeEndpointUnitTests
    : BaseEndpointUnitTest<GetAllMediaByTypeEndpoint, GetAllMediaByTypeRequest, PlexMediaStatisticsDTO>
{
    [Test]
    public async Task ShouldMapFriendlyRequestFiltersToMediaQueryFilter_WhenHandlingRequest()
    {
        // Arrange
        await SetupDatabase(
            42,
            config =>
            {
                config.PlexServerCount = 1;
                config.PlexMovieLibraryCount = 1;
            }
        );
        var libraryId = await IDbContext
            .PlexLibraries.Where(x => x.Type == PlexMediaType.Movie)
            .Select(x => x.Id)
            .FirstAsync(CancellationToken);

        var request = new GetAllMediaByTypeRequest
        {
            MediaType = PlexMediaType.Movie,
            PlexLibraryId = libraryId,
            Page = 2,
            PageSize = 25,
            Search = "matrix",
            CountryId = 7,
            GenreId = 13,
            RoleId = 11,
            QualityId = 480,
            ComparisonState = PlexMediaComparisonState.Missing,
            Sort = "sortIndex:asc",
            FilterOwnedMedia = true,
            FilterOfflineMedia = true,
        };

        var expectedFilter = string.Join(
            '&',
            [
                $"SearchTitle:contains:{request.Search}",
                $"Countries:any:Id:eq:{request.CountryId}",
                $"Actors:any:Id:eq:{request.RoleId}",
                $"Genres:any:Id:eq:{request.GenreId}",
                $"MediaDataList:any:Quality:eq:{request.QualityId!.Value.ToVideoQuality()}",
            ]
        );

        var mediaQueryCache = new Mock<IMediaQueryCache>(MockBehavior.Strict);
        mediaQueryCache
            .Setup(x =>
                x.GetMediaAsync(
                    It.Is<MediaQueryFilter>(filter =>
                        filter.MediaType == PlexMediaType.Movie
                        && filter.PlexLibraryId == libraryId
                        && filter.FilterOwnedMedia
                        && filter.FilterOfflineMedia
                        && filter.ComparisonState == PlexMediaComparisonState.Missing
                        && filter.Parameters.Page == 2
                        && filter.Parameters.PageSize == 25
                        && filter.Parameters.Sort == "sortIndex:asc"
                        && filter.Parameters.Filter == expectedFilter
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new PagedMediaQueryResult()))
            .Verifiable(Times.Once());

        // Act
        await TestEndpointHandleAsync(request, services => services.AddSingleton(_ => mediaQueryCache.Object));

        // Assert
        mediaQueryCache.Verify();
    }

    [Test]
    public void ShouldRejectNonPositiveFriendlyFilterIdsButAllowAllMediaComparisonState_WhenValidatingRequest()
    {
        // Arrange
        var validator = new GetAllMediaByTypeRequestValidator();
        var request = new GetAllMediaByTypeRequest
        {
            MediaType = PlexMediaType.Movie,
            PlexLibraryId = 0,
            CountryId = 0,
            GenreId = -1,
            RoleId = 0,
            QualityId = -1,
            ComparisonState = PlexMediaComparisonState.Missing,
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetAllMediaByTypeRequest.CountryId));
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetAllMediaByTypeRequest.GenreId));
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetAllMediaByTypeRequest.RoleId));
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetAllMediaByTypeRequest.QualityId));
        result.Errors.ShouldNotContain(x => x.PropertyName == nameof(GetAllMediaByTypeRequest.PlexLibraryId));
    }
}
