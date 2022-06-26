using FluentResultExtensions;

namespace FluentResultExtensionTests
{
    public class ResultExtensionsWebApi404Tests
    {
        #region Result

        [Fact]
        public void ShouldAdd404NotFoundError_WhenAdd404NotFoundErrorCalled()
        {
            // Arrange
            var result = new Result();

            // Act
            result.Add404NotFoundError();

            // Assert
            result.Reasons.First().Metadata.ContainsKey(ResultExtensions.StatusCodeName).ShouldBeTrue();
            result.Reasons.First().Metadata[ResultExtensions.StatusCodeName].ShouldBe(HttpCodes.Status404NotFound);
        }

        [Fact]
        public void ShouldHave404NotFoundError_WhenHad404NotFoundErrorCalled()
        {
            // Arrange
            var result = new Result().Add404NotFoundError();

            // Act
            var has404NotFoundError = result.Has404NotFoundError();

            // Assert
            has404NotFoundError.ShouldBeTrue();
        }

        #endregion

        #region Result<T>

        [Fact]
        public void ShouldAdd404NotFoundError_WhenAdd404NotFoundErrorCalledOnResultT()
        {
            // Arrange
            var result = new Result<int>();

            // Act
            result.Add404NotFoundError();

            // Assert
            result.Reasons.First().Metadata.ContainsKey(ResultExtensions.StatusCodeName).ShouldBeTrue();
            result.Reasons.First().Metadata[ResultExtensions.StatusCodeName].ShouldBe(HttpCodes.Status404NotFound);
        }

        [Fact]
        public void ShouldHave404NotFoundError_WhenHad404NotFoundErrorCalledOnResultT()
        {
            // Arrange
            var result = new Result<int>().Add404NotFoundError();

            // Act
            var has404NotFoundError = result.Has404NotFoundError();

            // Assert
            has404NotFoundError.ShouldBeTrue();
        }

        [Fact]
        public void ShouldCreate404NotFoundResult_WhenCreate404NotFoundResultCalled()
        {
            // Act
            var result = ResultExtensions.Create404NotFoundResult();

            // Assert
            result.Has404NotFoundError().ShouldBeTrue();
        }

        [Fact]
        public void ShouldHaveMessage_WhenCreate404NotFoundResultCalled()
        {
            // Arrange
            var message = "Test Message 12345678910";

            // Act
            var result = ResultExtensions.Create404NotFoundResult(message);

            // Assert
            result.Has404NotFoundError().ShouldBeTrue();
            result.Errors.First().Message.ShouldBe(message);
        }

        #endregion
    }
}