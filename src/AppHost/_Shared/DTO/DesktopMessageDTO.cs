namespace Reaparr.AppHost;

/// <summary>
/// Message sent from the front-end to the back-end in Desktop mode. This is for opening external links etc
/// </summary>
public record DesktopMessageDTO
{
    /// <summary>
    /// The type of the <see cref="DesktopMessageDTO"/>
    /// </summary>
    public required DesktopMessageType Type { get; init; }

    /// <summary>
    /// The value of the <see cref="DesktopMessageDTO"/>
    /// </summary>
    public required string Value { get; init; }
}
