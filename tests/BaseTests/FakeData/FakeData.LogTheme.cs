using System.Drawing;
using Bogus;
using Bogus.DataSets;
using Logging.Theme;
using Serilog.Sinks.SystemConsole.Themes;

namespace PlexRipper.BaseTests;

public static partial class FakeData
{
    public static Faker<AnsiConsoleTheme> GetFakeTheme(int seed = 0)
    {
        return new Faker<AnsiConsoleTheme>()
            .UseSeed(seed)
            .CustomInstantiator(faker =>
            {
                var styles = GetFakeLogStyle(seed, true).Generate(16);
                var dict = new Dictionary<ConsoleThemeStyle, string>
                {
                    // Timestamp, classname, method name and line number
                    [ConsoleThemeStyle.SecondaryText] = styles[0].ToStyle(),

                    // Brackets, dots and colons
                    [ConsoleThemeStyle.TertiaryText] = styles[1].ToStyle(),

                    // Log message
                    [ConsoleThemeStyle.Text] = styles[2].ToStyle(),

                    [ConsoleThemeStyle.Invalid] = styles[3].ToStyle(),
                    [ConsoleThemeStyle.Null] = styles[4].ToStyle(),
                    [ConsoleThemeStyle.Name] = styles[5].ToStyle(),

                    // Log values
                    [ConsoleThemeStyle.String] = styles[6].ToStyle(),
                    [ConsoleThemeStyle.Number] = styles[7].ToStyle(),
                    [ConsoleThemeStyle.Boolean] = styles[8].ToStyle(),
                    [ConsoleThemeStyle.Scalar] = styles[9].ToStyle(),

                    // Log Level coloring
                    // [ConsoleThemeStyle.LevelVerbose] = styles[10].ToStyle(),
                    // [ConsoleThemeStyle.LevelDebug] = styles[11].ToStyle(),
                    // [ConsoleThemeStyle.LevelInformation] = styles[12].ToStyle(),
                    // [ConsoleThemeStyle.LevelWarning] = styles[13].ToStyle(),
                    // [ConsoleThemeStyle.LevelError] = styles[14].ToStyle(),
                    // [ConsoleThemeStyle.LevelFatal] = styles[15].ToStyle(),

                    // Log Level coloring
                    [ConsoleThemeStyle.LevelVerbose] = LogTheme.Style(Color.White, Color.DarkGray),
                    [ConsoleThemeStyle.LevelDebug] = LogTheme.Style(Color.White, Color.DarkGray),
                    [ConsoleThemeStyle.LevelInformation] = LogTheme.Style(Color.White, Color.FromArgb(23, 126, 137)),
                    [ConsoleThemeStyle.LevelWarning] = LogTheme.Style(Color.Black, Color.FromArgb(255, 200, 87)),
                    [ConsoleThemeStyle.LevelError] = LogTheme.Style(Color.White, Color.Red),
                    [ConsoleThemeStyle.LevelFatal] = LogTheme.Style(Color.White, Color.DarkRed),
                };
                return new AnsiConsoleTheme(dict);
            });
    }

    public static Faker<LogStyle> GetFakeLogStyle(int seed = 0, bool foreGround = false, bool background = false)
    {
        return new Faker<LogStyle>()
            .UseSeed(seed)
            .CustomInstantiator(faker =>
            {
                var style = new LogStyle();
                if (foreGround)
                {
                    var colorString = faker.Internet.Color(format: ColorFormat.Delimited).Split(',').Select(int.Parse).ToList();
                    style.SetForeground(Color.FromArgb(colorString[0], colorString[1], colorString[2]));
                }

                if (background)
                {
                    var colorString = faker.Internet.Color(format: ColorFormat.Delimited).Split(',').Select(int.Parse).ToList();
                    style.SetBackground(Color.FromArgb(colorString[0], colorString[1], colorString[2]));
                }

                var formatType = faker.Random.Enum<FormatType>();
                style.SetFormatType(formatType);
                return style;
            });
    }
}