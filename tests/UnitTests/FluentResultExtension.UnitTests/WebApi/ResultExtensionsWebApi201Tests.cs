namespace Reaparr.FluentResultExtension.UnitTests;

public class ResultExtensionsWebApi201Tests
{
    #region Result

    [Test]
    public void ShouldAdd201CreatedRequestSuccess_WhenAdd201CreatedRequestSuccessCalled()
    {
        // Arrange
        var result = new Result();

        // Act
        result.Add201CreatedRequestSuccess();

        // Assert
        result.Reasons.First().Metadata.ContainsKey(ResultExtensions.StatusCodeName).ShouldBeTrue();
        result.Reasons.First().Metadata[ResultExtensions.StatusCodeName].ShouldBe(HttpCodes.Status201Created);
    }

    [Test]
    public void ShouldHave201CreatedRequestSuccess_WhenHad201CreatedRequestSuccessCalled()
    {
        // Arrange
        var result = new Result().Add201CreatedRequestSuccess();

        // Act
        var has201CreatedRequest = result.Has201CreatedRequestSuccess();

        // Assert
        has201CreatedRequest.ShouldBeTrue();
    }

    [Test]
    public void ShouldCreate201CreatedResult_WhenCreate201CreatedResultCalled()
    {
        // Act
        var result = ResultExtensions.Create201CreatedResult();

        // Assert
        result.Has201CreatedRequestSuccess().ShouldBeTrue();
    }

    #endregion

    #region Result<T>

    [Test]
    public void ShouldAdd201CreatedRequestSuccess_WhenAdd201CreatedRequestSuccessCalledOnResultT()
    {
        // Arrange
        var result = new Result<int>();

        // Act
        result.Add201CreatedRequestSuccess();

        // Assert
        result.Reasons.First().Metadata.ContainsKey(ResultExtensions.StatusCodeName).ShouldBeTrue();
        result.Reasons.First().Metadata[ResultExtensions.StatusCodeName].ShouldBe(HttpCodes.Status201Created);
    }

    [Test]
    public void ShouldHave201CreatedRequestSuccess_WhenHad201CreatedRequestSuccessCalledOnResultT()
    {
        // Arrange
        var result = new Result<int>().Add201CreatedRequestSuccess();

        // Act
        var has201CreatedRequest = result.Has201CreatedRequestSuccess();

        // Assert
        has201CreatedRequest.ShouldBeTrue();
    }

    [Test]
    public void ShouldPreserveValue_WhenAdd201CreatedRequestSuccessCalledOnResultT()
    {
        // Arrange
        var result = new Result<int>().WithValue(100);

        // Act
        result.Add201CreatedRequestSuccess();

        // Assert
        result.Value.ShouldBe(100);
    }

    [Test]
    public void ShouldCreate201CreatedResult_WhenCreate201CreatedResultCalledWithResultT()
    {
        // Act
        var result = ResultExtensions.Create201CreatedResult(100);

        // Assert
        result.Has201CreatedRequestSuccess().ShouldBeTrue();
    }

    [Test]
    public void ShouldPreserveValue_WhenCreate201CreatedResultCalledWithResultT()
    {
        // Act
        var result = ResultExtensions.Create201CreatedResult(100);

        // Assert
        result.Has201CreatedRequestSuccess().ShouldBeTrue();
        result.Value.ShouldBe(100);
    }

    #endregion
}
