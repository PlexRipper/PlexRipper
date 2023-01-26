using System.Drawing;

namespace Logging.Theme;

public static class ThemeExtensions
{
    public static string ToAnsiString(this FormatType formatType)
    {
        return formatType switch
        {
            FormatType.None => "0",
            FormatType.BoldMode => "1",
            FormatType.DimFaintMode => "2",
            FormatType.ItalicMode => "3",
            FormatType.UnderlineMode => "4",
            FormatType.BlinkingMode => "5",
            FormatType.InverseReverseMode => "7",
            FormatType.HiddenMode => "8",
            FormatType.Strikethrough => "9",
            _ => "0",
        };
    }

    public static string ToAnsiString(this Color? color, ColorLayerEnum colorLayerEnum)
    {
        if (color is null)
            return "";

        var colorString = "";

        if (colorLayerEnum == ColorLayerEnum.ForegroundColor)
            colorString += "38;";

        if (colorLayerEnum == ColorLayerEnum.BackgroundColor)
            colorString += "48;";

        return colorString + $"2;{color.Value.R};{color.Value.G};{color.Value.B}";
    }
}