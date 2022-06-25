﻿using System.IO;
using System.Linq;
using Autofac.Extras.Moq;
using Logging;
using PlexRipper.FileSystem;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace FileSystem.UnitTests
{
    public class PathSystem_SanitizeFileName_UnitTests
    {
        public PathSystem_SanitizeFileName_UnitTests(ITestOutputHelper output)
        {
            Log.SetupTestLogging(output);
        }

        [Theory]
        [InlineData("Shaun het Schaap: De Film (2015)")]
        [InlineData("RANDOM MOVIE: # · GREAT")]
        public void ShouldFilterAllInvalidCharsFromName_WhenGivenInvalidName(string testString)
        {
            // Arrange
            using var mock = AutoMock.GetStrict();
            var _sut = mock.Create<PathSystem>();

            // Act
            var result = _sut.SanitizePath(testString);

            // Assert
            result.ShouldNotBeNullOrEmpty();
            result.ShouldNotContain("  ");
            var invalidChars = Path.GetInvalidFileNameChars();
            result.ShouldAllBe(c => !invalidChars.Contains(c));
        }
    }
}