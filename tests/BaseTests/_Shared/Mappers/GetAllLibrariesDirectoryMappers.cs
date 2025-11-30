using LukeHagar.PlexAPI.SDK.Models.Components;
using Reaparr.PlexApi;

namespace Reaparr.BaseTests;

public static class LibrarySectionMappers
{
    public static LibrarySection ToPlexApiDTO(this PlexLibrary source) =>
        new()
        {
            AllowSync = false,
            Art = string.Empty,
            Composite = string.Empty,
            Filters = false,
            Refreshing = false,
            Thumb = string.Empty,
            Key = source.Key,
            Type = source.Type.ToMediaType(),
            Title = source.Title,
            Agent = string.Empty,
            Scanner = string.Empty,
            Language = source.Language,
            Uuid = source.Uuid,
            UpdatedAt = source.UpdatedAt.ToUnixLong(),
            CreatedAt = source.CreatedAt.ToUnixLong(),
            ScannedAt = source.ScannedAt.ToUnixLong(),
            Content = false,
            Directory = false,
            ContentChangedAt = 0,
            Hidden = null,
            Location = [],
        };

    public static List<LibrarySection> ToPlexApiDTO(this List<PlexLibrary> plexLibraries) =>
        plexLibraries.Select(x => x.ToPlexApiDTO()).ToList();
}
