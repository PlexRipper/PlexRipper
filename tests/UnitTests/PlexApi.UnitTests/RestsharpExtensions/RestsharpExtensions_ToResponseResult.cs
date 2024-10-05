using System.Net;
using System.Text.Json;
using PlexRipper.PlexApi;
using RestSharp;

namespace PlexApi.UnitTests.RestsharpExtensions;

public class RestsharpExtensions_ToResponseResult : BaseUnitTest<RestsharpExtensions_ToResponseResult>
{
    #region Setup/Teardown

    public RestsharpExtensions_ToResponseResult(ITestOutputHelper output)
        : base(output) { }

    #endregion

    private RestRequest CreateRestRequest() => new() { Resource = "test" };

    [Fact]
    public void ShouldHaveAFailedResult_WhenOkResponseContainsBadGateway()
    {
        // Arrange
        var content = "Bad Gateway";
        var response = new RestResponse<string>(new RestRequest())
        {
            IsSuccessStatusCode = true,
            StatusCode = HttpStatusCode.OK,
            Content = content,
            Request = CreateRestRequest(),
        };

        // Act
        var result = response.ToResponseResult();

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
        result.Has502BadGatewayError().ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveAFailedResult_WhenTimeoutTaskCanceledException()
    {
        // Arrange
        var response = new RestResponse<string>(new RestRequest())
        {
            StatusCode = 0,
            ResponseStatus = ResponseStatus.TimedOut,
            Request = CreateRestRequest(),
            ErrorMessage = "A task was canceled.",
            ErrorException = new TaskCanceledException("A task was canceled."),
        };

        // Act
        var result = response.ToResponseResult();

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
        result.Has408RequestTimeout().ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveAFailedResultWithPlexError_WhenPlexErrorEnterVerificationCodeResponse()
    {
        // Arrange
        var response = new RestResponse<string>(new RestRequest())
        {
            StatusCode = HttpStatusCode.Unauthorized,
            ResponseStatus = ResponseStatus.Error,
            Request = CreateRestRequest(),
            Content = JsonSerializer.Serialize(FakePlexApiData.GetFailedEnterVerificationCode()),
        };

        // Act
        var result = response.ToResponseResult();

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
        result.HasPlexErrorEnterVerificationCode().ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveAFailedResultWithPlexError_WhenPlexErrorInvalidVerificationCodeResponse()
    {
        // Arrange
        var response = new RestResponse<string>(new RestRequest())
        {
            StatusCode = HttpStatusCode.Unauthorized,
            ResponseStatus = ResponseStatus.Error,
            Request = CreateRestRequest(),
            Content = JsonSerializer.Serialize(FakePlexApiData.GetFailedInvalidVerificationCode()),
        };

        // Act
        var result = response.ToResponseResult();

        // Assert
        result.ShouldNotBeNull();
        result.IsFailed.ShouldBeTrue();
        result.HasPlexErrorInvalidVerificationCode().ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveASuccessfulResult_WhenStatusCodeIsOK()
    {
        // Arrange
        var content = "test success";
        var response = new RestResponse<string>(new RestRequest())
        {
            IsSuccessStatusCode = true,
            StatusCode = HttpStatusCode.OK,
            Data = content,
            Request = CreateRestRequest(),
        };

        // Act
        var result = response.ToResponseResult();

        // Assert
        result.ShouldNotBeNull();
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(content);
    }
}
