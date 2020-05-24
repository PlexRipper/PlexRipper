using AutoMapper;
using System;

namespace PlexRipper.Domain.Entities
{
    public class PlexLibrary : BaseEntity
    {

        /// <summary>
        /// The Library Section Identifier used by Plex
        /// </summary>
        public string Key { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// Plex Library type, see: https://github.com/Arcanemagus/plex-api/wiki/MediaTypes
        /// </summary>
        public string Type { get; set; }

        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ScannedAt { get; set; }
        public DateTime ContentChangedAt { get; set; }

        public Guid Uuid { get; set; }

        /// <summary>
        /// This is the relative path Id of the Library location
        /// </summary>
        public int LibraryLocationId { get; set; }

        /// <summary>
        /// This is a relative path of the Library location, e.g: /AnimeSeries
        /// </summary>
        public string LibraryLocationPath { get; set; }



        /// <summary>
        /// The PlexServer this PlexLibrary belongs to
        /// </summary>
        [IgnoreMap]
        public virtual PlexServer PlexServer { get; set; }

        /// <summary>
        /// The PlexServerId of the PlexServer this PlexLibrary belongs to
        /// </summary>
        [IgnoreMap]
        public int PlexServerId { get; set; }

        // TODO Create a many-to-many relationship to determing which PlexAccounts have access to this PlexLibrary
        // public bool HasAccess { get; set; }
    }
}
