using System.Text.RegularExpressions;
using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public static class PlexMediaMapper
{
    public static PlexMovie ToPlexMovie(this PlexMedia source, LibraryMediaItemDTO originalSource) =>
        new()
        {
            Id = source.Id,
            Key = source.Key,
            Title = source.Title,
            Year = source.Year,
            SortIndex = source.SortIndex,
            SearchTitle = source.SearchTitle,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            MetaDataKey = source.MetaDataKey,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            HasThumb = source.HasThumb,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            MediaData = source.MediaData,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            FullBannerUrl = source.FullBannerUrl,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            FullTitle = $"{originalSource.Title} ({originalSource.Year})",
            PlexLibrary = source.PlexLibrary,
            PlexServer = source.PlexServer,
            Type = source.Type,
            Guid = source.Guid,
            Guid_IMDB = source.Guid_IMDB,
            Guid_TMDB = source.Guid_TMDB,
            Guid_TVDB = source.Guid_TVDB,
            Country =
                originalSource.Country?.Select(x => new PlexCountry { Name = x.Tag ?? "", PlexKey = 0 }).ToList() ?? [],
            Roles =
                originalSource
                    .Role?.Select(x => new PlexRole
                    {
                        Name = x.Tag ?? "",
                        PlexKey = 0,
                        ThumbnailUrl = "",
                    })
                    .ToList() ?? [],
            Genres =
                originalSource.Genre?.Select(x => new PlexGenre() { Name = x.Tag ?? "", PlexKey = 0 }).ToList() ?? [],
        };

    public static PlexTvShow ToPlexTvShow(this PlexMedia source, LibraryMediaItemDTO originalSource) =>
        new()
        {
            Id = source.Id,
            Key = source.Key,
            Title = source.Title,
            Year = source.Year,
            SortIndex = source.SortIndex,
            SearchTitle = source.SearchTitle,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            MetaDataKey = source.MetaDataKey,
            ChildCount = source.ChildCount,
            GrandChildCount = 0, // This is set later on in BuildTvShowTree
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            HasThumb = source.HasThumb,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            MediaData = source.MediaData,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            FullBannerUrl = source.FullBannerUrl,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            FullTitle = originalSource.Title,
            PlexLibrary = source.PlexLibrary,
            PlexServer = source.PlexServer,
            Type = source.Type,
            Guid = source.Guid,
            Guid_IMDB = source.Guid_IMDB,
            Guid_TMDB = source.Guid_TMDB,
            Guid_TVDB = source.Guid_TVDB,
            Country =
                originalSource.Country?.Select(x => new PlexCountry { Name = x.Tag ?? "", PlexKey = 0 }).ToList() ?? [],
            Roles =
                originalSource
                    .Role?.Select(x => new PlexRole
                    {
                        Name = x.Tag ?? "",
                        PlexKey = 0,
                        ThumbnailUrl = "",
                    })
                    .ToList() ?? [],
            Genres =
                originalSource.Genre?.Select(x => new PlexGenre() { Name = x.Tag ?? "", PlexKey = 0 }).ToList() ?? [],
        };

    public static PlexTvShowSeason ToPlexTvShowSeason(this PlexMedia source, LibraryMediaItemDTO originalSource) =>
        new()
        {
            Id = source.Id,
            Key = source.Key,
            Title = source.Title,
            Year = source.Year,
            SortIndex = source.SortIndex,
            SearchTitle = source.SearchTitle,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            MetaDataKey = source.MetaDataKey,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            HasThumb = source.HasThumb,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            MediaData = source.MediaData,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            FullBannerUrl = source.FullBannerUrl,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            ParentKey = originalSource.GetParentKey(),
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            FullTitle = $"{originalSource.ParentTitle}/{originalSource.Title}",
            PlexLibrary = source.PlexLibrary,
            PlexServer = source.PlexServer,
            Type = source.Type,
            Guid = source.Guid,
            Guid_IMDB = source.Guid_IMDB,
            Guid_TMDB = source.Guid_TMDB,
            Guid_TVDB = source.Guid_TVDB,
            ParentGuid = originalSource.ParentGuid,
        };

    public static PlexTvShowEpisode ToPlexTvShowEpisode(this PlexMedia source, LibraryMediaItemDTO originalSource) =>
        new()
        {
            Id = source.Id,
            Key = source.Key,
            Title = source.Title,
            Year = source.Year,
            SortIndex = source.SortIndex,
            SearchTitle = source.SearchTitle,
            Duration = source.Duration,
            MediaSize = source.MediaSize,
            MetaDataKey = source.MetaDataKey,
            ChildCount = source.ChildCount,
            AddedAt = source.AddedAt,
            UpdatedAt = source.UpdatedAt,
            HasThumb = source.HasThumb,
            HasArt = source.HasArt,
            HasTheme = source.HasTheme,
            MediaData = source.MediaData,
            PlexLibraryId = source.PlexLibraryId,
            PlexServerId = source.PlexServerId,
            FullBannerUrl = source.FullBannerUrl,
            Studio = source.Studio,
            Summary = source.Summary,
            ContentRating = source.ContentRating,
            Rating = source.Rating,
            OriginallyAvailableAt = source.OriginallyAvailableAt,
            FullTitle = $"{originalSource.GrandparentTitle}/{originalSource.ParentTitle}/{originalSource.Title}",
            PlexLibrary = source.PlexLibrary,
            PlexServer = source.PlexServer,
            ParentKey = originalSource.GetParentKey(),
            Type = source.Type,
            Guid = source.Guid,
            Guid_IMDB = source.Guid_IMDB,
            Guid_TMDB = source.Guid_TMDB,
            Guid_TVDB = source.Guid_TVDB,
            ParentGuid = originalSource.ParentGuid,
        };

    /// <summary>
    /// The PlexAPI is sometimes missing the ParentKey, this method will attempt to get the ParentKey from the ParentGuid.
    /// </summary>
    /// <param name="originalSource"> The original source to get the ParentKey from.</param>
    private static int GetParentKey(this LibraryMediaItemDTO originalSource)
    {
        var parentKey = originalSource.ParentRatingKey != null ? int.Parse(originalSource.ParentRatingKey) : -1;
        if (parentKey == -1 && originalSource.ParentGuid != null && originalSource.ParentGuid.Contains("local"))
        {
            // Replace all non-numeric characters
            var result = Regex.Replace(originalSource.ParentGuid, "[^0-9]", "");
            if (!int.TryParse(result, out parentKey))
            {
                parentKey = -1;
            }
        }

        return parentKey;
    }
}
