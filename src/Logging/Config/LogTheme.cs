using Serilog.Sinks.SystemConsole.Themes;

namespace Logging;

public static class LogTheme
{
    public static SystemConsoleTheme ColoredDark { get; } = new(
        new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
        {
            [ConsoleThemeStyle.Text] = new() { Foreground = ConsoleColor.Gray },
            [ConsoleThemeStyle.SecondaryText] = new() { Foreground = ConsoleColor.DarkGray },
            [ConsoleThemeStyle.TertiaryText] = new() { Foreground = ConsoleColor.DarkGray },
            [ConsoleThemeStyle.Invalid] = new() { Foreground = ConsoleColor.Yellow },
            [ConsoleThemeStyle.Null] = new() { Foreground = ConsoleColor.White },
            [ConsoleThemeStyle.Name] = new() { Foreground = ConsoleColor.White },
            [ConsoleThemeStyle.String] = new() { Foreground = ConsoleColor.White },
            [ConsoleThemeStyle.Number] = new() { Foreground = ConsoleColor.White },
            [ConsoleThemeStyle.Boolean] = new() { Foreground = ConsoleColor.White },
            [ConsoleThemeStyle.Scalar] = new() { Foreground = ConsoleColor.White },

            // Log Level coloring
            [ConsoleThemeStyle.LevelVerbose] = new() { Foreground = ConsoleColor.Gray, Background = ConsoleColor.DarkGray },
            [ConsoleThemeStyle.LevelDebug] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.DarkGray },
            [ConsoleThemeStyle.LevelInformation] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Blue },
            [ConsoleThemeStyle.LevelWarning] = new() { Foreground = ConsoleColor.DarkGray, Background = ConsoleColor.Yellow },
            [ConsoleThemeStyle.LevelError] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
            [ConsoleThemeStyle.LevelFatal] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
        });
}