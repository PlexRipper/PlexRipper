using System.Collections.Generic;
using System.Linq;
using FluentResults;
using PlexRipper.BaseTests;
using Serilog.Events;
using Serilog.Sinks.TestCorrelator;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace FluentResultExtensionTests.Logging
{
    public class ResultExtensionsLoggingTests
    {
        public ResultExtensionsLoggingTests(ITestOutputHelper output)
        {
            BaseDependanciesTest.SetupLogging(output);
        }

        #region Result

        [Fact]
        public void ShouldHaveVerboseLoggedWithParameterName_WhenLogVerboseIsCalled()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.IsNull(parameterName).LogVerbose();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Verbose);
            }
        }

        [Fact]
        public void ShouldHaveDebugLoggedWithParameterName_WhenLogDebugIsCalled()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.IsNull(parameterName).LogDebug();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Debug);
            }
        }

        [Fact]
        public void ShouldHaveInformationLoggedWithParameterName_WhenLogInformationIsCalled()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.IsNull(parameterName).LogInformation();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Information);
            }
        }

        [Fact]
        public void ShouldHaveWarningLoggedWithParameterName_WhenLogWarningIsCalled()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.IsNull(parameterName).LogWarning();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Warning);
            }
        }

        [Fact]
        public void ShouldHaveErrorLoggedWithParameterName_WhenLogErrorIsCalled()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.IsNull(parameterName).LogError();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Error);
            }
        }

        [Fact]
        public void ShouldHaveFatalLoggedWithParameterName_WhenLogFatalIsCalled()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.IsNull(parameterName).LogFatal();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Fatal);
            }
        }

        #endregion

        #region Result<T>

        [Fact]
        public void ShouldHaveVerboseLoggedWithParameterName_WhenLogVerboseIsCalledOnResultT()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.Create201CreatedResult(100, parameterName).LogVerbose();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Verbose);
            }
        }

        [Fact]
        public void ShouldHaveDebugLoggedWithParameterName_WhenLogDebugIsCalledOnResultT()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.Create201CreatedResult(100, parameterName).LogDebug();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Debug);
            }
        }

        [Fact]
        public void ShouldHaveInformationLoggedWithParameterName_WhenLogInformationIsCalledOnResultT()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.Create201CreatedResult(100, parameterName).LogInformation();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Information);
            }
        }

        [Fact]
        public void ShouldHaveWarningLoggedWithParameterName_WhenLogWarningIsCalledOnResultT()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.Create201CreatedResult(100, parameterName).LogWarning();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Warning);
            }
        }

        [Fact]
        public void ShouldHaveErrorLoggedWithParameterName_WhenLogErrorIsCalledOnResultT()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.Create201CreatedResult(100, parameterName).LogError();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Error);
            }
        }

        [Fact]
        public void ShouldHaveFatalLoggedWithParameterName_WhenLogFatalIsCalledOnResultT()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var parameterName = "ParameterXYZ";

                // Act
                ResultExtensions.Create201CreatedResult(100, parameterName).LogFatal();

                // Assert
                var logEvent = TestCorrelator.GetLogEventsFromCurrentContext().First();
                logEvent.MessageTemplate.Text.ShouldContain(parameterName);
                logEvent.Level.ShouldBe(LogEventLevel.Fatal);
            }
        }

        #endregion

        #region LogReasons

        [Fact]
        public void ShouldHaveMultipleErrorsLogged_WhenResultHasMultipleErrors()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var result = Result.Fail("Test Error #1").WithError("Test Error #2").WithError("Test Error #3");

                // Act
                result.LogError();

                // Assert
                var logContext = TestCorrelator.GetLogEventsFromCurrentContext().ToList();

                for (int i = 0; i < logContext.Count; i++)
                {
                    logContext[i].Level.ShouldBe(LogEventLevel.Error);
                    logContext[i].MessageTemplate.Text.ShouldBe($"Test Error #{i + 1}");
                }
            }
        }

        [Fact]
        public void ShouldHaveMultipleNestedErrorsLogged_WhenResultHasMultipleNestedErrors()
        {
            using (TestCorrelator.CreateContext())
            {
                // Arrange
                var result = Result.Fail("Test Error #1");

                result.AddNestedErrors(
                    new List<Error>
                    {
                        new Error("Error #1"),
                        new Error("Error #2"),
                        new Error("Error #3"),
                        new Error("Error #4"),
                        new Error("Error #5"),
                    });

                // Act
                result.LogError();

                // Assert
                var logContext = TestCorrelator.GetLogEventsFromCurrentContext().ToList();

                logContext.First().Level.ShouldBe(LogEventLevel.Error);
                logContext.First().MessageTemplate.Text.ShouldBe("Test Error #1");

                for (int i = 1; i < logContext.Count; i++)
                {
                    logContext[i].Level.ShouldBe(LogEventLevel.Error);
                    logContext[i].MessageTemplate.Text.ShouldBe($"--Error #{i}");
                }
            }
        }

        #endregion
    }
}