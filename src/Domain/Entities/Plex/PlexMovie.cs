using System.Collections.Generic;

namespace PlexRipper.Domain
{
    public class PlexMovie : PlexMedia
    {
        #region Properties

        public List<PlexMovieGenre> PlexMovieGenres { get; set; }
        public List<PlexMovieRole> PlexMovieRoles { get; set; }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the absolute url of the thumb image, which requires a <see cref="PlexLibrary"/> and <see cref="PlexServer"/> navigation property.
        /// Result is empty if invalid.
        /// </summary>
        public string ThumbUrl => PlexLibrary?.PlexServer?.BaseUrl + Thumb ?? string.Empty;

        #endregion
    }
}