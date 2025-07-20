using LukeHagar.PlexAPI.SDK.Models.Requests;
using PlexApi.Contracts;
using PlexRipper.PlexApi;

namespace PlexRipper.BaseTests;

public static class GetAllLibrariesDirectoryMappers
{
    public static GetAllLibrariesDirectory ToPlexApiDTO(this PlexLibrary source) =>
        new()
        {
            AllowSync = false,
            Art = string.Empty,
            Composite = string.Empty,
            Filters = false,
            Refreshing = false,
            Thumb = string.Empty,
            Key = source.Key,
            Type = source.Type.ToGetAllLibrariesType(),
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

    public static List<GetAllLibrariesDirectory> ToPlexApiDTO(this List<PlexLibrary> plexLibraries) =>
        plexLibraries.Select(x => x.ToPlexApiDTO()).ToList();
}
