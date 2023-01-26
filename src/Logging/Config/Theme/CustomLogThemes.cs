using System.Drawing;
using Serilog.Sinks.SystemConsole.Themes;

namespace Logging.Theme;

public static class CustomLogThemes
{
    public static SystemConsoleTheme ColoredDark { get; } = new(
        new Dictionary<ConsoleThemeStyle, SystemConsoleThemeStyle>
        {
            // Timestamp, classname, method name and line number
            [ConsoleThemeStyle.SecondaryText] = new() { Foreground = ConsoleColor.Gray },

            // Brackets, dots and colons
            [ConsoleThemeStyle.TertiaryText] = new() { Foreground = ConsoleColor.Gray },

            // Log message
            [ConsoleThemeStyle.Text] = new() { Foreground = ConsoleColor.Black },

            [ConsoleThemeStyle.Invalid] = new() { Foreground = ConsoleColor.Yellow },
            [ConsoleThemeStyle.Null] = new() { Foreground = ConsoleColor.White },
            [ConsoleThemeStyle.Name] = new() { Foreground = ConsoleColor.White },

            // Log values
            [ConsoleThemeStyle.String] = new() { Foreground = ConsoleColor.DarkRed },
            [ConsoleThemeStyle.Number] = new() { Foreground = ConsoleColor.DarkGreen },
            [ConsoleThemeStyle.Boolean] = new() { Foreground = ConsoleColor.DarkYellow },
            [ConsoleThemeStyle.Scalar] = new() { Foreground = ConsoleColor.Black },

            // Log Level coloring
            [ConsoleThemeStyle.LevelVerbose] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.DarkGray },
            [ConsoleThemeStyle.LevelDebug] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.DarkGray },
            [ConsoleThemeStyle.LevelInformation] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Blue },
            [ConsoleThemeStyle.LevelWarning] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Yellow },
            [ConsoleThemeStyle.LevelError] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.Red },
            [ConsoleThemeStyle.LevelFatal] = new() { Foreground = ConsoleColor.White, Background = ConsoleColor.DarkRed },
        });

    public static AnsiConsoleTheme ColoredDarkAnsi { get; } = new(
        new Dictionary<ConsoleThemeStyle, string>
        {
            // Timestamp, classname, method name and line number
            [ConsoleThemeStyle.SecondaryText] = LogTheme
                .Foreground(Color.Gray)
                .FormatType(FormatType.ItalicMode)
                .ToStyle(),

            // Brackets, dots and colons
            [ConsoleThemeStyle.TertiaryText] = LogTheme.Style(Color.Gray),

            // Log message
            [ConsoleThemeStyle.Text] = LogTheme
                .Foreground(Color.LightGray)
                .ToStyle(),

            [ConsoleThemeStyle.Invalid] = LogTheme.Style(Color.Yellow),
            [ConsoleThemeStyle.Null] = LogTheme.Style(Color.LightGray),
            [ConsoleThemeStyle.Name] = LogTheme.Style(Color.White),

            // Log values
            [ConsoleThemeStyle.String] = LogTheme.Style(Color.DarkRed),
            [ConsoleThemeStyle.Number] = LogTheme.Style(Color.DarkGreen),
            [ConsoleThemeStyle.Boolean] = LogTheme.Style(Color.DarkKhaki),
            [ConsoleThemeStyle.Scalar] = LogTheme.Style(Color.Black),

            // Log Level coloring
            [ConsoleThemeStyle.LevelVerbose] = LogTheme.Style(Color.White, Color.DarkGray),
            [ConsoleThemeStyle.LevelDebug] = LogTheme.Style(Color.White, Color.DarkGray),
            [ConsoleThemeStyle.LevelInformation] = LogTheme.Style(Color.White, Color.FromArgb(23, 126, 137)),
            [ConsoleThemeStyle.LevelWarning] = LogTheme.Style(Color.Black, Color.FromArgb(255, 200, 87)),
            [ConsoleThemeStyle.LevelError] = LogTheme.Style(Color.White, Color.Red),
            [ConsoleThemeStyle.LevelFatal] = LogTheme.Style(Color.White, Color.DarkRed),
        });
}