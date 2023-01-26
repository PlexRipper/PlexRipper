using System.Drawing;

namespace Logging.Theme;

public class LogStyle
{
    private Color? _foreground = null;

    private Color? _background = null;

    private FormatType _formatType = FormatType.None;

    public LogStyle()
    {

    }

    public LogStyle(Color? foreground, Color? background)
    {
        _foreground = foreground;
        _background = background;
    }

    public LogStyle(Color? foreground, Color? background, FormatType formatType)
    {
        _foreground = foreground;
        _background = background;
        _formatType = formatType;
    }

    public LogStyle SetForeground(Color? foreground)
    {
        _foreground = foreground;
        return this;
    }

    public LogStyle SetBackground(Color? background)
    {
        _background = background;
        return this;
    }

    public LogStyle SetFormatType(FormatType formatType)
    {
        _formatType = formatType;
        return this;
    }

    public string ToStyle()
    {
        return LogTheme.Style(_foreground, _background, _formatType);
    }
}