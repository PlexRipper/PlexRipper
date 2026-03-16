using System.Net;

namespace Reaparr.FluentResultExtension.UnitTests;

public class ResultExtensionsGeneralTests
{
    [Fact]
    public void ShouldNotFindStatusCode_WhenGivenAnInvalidHasStatusCode()
    {
        // Arrange
        var result = new Result().Add404NotFoundError();

        // Act
        var has201CreatedRequest = result.Has201CreatedRequestSuccess();

        // Assert
        has201CreatedRequest.ShouldBeFalse();
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public void ShouldMapKnownHttpStatusCodesForResult(HttpStatusCode statusCode)
    {
        // Arrange
        var result = Result.Ok();

        // Act
        var mapped = result.AddStatusCode(statusCode);

        // Assert
        mapped.HasStatusCode((int)statusCode).ShouldBeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.NoContent)]
    [InlineData(HttpStatusCode.Forbidden)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public void ShouldMapKnownHttpStatusCodesForGenericResult(HttpStatusCode statusCode)
    {
        // Arrange
        var result = Result.Ok("ok");

        // Act
        var mapped = result.AddStatusCode(statusCode);

        // Assert
        mapped.HasStatusCode((int)statusCode).ShouldBeTrue();
    }

    [Fact]
    public void ShouldFallbackToRawStatusCode_WhenNoSpecialMappingExists()
    {
        // Arrange
        var result = Result.Fail("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.ServiceUnavailable, "service unavailable");

        // Assert
        mapped.HasStatusCode((int)HttpStatusCode.ServiceUnavailable).ShouldBeTrue();
    }

    [Fact]
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

    [Fact]
    public void ShouldMapInternalServerErrorStatusCode()
    {
        // Arrange
        var result = Result.Fail("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.InternalServerError, "server error");

        // Assert
        mapped.Has500InternalServerError().ShouldBeTrue();
    }

    [Fact]
    public void ShouldMapServiceUnavailableStatusCode()
    {
        // Arrange
        var result = Result.Fail("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.ServiceUnavailable, "unavailable");

        // Assert
        mapped.Has503ServiceUnavailableError().ShouldBeTrue();
    }

    [Fact]
    public void ShouldMapInternalServerErrorStatusCodeForGenericResult()
    {
        // Arrange
        var result = Result.Fail<string>("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.InternalServerError, "server error");

        // Assert
        mapped.Has500InternalServerError().ShouldBeTrue();
    }

    [Fact]
    public void ShouldMapServiceUnavailableStatusCodeForGenericResult()
    {
        // Arrange
        var result = Result.Fail<string>("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.ServiceUnavailable, "unavailable");

        // Assert
        mapped.Has503ServiceUnavailableError().ShouldBeTrue();
    }

    [Fact]
    public void ShouldMapTooManyRequestsStatusCodeForGenericResult()
    {
        // Arrange
        var result = Result.Fail<string>("failed");

        // Act
        var mapped = result.AddStatusCode(HttpStatusCode.TooManyRequests, "too many requests");

        // Assert
        mapped.Has429TooManyRequestsError().ShouldBeTrue();
    }

    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.BadGateway)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    [InlineData(HttpStatusCode.GatewayTimeout)]
    public void ShouldIdentifyServerUnreachableStatuses(HttpStatusCode statusCode)
    {
        // Arrange
        var result = Result.Fail("failed").AddStatusCode(statusCode);

        // Act
        var isServerUnreachable = result.IsServerUnreachable();

        // Assert
        isServerUnreachable.ShouldBeTrue();
    }

    [Fact]
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
