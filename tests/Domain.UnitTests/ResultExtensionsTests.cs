using FluentResults;
using PlexRipper.BaseTests;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace PlexRipper.Domain.UnitTests
{
    public class ResultExtensionsTests
    {
        private BaseContainer Container { get; }

        public ResultExtensionsTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
            Container = new BaseContainer();
        }


        [Fact]
        public void ShouldHave400BadRequestError()
        {
            // Arrange
            string testMessage = "TestMessage1";

            // Act
            var result = ResultExtensions.Create400BadRequestResult(testMessage).LogError();
            var result2 = Result.Fail("").Add400BadRequestError(testMessage);

            // Assert
            result.Has400BadRequestError().ShouldBeTrue();
            result.Has404NotFoundError().ShouldBeFalse();
            result.Errors.Count.ShouldBe(1);
            result.Errors[0].Message.ShouldBe(testMessage);

            result2.Has400BadRequestError().ShouldBeTrue();
            result2.Has404NotFoundError().ShouldBeFalse();
            result2.Errors.Count.ShouldBe(2);
            result2.Errors[1].Message.ShouldBe(testMessage);

        }

        [Fact]
        public void ShouldHave404NotFoundError()
        {
            // Arrange
            string testMessage = "TestMessage2";

            // Act
            var result = ResultExtensions.Create404NotFoundResult(testMessage).LogError();
            var result2 = Result.Fail("").Add404NotFoundError(testMessage);

            // Assert
            result.Has404NotFoundError().ShouldBeTrue();
            result.Has400BadRequestError().ShouldBeFalse();
            result.Errors.Count.ShouldBe(1);
            result.Errors[0].Message.ShouldBe(testMessage);

            result2.Has404NotFoundError().ShouldBeTrue();
            result2.Has400BadRequestError().ShouldBeFalse();
            result2.Errors.Count.ShouldBe(2);
            result2.Errors[1].Message.ShouldBe(testMessage);

        }
    }
}