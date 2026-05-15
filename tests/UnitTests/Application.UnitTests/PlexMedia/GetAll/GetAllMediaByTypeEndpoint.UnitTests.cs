namespace Reaparr.Application.UnitTests;

public class GetAllMediaByTypeEndpointUnitTests : BaseUnitTest<GetAllMediaByTypeEndpoint>
{
    [Test]
    public async Task ShouldMapFriendlyRequestFiltersToMediaQueryFilter_WhenHandlingRequest()
    {
        // Arrange
        var request = new GetAllMediaByTypeRequest
        {
            MediaType = PlexMediaType.Movie,
            PlexLibraryId = 42,
            Page = 2,
            PageSize = 25,
            Search = "matrix",
            CountryId = 7,
            GenreId = 13,
            RoleId = 11,
            QualityId = 480,
            Sort = "sortIndex:asc",
            FilterOwnedMedia = true,
            FilterOfflineMedia = true,
        };

        Mock.Mock<ICommandExecutor>()
            .Setup(x =>
                x.Send(
                    It.Is<GetMediaByTypeCommand>(command =>
                        command.Filter.MediaType == PlexMediaType.Movie
                        && command.Filter.PlexLibraryId == 42
                        && command.Filter.FilterOwnedMedia
                        && command.Filter.FilterOfflineMedia
                        && command.Filter.Parameters.Page == 2
                        && command.Filter.Parameters.PageSize == 25
                        && command.Filter.Parameters.Sort == "sortIndex:asc"
                        && command.Filter.Parameters.Filter
                            == "SearchTitle:contains:matrix&Countries:any:Id:eq:7&Actors:any:Id:eq:11&Genres:any:Id:eq:13&MediaDataList:any:Quality:eq:SD"),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(Result.Ok(new PagedMediaQueryResult()))
            .Verifiable(Times.Once());

        // Act
        var endpoint = SetupEndpointUnitTest<GetAllMediaByTypeEndpoint>();
        await endpoint.HandleAsync(request, CancellationToken);

        // Assert
        Mock.Mock<ICommandExecutor>().Verify();
    }

    [Test]
    public void ShouldRejectNonPositiveFriendlyFilterIds_WhenValidatingRequest()
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
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetAllMediaByTypeRequest.CountryId));
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetAllMediaByTypeRequest.GenreId));
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetAllMediaByTypeRequest.RoleId));
        result.Errors.ShouldContain(x => x.PropertyName == nameof(GetAllMediaByTypeRequest.QualityId));
    }
}
