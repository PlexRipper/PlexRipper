using PlexRipper.Domain.Entities.JoinTables;
using System;
using System.Collections.Generic;

namespace PlexRipper.Domain.Entities
{
    public class PlexMovies : BaseEntity
    {

        public int RatingKey { get; set; }
        public string Key { get; set; }
        public string Guid { get; set; }
        public string Studio { get; set; }
        public string Type { get; set; }
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
        public virtual List<PlexMovieGenre> PlexMovieGenres { get; set; }
        public virtual List<PlexMovieRole> PlexMovieRoles { get; set; }

        public int? ViewCount { get; set; }
        public DateTime? LastViewedAt { get; set; }
        public string Theme { get; set; }

        public virtual PlexLibrary PlexLibrary { get; set; }
        public int PlexLibraryId { get; set; }
    }
}
