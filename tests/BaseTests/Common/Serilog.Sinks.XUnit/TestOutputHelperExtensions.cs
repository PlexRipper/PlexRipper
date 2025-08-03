using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;

namespace PlexRipper.BaseTests
{
    /// <summary>
    /// Provides extension methods that create Serilog loggers
    /// directly from <see cref="ITestOutputHelper"/> objects.
    /// </summary>
    public static class TestOutputHelperExtensions
    {
        /// <summary>
        /// Initializes a new Serilog logger that writes to xunit's <paramref name="testOutputHelper"/>.
        /// </summary>
        /// <param name="testOutputHelper">The <see cref="ITestOutputHelper"/> that will be written to.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for
        /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="outputTemplate">A message template describing the format used to write to the sink.
        /// the default is "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}".</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <returns>The Serilog logger that logs to <paramref name="testOutputHelper"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="testOutputHelper"/> is null.</exception>
        public static Logger CreateTestLogger(
            this ITestOutputHelper testOutputHelper,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string outputTemplate = TestOutputLoggerConfigurationExtensions.DefaultConsoleOutputTemplate,
            IFormatProvider? formatProvider = null,
            LoggingLevelSwitch? levelSwitch = null
        )
        {
            return new LoggerConfiguration()
                .WriteTo.TestOutput(
                    testOutputHelper,
                    restrictedToMinimumLevel,
                    outputTemplate,
                    formatProvider,
                    levelSwitch
                )
                .CreateLogger();
        }

        /// <summary>
        /// Initializes a new Serilog logger that writes to xunit's <paramref name="testOutputHelper"/>.
        /// </summary>
        /// <param name="testOutputHelper">The <see cref="ITestOutputHelper"/> that will be written to.</param>
        /// <param name="formatter">Controls the rendering of log events into text, for example to log JSON. To
        /// control plain text formatting, use the overload that accepts an output template.</param>
        /// <param name="restrictedToMinimumLevel">The minimum level for
        /// events passed through the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level
        /// to be changed at runtime.</param>
        /// <returns>The Serilog logger that logs to <paramref name="testOutputHelper"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="testOutputHelper"/>
        /// or <paramref name="formatter"/> is null.</exception>
        public static Logger CreateTestLogger(
            this ITestOutputHelper testOutputHelper,
            ITextFormatter formatter,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch? levelSwitch = null
        )
        {
            return new LoggerConfiguration()
                .WriteTo.TestOutput(testOutputHelper, formatter, restrictedToMinimumLevel, levelSwitch)
                .CreateLogger();
        }
    }
}
