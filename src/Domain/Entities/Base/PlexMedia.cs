using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlexRipper.Domain
{
    /// <summary>
    ///     Plex stores media in 1 generic type but PlexRipper stores it by type, this is the base entity for common
    ///     properties.
    /// </summary>
    public class PlexMedia : BaseEntity
    {
        #region Properties

        /// <summary>
        /// Unique key identifying this item by the Plex Api. This is used by the PlexServers to differentiate between media items.
        /// e.g: 28550, 1723, 21898.
        /// </summary>
        public int RatingKey { get; set; }

        /// <summary>
        /// Unique key identifying this item by the Plex Api. with the url path included
        /// e.g: "/library/metadata/9725", "/library/metadata/9724".
        /// TODO: This can possibly be removed from the database as it seems that every "Key" is always "/library/metadata/" + RatingKey.
        /// </summary>
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

        /// <summary>
        /// The total filesize of the nested media.
        /// </summary>
        public long MediaSize { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int? ViewCount { get; set; }

        public string Theme { get; set; }

        #endregion

        #region Relationships

        public PlexLibrary PlexLibrary { get; set; }

        public int PlexLibraryId { get; set; }

        #endregion

        #region Helpers

        [NotMapped]
        public string MetaDataUrl => $"{PlexLibrary.PlexServer.ServerUrl}{Key}";

        /// <summary>
        ///     Gets the absolute url of the thumb image, which requires a <see cref="PlexLibrary" /> and <see cref="PlexServer" />
        ///     navigation property.
        ///     Result is empty if invalid.
        /// </summary>
        [NotMapped]
        public string ThumbUrl => PlexLibrary?.PlexServer?.ServerUrl + Thumb ?? string.Empty;

        [NotMapped]
        public virtual PlexMediaType Type => PlexMediaType.None;

        /// <summary>
        /// The base <see cref="DownloadTask"/> used as a template to create DownloadTasks in child classes.
        /// </summary>
        /// <returns>The base <see cref="DownloadTask"/>.</returns>
        protected DownloadTask CreateBaseDownloadTask()
        {
            return new DownloadTask
            {
                PlexServer = PlexLibrary?.PlexServer,
                PlexServerId = PlexLibrary?.PlexServer?.Id ?? 0,
                PlexLibraryId = PlexLibraryId,
                Created = DateTime.Now,
                DownloadStatus = DownloadStatus.Initialized,
                RatingKey = RatingKey,
            };
        }

        #endregion
    }
}