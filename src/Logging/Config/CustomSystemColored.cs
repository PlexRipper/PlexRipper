using Serilog.Sinks.Console.LogThemes;
using Serilog.Sinks.SystemConsole.Themes;

namespace Logging;

public class CustomSystemColored : ColoredSystemThemeTemplate
{
    protected override SystemConsoleThemeStyle TertiaryText { get; } = LogSystemTheme.Style(ConsoleColor.Magenta);
    protected override SystemConsoleThemeStyle Text => LogSystemTheme.Foreground(ConsoleColor.Black);
    protected override SystemConsoleThemeStyle SecondaryText => LogSystemTheme.Foreground(ConsoleColor.Gray);
}
