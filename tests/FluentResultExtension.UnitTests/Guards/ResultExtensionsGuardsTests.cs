using System.Collections.Generic;
using System.Linq;
using FluentResults;
using Shouldly;
using Xunit;

namespace FluentResultExtensionTests.Guards
{
    public class ResultExtensionsGuardsTests
    {
        #region General

        [Fact]
        public void ShouldHaveAddedErrors_WhenAddNestedErrorsIsCalled()
        {
            // Arrange
            var result = Result.Fail("Main Error");

            var errors = new List<Error>
            {
                new Error("Error #1"),
                new Error("Error #2"),
                new Error("Error #3"),
                new Error("Error #4"),
                new Error("Error #5"),
            };

            // Act
            var resultWithErrors = result.AddNestedErrors(errors);

            // Assert
            resultWithErrors.Errors.Count.ShouldBe(1);
            resultWithErrors.Errors.First().Reasons.Count.ShouldBe(5);
        }

        #endregion

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