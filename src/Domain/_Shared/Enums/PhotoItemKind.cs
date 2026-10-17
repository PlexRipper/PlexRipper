namespace Reaparr.Domain;

/// <summary>
/// Identifies a photo browse item and the corresponding expansion selection.
/// The numeric values are shared by both contracts.
/// </summary>
public enum PhotoItemKind
{
    /// <summary>
    /// A photo folder.
    /// </summary>
    Folder = 0,

    /// <summary>
    /// A metadata photo album.
    /// </summary>
    Album = 1,

    /// <summary>
    /// A photo asset.
    /// </summary>
    Asset = 2,
}
