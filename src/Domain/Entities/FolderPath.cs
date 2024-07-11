using System.ComponentModel.DataAnnotations.Schema;

// ReSharper disable CollectionNeverUpdated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace PlexRipper.Domain;

public class FolderPath : BaseEntity
{
    #region Constructor

    public FolderPath() { }

    public FolderPath(FolderType folderType, string displayName, string directoryPath)
    {
        FolderType = folderType;
        DisplayName = displayName;
        DirectoryPath = directoryPath;
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets or sets the display name of this <see cref="FolderPath"/>.
    /// </summary>
    [Column(Order = 1)]
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="FolderType"/> of this <see cref="FolderPath"/>.
    /// </summary>
    [Column(Order = 2)]
    public FolderType FolderType { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="PlexMediaType"/> associated with this <see cref="FolderPath"/>.
    /// </summary>
    [Column(Order = 3)]
    public PlexMediaType MediaType { get; set; }

    /// <summary>
    ///  Gets or sets the filesystem directory path of this <see cref="FolderPath"/>.
    /// </summary>
    [Column(Order = 4)]
    public string DirectoryPath { get; set; }

    #endregion

    #region Relationships

    /// <summary>
    /// Gets or sets the <see cref="PlexLibraries"/> that use this <see cref="FolderPath"/> as a download destination folder.
    /// </summary>
    public List<PlexLibrary> PlexLibraries { get; set; }

    #endregion

    #region Helpers

    /// <summary>
    /// Checks if the <see cref="FolderPath"/> is valid by checking if the directory exists.
    /// </summary>
    /// <returns>If the check is successful.</returns>
    public bool IsValid => Directory.Exists(DirectoryPath);

    #endregion
}
