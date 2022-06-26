using Shouldly;
using Xunit;

namespace FluentResultExtensionTests
{
    public class ResultExtensionsGeneral
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
    }
}