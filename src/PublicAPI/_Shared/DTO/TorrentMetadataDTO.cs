using FastEndpoints;
using Microsoft.AspNetCore.WebUtilities;
using Reaparr.Environment;

namespace Reaparr.PublicAPI;

public record TorrentMetadataDTO
{
    [QueryParam]
    public required PlexMediaType Type { get; init; }

    [QueryParam]
    public required int MediaId { get; init; }

    [QueryParam]
    public required int DataId { get; init; }

    [QueryParam]
    public required VideoQuality Quality { get; init; }

    [QueryParam]
    public required int LibraryId { get; init; }

    [QueryParam]
    public required int ServerId { get; init; }

    public string ToUrl()
    {
        var url = new UriBuilder()
        {
            Host = "localhost",
            Port = EnvironmentExtensions.GetPort,
            Path = PublicApiRoutes.DownloadTorrent,
        }.ToString();

        url = QueryHelpers.AddQueryString(url, Values!);

        return url;
    }

    public Dictionary<string, string> Values => new()
    {
        { nameof(Type), Type.ToString() },
        { nameof(MediaId), MediaId.ToString() },
        { nameof(DataId), DataId.ToString() },
        { nameof(Quality), Quality.ToString() },
        { nameof(LibraryId), LibraryId.ToString() },
        { nameof(ServerId), ServerId.ToString() },
    };
}