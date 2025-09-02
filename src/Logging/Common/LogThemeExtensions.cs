using Serilog.Sinks.Console.LogThemes;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog.Templates.Themes;

namespace Reaparr.Logging;

public static class LogThemeExtensions
{
    public static TemplateTheme ToTemplateTheme(this SystemConsoleTheme consoleTheme)
    {
        IReadOnlyDictionary<TemplateThemeStyle, string> mapped = consoleTheme.Styles.ToDictionary(
            x => x.Key.ToTemplate(),
            x => x.Value.ToAnsi()
        );

        return new TemplateTheme(mapped);
    }

    public static TemplateThemeStyle ToTemplate(this ConsoleThemeStyle style)
    {
        return style switch
        {
            ConsoleThemeStyle.Text => TemplateThemeStyle.Text,
            ConsoleThemeStyle.SecondaryText => TemplateThemeStyle.SecondaryText,
            ConsoleThemeStyle.TertiaryText => TemplateThemeStyle.TertiaryText,
            ConsoleThemeStyle.Invalid => TemplateThemeStyle.Invalid,
            ConsoleThemeStyle.Null => TemplateThemeStyle.Null,
            ConsoleThemeStyle.Name => TemplateThemeStyle.Name,
            ConsoleThemeStyle.String => TemplateThemeStyle.String,
            ConsoleThemeStyle.Number => TemplateThemeStyle.Number,
            ConsoleThemeStyle.Boolean => TemplateThemeStyle.Boolean,
            ConsoleThemeStyle.Scalar => TemplateThemeStyle.Scalar,
            ConsoleThemeStyle.LevelVerbose => TemplateThemeStyle.LevelVerbose,
            ConsoleThemeStyle.LevelDebug => TemplateThemeStyle.LevelDebug,
            ConsoleThemeStyle.LevelInformation => TemplateThemeStyle.LevelInformation,
            ConsoleThemeStyle.LevelWarning => TemplateThemeStyle.LevelWarning,
            ConsoleThemeStyle.LevelError => TemplateThemeStyle.LevelError,
            ConsoleThemeStyle.LevelFatal => TemplateThemeStyle.LevelFatal,
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };
    }

    public static ConsoleThemeStyle ToConsole(this TemplateThemeStyle style)
    {
        return style switch
        {
            TemplateThemeStyle.Text => ConsoleThemeStyle.Text,
            TemplateThemeStyle.SecondaryText => ConsoleThemeStyle.SecondaryText,
            TemplateThemeStyle.TertiaryText => ConsoleThemeStyle.TertiaryText,
            TemplateThemeStyle.Invalid => ConsoleThemeStyle.Invalid,
            TemplateThemeStyle.Null => ConsoleThemeStyle.Null,
            TemplateThemeStyle.Name => ConsoleThemeStyle.Name,
            TemplateThemeStyle.String => ConsoleThemeStyle.String,
            TemplateThemeStyle.Number => ConsoleThemeStyle.Number,
            TemplateThemeStyle.Boolean => ConsoleThemeStyle.Boolean,
            TemplateThemeStyle.Scalar => ConsoleThemeStyle.Scalar,
            TemplateThemeStyle.LevelVerbose => ConsoleThemeStyle.LevelVerbose,
            TemplateThemeStyle.LevelDebug => ConsoleThemeStyle.LevelDebug,
            TemplateThemeStyle.LevelInformation => ConsoleThemeStyle.LevelInformation,
            TemplateThemeStyle.LevelWarning => ConsoleThemeStyle.LevelWarning,
            TemplateThemeStyle.LevelError => ConsoleThemeStyle.LevelError,
            TemplateThemeStyle.LevelFatal => ConsoleThemeStyle.LevelFatal,
            _ => throw new ArgumentOutOfRangeException(nameof(style), style, null),
        };
    }
}
