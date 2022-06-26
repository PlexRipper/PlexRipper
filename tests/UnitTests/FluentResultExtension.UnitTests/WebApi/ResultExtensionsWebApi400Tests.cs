using FluentResultExtensions;

namespace FluentResultExtensionTests;

public class ResultExtensionsWebApi400Tests
{
    #region Result

    [Fact]
    public void ShouldAddBadRequestError_WhenAdd400BadRequestErrorCalled()
    {
        // Arrange
        var result = new Result();

        // Act
        result.Add400BadRequestError();

        // Assert
        result.Reasons.First().Metadata.ContainsKey(ResultExtensions.StatusCodeName).ShouldBeTrue();
        result.Reasons.First().Metadata[ResultExtensions.StatusCodeName].ShouldBe(HttpCodes.Status400BadRequest);
    }

    [Fact]
    public void ShouldHaveBadRequestError_WhenHad400BadRequestErrorCalled()
    {
        // Arrange
        var result = new Result().Add400BadRequestError();

        // Act
        var has400error = result.Has400BadRequestError();

        // Assert
        has400error.ShouldBeTrue();
    }

    #endregion

    #region Result<T>

    [Fact]
    public void ShouldAdd400BadRequestError_WhenAdd400BadRequestErrorCalledOnResultT()
    {
        // Arrange
        var result = new Result<int>();

        // Act
        result.Add400BadRequestError();

        // Assert
        result.Reasons.First().Metadata.ContainsKey(ResultExtensions.StatusCodeName).ShouldBeTrue();
        result.Reasons.First().Metadata[ResultExtensions.StatusCodeName].ShouldBe(HttpCodes.Status400BadRequest);
    }

    [Fact]
    public void ShouldHave400BadRequestError_WhenHad400BadRequestErrorCalledOnResultT()
    {
        // Arrange
        var result = new Result<int>().Add400BadRequestError();

        // Act
        var has400BadRequestError = result.Has400BadRequestError();

        // Assert
        has400BadRequestError.ShouldBeTrue();
    }

    [Fact]
    public void ShouldCreate404NotFoundResult_WhenCreate400BadRequestResultCalled()
    {
        // Act
        var result = ResultExtensions.Create400BadRequestResult();

        // Assert
        result.Has400BadRequestError().ShouldBeTrue();
    }

    [Fact]
    public void ShouldHaveMessage_WhenCreate400BadRequestResultCalled()
    {
        // Arrange
        var message = "Test Message 12345678910";

        // Act
        var result = ResultExtensions.Create400BadRequestResult(message);

        // Assert
        result.Has400BadRequestError().ShouldBeTrue();
        result.Errors.First().Message.ShouldBe(message);
    }

    #endregion
}