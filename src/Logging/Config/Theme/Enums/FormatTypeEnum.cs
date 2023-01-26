namespace Logging;

public enum FormatType
{
    None = 0,

    /// <summary>
    /// Works,
    /// </summary>
    BoldMode = 1,

    /// <summary>
    /// Works, text is faint
    /// </summary>
    DimFaintMode = 2,

    /// <summary>
    /// Works,
    /// </summary>
    ItalicMode = 3,

    /// <summary>
    /// Works, the underline is the same color as the foreground.
    /// </summary>
    UnderlineMode = 4,

    /// <summary>
    /// Not working
    /// </summary>
    BlinkingMode = 5,

    /// <summary>
    /// Works, uses the foreground as the background color and vice versa
    /// </summary>
    InverseReverseMode = 6,

    /// <summary>
    /// Not working
    /// </summary>
    HiddenMode = 7,

    /// <summary>
    /// Not working
    /// </summary>
    Strikethrough = 8,
}