using BencodeNET.Parsing;
using BencodeNET.Torrents;
using FastEndpoints;
using FluentValidation;

namespace Reaparr.PublicAPI;

public record AddTorrentEndpointRequest
{
    [FormField, BindFrom("urls")]
    public string? Urls { get; init; }

    [FormField, BindFrom("category")]
    public string? Category { get; init; }

    [FormField, BindFrom("paused")]
    public bool? Paused { get; init; }

    /// <summary>
    /// Single torrent file (binary upload).
    /// </summary>
    [FormField, BindFrom("torrents")]
    public IFormFile? TorrentFile { get; init; }
}

public class AddTorrentEndpointRequestValidator : Validator<AddTorrentEndpointRequest>
{
    public AddTorrentEndpointRequestValidator()
    {
        RuleFor(x => x.TorrentFile).NotNull().NotEmpty().WithMessage("A Reaparr torrent file must be provided.");
    }
}

public class AddTorrentEndpoint : Endpoint<AddTorrentEndpointRequest>
{
    private readonly ILogger _log;

    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/torrents/add");
        Description(x => x.IsDownloadClient());
        AllowFileUploads();
        AllowAnonymous();
    }

    public AddTorrentEndpoint(ILogger logger)
    {
        _log = logger.ForContext<AddTorrentEndpoint>();
    }

    public override async Task HandleAsync(AddTorrentEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        if (req.TorrentFile is null)
            return;

        var parser = new BencodeParser();
        var torrent = parser.Parse<Torrent>(req.TorrentFile.OpenReadStream());

        var guid = torrent.ExtraFields?.TryGetValue("reaparr-guid", out var guidVal) == true
            ? guidVal.ToString()
            : null;

        _log.Here()
            .Debug(
                "[Torrent/Add] Uploaded torrent file: {FileName}, {Size} bytes, ReaparrGuid={Guid}",
                req.TorrentFile.FileName, torrent.File.FileSize, guid ?? "<none>");

        await Send.StringAsync("Ok.", cancellation: ct);
    }
}