using System.ComponentModel;
using FastEndpoints;

namespace PlexRipper.Application;

public abstract record PlexMediaFilterQueryRequest
{
    /// <summary>
    /// NOTE: This constructor is needed to make the query param optional in the front-end typescript-api generation.
    /// </summary>
    protected PlexMediaFilterQueryRequest(
        int page = 0,
        int size = 0,
        int countryId = 0,
        int roleId = 0,
        int genreId = 0,
        bool filterOfflineMedia = false,
        bool filterOwnedMedia = false
    )
    {
        Page = page;
        Size = size;
        CountryId = countryId;
        RoleId = roleId;
        GenreId = genreId;
        FilterOfflineMedia = filterOfflineMedia;
        FilterOwnedMedia = filterOwnedMedia;
    }

    [QueryParam, BindFrom("page")]
    [DefaultValue(0)]
    public int Page { get; init; }

    [QueryParam, BindFrom("size")]
    [DefaultValue(0)]
    public int Size { get; init; }

    [QueryParam, BindFrom("countryId")]
    [DefaultValue(0)]
    public int CountryId { get; init; }

    [QueryParam, BindFrom("genreId")]
    [DefaultValue(0)]
    public int GenreId { get; init; }

    [QueryParam, BindFrom("roleId")]
    [DefaultValue(0)]
    public int RoleId { get; init; }

    [QueryParam, BindFrom("filterOfflineMedia")]
    [DefaultValue(false)]
    public bool FilterOfflineMedia { get; init; }

    [QueryParam, BindFrom("filterOwnedMedia")]
    [DefaultValue(false)]
    public bool FilterOwnedMedia { get; init; }
}
