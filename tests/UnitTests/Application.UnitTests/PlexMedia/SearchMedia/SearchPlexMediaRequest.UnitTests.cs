namespace Reaparr.Application.UnitTests;

public class SearchPlexMediaRequestUnitTests
    : BaseEndpointUnitTest<SearchPlexMediaEndpoint, SearchPlexMediaRequest, ResultDTO<List<PlexMediaSlimDTO>>>
{
    [Test]
    public async Task ShouldRejectNullQuery_WhenSearchingPlexMedia()
    {
        // Arrange
        var request = new SearchPlexMediaRequest { Query = null! };

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Response.ShouldBeNull();
        result.ValidationResult.ShouldNotBeNull();
        result.ValidationResult.Errors.ShouldContain(x => x.PropertyName == nameof(SearchPlexMediaRequest.Query));
    }

    [Test]
    public async Task ShouldRejectEmptyQuery_WhenSearchingPlexMedia()
    {
        // Arrange
        var request = new SearchPlexMediaRequest { Query = string.Empty };

        // Act
        var result = await TestEndpointHandleAsync(request);

        // Assert
        result.IsValid.ShouldBeFalse();
        result.Response.ShouldBeNull();
        result.ValidationResult.ShouldNotBeNull();
        result.ValidationResult.Errors.ShouldContain(x => x.PropertyName == nameof(SearchPlexMediaRequest.Query));
    }
}
