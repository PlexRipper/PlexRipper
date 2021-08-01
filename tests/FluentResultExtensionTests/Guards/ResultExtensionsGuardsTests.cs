using FluentResults;
using Shouldly;
using Xunit;

namespace FluentResultExtensionTests.Guards
{
    public class ResultExtensionsGuardsTests
    {
        [Fact]
        public void ShouldHave400BadRequestError_WhenIsNullIsCalled()
        {
            // Arrange
            var result = ResultExtensions.IsNull("ParameterXYZ");

            // Act
            var has400BadRequestError = result.Has400BadRequestError();

            // Assert
            has400BadRequestError.ShouldBeTrue();
        }

        [Fact]
        public void ShouldHave400BadRequestError_WhenIsEmptyIsCalled()
        {
            // Arrange
            var result = ResultExtensions.IsEmpty("ParameterXYZ");

            // Act
            var has400BadRequestError = result.Has400BadRequestError();

            // Assert
            has400BadRequestError.ShouldBeTrue();
        }

        [Fact]
        public void ShouldHave400BadRequestError_WhenIsInvalidIdIsCalled()
        {
            // Arrange
            var result = ResultExtensions.IsInvalidId("ParameterXYZ");

            // Act
            var has400BadRequestError = result.Has400BadRequestError();

            // Assert
            has400BadRequestError.ShouldBeTrue();
        }


        [Fact]
        public void ShouldHave404NotFoundError_WhenEntityNotFoundIsCalled()
        {
            // Arrange
            var result = ResultExtensions.EntityNotFound("ParameterXYZ", 0);

            // Act
            var has404NotFoundError = result.Has404NotFoundError();

            // Assert
            has404NotFoundError.ShouldBeTrue();
        }
    }
}