using FluentAssertions;
using FluentResults;
using PlexRipper.BaseTests;
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
            result.Has400BadRequestError().Should().BeTrue();
            result.Has404NotFoundError().Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].Message.Should().Be(testMessage);

            result2.Has400BadRequestError().Should().BeTrue();
            result2.Has404NotFoundError().Should().BeFalse();
            result2.Errors.Count.Should().Be(2);
            result2.Errors[1].Message.Should().Be(testMessage);

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
            result.Has404NotFoundError().Should().BeTrue();
            result.Has400BadRequestError().Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].Message.Should().Be(testMessage);

            result2.Has404NotFoundError().Should().BeTrue();
            result2.Has400BadRequestError().Should().BeFalse();
            result2.Errors.Count.Should().Be(2);
            result2.Errors[1].Message.Should().Be(testMessage);

        }
    }
}