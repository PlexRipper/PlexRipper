using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    /// <summary>
    /// Plex stores media in 1 generic type but PlexRipper stores it by type, this is the base entity for common properties.
    /// </summary>
    public class PlexMedia : BaseEntity
    {
        public int RatingKey { get; set; }
        public string Key { get; set; }
        public string Guid { get; set; }
        public string Studio { get; set; }
        public string Title { get; set; }
        public string ContentRating { get; set; }
        public string Summary { get; set; }
        public int Index { get; set; }
        public double Rating { get; set; }
        public int Year { get; set; }
        public string Thumb { get; set; }
        public string Art { get; set; }
        public string Banner { get; set; }
        public int Duration { get; set; }
        public DateTime OriginallyAvailableAt { get; set; }
        public int LeafCount { get; set; }
        public int ViewedLeafCount { get; set; }
        public int ChildCount { get; set; }
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }


        public int? ViewCount { get; set; }
        public DateTime? LastViewedAt { get; set; }
        public string Theme { get; set; }

        #region Relationships

        public virtual PlexLibrary PlexLibrary { get; set; }
        public int PlexLibraryId { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public string MetaDataUrl => $"{PlexLibrary.PlexServer.BaseUrl}{Key}";


        /// <summary>
        /// Gets the absolute url of the thumb image, which requires a <see cref="PlexLibrary"/> and <see cref="PlexServer"/> navigation property.
        /// Result is empty if invalid.
        /// </summary>
        [NotMapped]
        public string ThumbUrl => PlexLibrary?.PlexServer?.BaseUrl + Thumb ?? string.Empty;

        #endregion
    }
}