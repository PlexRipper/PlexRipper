using System.Security.Cryptography;
using System.Text;
using Flurl;
using Reaparr.Application.Contracts;

namespace Reaparr.PublicAPI;

public sealed record TorznabFeedItemProjection
{
    public required PlexMediaType MediaType { get; init; }
    public required int MediaId { get; init; }
    public required int DataId { get; init; }
    public required int PlexServerId { get; init; }
    public required string PlexServerMachineIdentifier { get; init; }
    public required int PlexLibraryId { get; init; }
    public required int PlexApiRatingKey { get; init; }
    public required int PlexApiMediaId { get; init; }
    public required int PlexApiPartId { get; init; }
    public required string Title { get; init; }
    public required DateTime AddedAt { get; init; }
    public required long Size { get; init; }
    public required VideoQuality Quality { get; init; }
    public required VideoQuality VideoResolution { get; init; }
    public required ReleaseSource Source { get; init; }
    public required string VideoCodec { get; init; }
    public required string AudioCodec { get; init; }
    public int? SeasonNumber { get; init; }
    public int? EpisodeNumber { get; init; }
    public int? TvdbId { get; init; }
    public int? TmdbId { get; init; }
    public string? ImdbId { get; init; }

    public string CreateStableId()
    {
        var identity = string.Join(
            ':',
            PlexServerMachineIdentifier,
            MediaType,
            PlexApiRatingKey,
            PlexApiMediaId,
            PlexApiPartId
        );
        return "reaparr-" + Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(identity)));
    }

    public TorznabItem ToTorznabItem(IntegrationIdentity integration, string torznabApiKey, string baseUrl)
    {
        var torrentMetadata = new TorrentMetadataDTO
        {
            Type = MediaType,
            MediaId = MediaId,
            DataId = DataId,
            PartId = DataId,
            PlexApiPartId = PlexApiPartId,
            Quality = Quality,
            LibraryId = PlexLibraryId,
            ServerId = PlexServerId,
        };
        string downloadUrl = baseUrl
            .AppendPathSegment(
                PublicApiRoutes.DownloadTorrent.Replace("{integrationId:guid}", integration.Id.ToString())
            )
            .SetQueryParams(torrentMetadata.Values)
            .SetQueryParam(IntegrationDefinitions.INDEXER_API_KEY, torznabApiKey, isEncoded: false);
        var category =
            MediaType == PlexMediaType.Movie ? this.ToTorznabMovieCategory() : this.ToTorznabEpisodeCategory();
        var item = new TorznabItem
        {
            Title = Title,
            PubDate = AddedAt.ToUniversalTime().ToString("R"),
            Guid = new TorznabGuid { Value = CreateStableId() },
            Link = downloadUrl,
            Size = Size,
            Enclosure = new TorznabEnclosure
            {
                Url = downloadUrl,
                Length = Size,
                Type = "application/x-bittorrent",
            },
        };

        item.Attributes.Add(new TorznabAttr("size", Size.ToString()));
        item.Attributes.Add(new TorznabAttr("category", category.ToString()));
        item.Attributes.Add(new TorznabAttr("seeders", "1"));
        item.Attributes.Add(new TorznabAttr("peers", "1"));
        item.Attributes.Add(new TorznabAttr("type", MediaType == PlexMediaType.Movie ? "movie" : "series"));
        item.Attributes.Add(new TorznabAttr("language", "English"));
        item.Attributes.Add(new TorznabAttr("downloadvolumefactor", "0.0"));
        item.Attributes.Add(new TorznabAttr("resolution", VideoResolution.ToResolutionLabel()));
        item.Attributes.Add(new TorznabAttr("source", Source.ToEnumMemberValue()));
        item.Attributes.Add(new TorznabAttr("videoCodec", VideoCodec));
        item.Attributes.Add(new TorznabAttr("audioCodec", AudioCodec));

        AddOptionalAttribute(item, "season", SeasonNumber);
        AddOptionalAttribute(item, "episode", EpisodeNumber);
        AddOptionalAttribute(item, "tvdbid", TvdbId);
        AddOptionalAttribute(item, "tmdbid", TmdbId);

        if (!string.IsNullOrEmpty(ImdbId))
            item.Attributes.Add(new TorznabAttr("imdb", ImdbId));

        return item;
    }

    private static void AddOptionalAttribute(TorznabItem item, string name, int? value)
    {
        if (value.HasValue)
            item.Attributes.Add(new TorznabAttr(name, value.Value.ToString()));
    }
}
