using System.Net;

namespace Reaparr.FluentResultExtension.UnitTests;

public class ResultExtensionsGeneralTests
{
    [Test]
    public void ShouldNotFindStatusCode_WhenGivenAnInvalidHasStatusCode()
    {
        // Arrange
        var result = new Result().Add404NotFoundError();

        // Act
        var has201CreatedRequest = result.Has201CreatedRequestSuccess();

        // Assert
        has201CreatedRequest.ShouldBeFalse();
    }

    [Test]
    [Arguments(HttpStatusCode.NoContent)]
    [Arguments(HttpStatusCode.Forbidden)]
    [Arguments(HttpStatusCode.InternalServerError)]
    [Arguments(HttpStatusCode.ServiceUnavailable)]
    [Arguments(HttpStatusCode.GatewayTimeout)]
    public void ShouldMapKnownHttpStatusCodesForResult(HttpStatusCode statusCode)
    {
        // Arrange
        var result = Result.Ok();

        // Act
        var mapped = result.AddStatusCode(statusCode);

        // Assert
        mapped.HasStatusCode((int)statusCode).ShouldBeTrue();
    }

    [Test]
    [Arguments(HttpStatusCode.NoContent)]
    [Arguments(HttpStatusCode.Forbidden)]
    [Arguments(HttpStatusCode.InternalServerError)]
    [Arguments(HttpStatusCode.ServiceUnavailable)]
    [Arguments(HttpStatusCode.GatewayTimeout)]
    public void ShouldMapKnownHttpStatusCodesForGenericResult(HttpStatusCode statusCode)
    {
        // Arrange
        var result = Result.Ok("ok");

        // Act
        var mapped = result.AddStatusCode(statusCode);

        // Assert
        mapped.HasStatusCode((int)statusCode).ShouldBeTrue();
    }

    [Test]
    public void ShouldFallbackToRawStatusCode_WhenNoSpecialMappingExists()
    {
        // Arrange
        var result = Result.Fail("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.ServiceUnavailable, "service unavailable");

        // Assert
        mapped.HasStatusCode((int)HttpStatusCode.ServiceUnavailable).ShouldBeTrue();
    }

    [Test]
    public void ShouldMapTooManyRequestsStatusCode()
    {
        // Arrange
        var result = Result.Fail("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.TooManyRequests, "too many requests");

        // Assert
        mapped.HasStatusCode((int)HttpStatusCode.TooManyRequests).ShouldBeTrue();
        mapped.Has429TooManyRequestsError().ShouldBeTrue();
    }

    [Test]
    public void ShouldMapInternalServerErrorStatusCode()
    {
        // Arrange
        var result = Result.Fail("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.InternalServerError, "server error");

        // Assert
        mapped.Has500InternalServerError().ShouldBeTrue();
    }

    [Test]
    public void ShouldMapServiceUnavailableStatusCode()
    {
        // Arrange
        var result = Result.Fail("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.ServiceUnavailable, "unavailable");

        // Assert
        mapped.Has503ServiceUnavailableError().ShouldBeTrue();
    }

    [Test]
    public void ShouldMapInternalServerErrorStatusCodeForGenericResult()
    {
        // Arrange
        var result = Result.Fail<string>("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.InternalServerError, "server error");

        // Assert
        mapped.Has500InternalServerError().ShouldBeTrue();
    }

    [Test]
    public void ShouldMapServiceUnavailableStatusCodeForGenericResult()
    {
        // Arrange
        var result = Result.Fail<string>("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.ServiceUnavailable, "unavailable");

        // Assert
        mapped.Has503ServiceUnavailableError().ShouldBeTrue();
    }

    [Test]
    public void ShouldMapTooManyRequestsStatusCodeForGenericResult()
    {
        // Arrange
        var result = Result.Fail<string>("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.TooManyRequests, "too many requests");

        // Assert
        mapped.Has429TooManyRequestsError().ShouldBeTrue();
    }

    [Test]
    [Arguments(HttpStatusCode.RequestTimeout)]
    [Arguments(HttpStatusCode.InternalServerError)]
    [Arguments(HttpStatusCode.BadGateway)]
    [Arguments(HttpStatusCode.ServiceUnavailable)]
    [Arguments(HttpStatusCode.GatewayTimeout)]
    public void ShouldIdentifyServerUnreachableStatuses(HttpStatusCode statusCode)
    {
        // Arrange
        var result = Result.Fail("failed").AddStatusCode(statusCode);

        // Act
        var isServerUnreachable = result.IsServerUnreachable();

        // Assert
        isServerUnreachable.ShouldBeTrue();
    }

    [Test]
    public void ShouldIdentifyServerUnreachableStatusWhenHttpClientTimeout()
    {
        // Arrange
        var result = Result.Fail(
            new ExceptionalError(
                new TaskCanceledException(
                    "The request was canceled due to the configured HttpClient.Timeout of 30 seconds elapsing."
                )
            )
        );

        // Act
        var isServerUnreachable = result.IsServerUnreachable();

        // Assert
        isServerUnreachable.ShouldBeTrue();
    }

    [Test]
    public void ShouldNotIdentifyClientErrorsAsServerUnreachable()
    {
        // Arrange
        var result = Result.Fail("failed").AddStatusCode(HttpStatusCode.NotFound);

        // Act
        var isServerUnreachable = result.IsServerUnreachable();

        // Assert
        isServerUnreachable.ShouldBeFalse();
    }
}
