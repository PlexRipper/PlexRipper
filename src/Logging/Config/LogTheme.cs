using System.Drawing;
using System.Text;
using Logging.Theme;

namespace Logging;

public static class LogTheme
{
    #region FluentApi

    #region Create

    public static LogStyle Foreground(Color foreGroundColor)
    {
        return new LogStyle().SetForeground(foreGroundColor);
    }

    public static LogStyle Background(Color backGroundColor)
    {
        return new LogStyle().SetBackground(backGroundColor);
    }

    public static LogStyle FormatType(FormatType formatType)
    {
        return new LogStyle().SetFormatType(formatType);
    }

    #endregion

    #region Extend

    public static LogStyle Foreground(this LogStyle logStyle, Color foreGroundColor)
    {
        return logStyle.SetForeground(foreGroundColor);
    }

    public static LogStyle Background(this LogStyle logStyle, Color backGroundColor)
    {
        return logStyle.SetBackground(backGroundColor);
    }

    public static LogStyle FormatType(this LogStyle logStyle, FormatType formatType)
    {
        return logStyle.SetFormatType(formatType);
    }

    #endregion



    #endregion

    #region Style

    public static string Style(Color? foreground, Color? background, FormatType formatType = Logging.FormatType.None)
    {
        var builder = new StringBuilder("\x1b["); // ESC character

        if (formatType is not Logging.FormatType.None)
            builder.Append(formatType.ToAnsiString() + ";"); // Formatting style, bold, italic etc

        if (foreground is not null)
            builder.Append(foreground.ToAnsiString(ColorLayerEnum.ForegroundColor)); // Font color

        if (foreground is not null && background is not null)
            builder.Append(";");

        if (foreground is not null)
            builder.Append(background.ToAnsiString(ColorLayerEnum.BackgroundColor)); // Font background color
        builder.Append("m"); // exit command
        return builder.ToString();
    }

    public static string Style(Color? foreground, FormatType formatType)
    {
        return Style(foreground, null, formatType);
    }

    public static string Style(Color? foreground)
    {
        return Style(foreground, null);
    }

    public static string Style()
    {
        return "";
    }

    #endregion
}