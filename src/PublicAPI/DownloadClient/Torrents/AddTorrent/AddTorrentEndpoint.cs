using BencodeNET.Parsing;
using BencodeNET.Torrents;
using FastEndpoints;
using FluentValidation;
using Reaparr.Application.Contracts;

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
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger _log;

    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/torrents/add");
        Description(x => x.IsDownloadClient());
        AllowFileUploads();
        AllowAnonymous();
    }

    public AddTorrentEndpoint(ILogger logger, ICommandExecutor commandExecutor)
    {
        _log = logger.ForContext<AddTorrentEndpoint>();
        _commandExecutor = commandExecutor;
    }

    public override async Task HandleAsync(AddTorrentEndpointRequest req, CancellationToken ct)
    {
        _log.Here().DebugApiCall(HttpContext, req);

        if (req.TorrentFile is null)
            return;

        var parser = new BencodeParser();
        var torrent = parser.Parse<Torrent>(req.TorrentFile.OpenReadStream());

        var metadata = torrent.ExtraFields.ToTorrentMetadataDTO();

        _log.Here()
            .Debug(
                "[Torrent/Add] Uploaded torrent file: {FileName}, {Size} bytes, MetaData={MetaData}",
                req.TorrentFile.FileName, torrent.File.FileSize, metadata);

        List<DownloadMediaDTO> list =
        [
            new()
            {
                Qualities =
                [
                    new PlexMediaQualityDTO
                    {
                        MediaDataType = metadata.Type,
                        MediaId = metadata.MediaId,
                        DataId = metadata.DataId,
                        Quality = metadata.Quality,
                    },
                ],
                MediaIds = [metadata.MediaId],
                Type = metadata.Type,
                PlexServerId = metadata.ServerId,
                PlexLibraryId = metadata.LibraryId,
            },
        ];
        var createResult = await _commandExecutor.Send(new CreateDownloadTasksCommand(list), ct);
        if (createResult.IsFailed)
        {
            _log.Here()
                .Error("[Torrent/Add] Failed to create download tasks for torrent {FileName}, Error: {Error}",
                    req.TorrentFile.FileName, createResult.Errors);
            await Send.StringAsync("Fail.", cancellation: ct);
            return;
        }

        await Send.StringAsync("Ok.", cancellation: ct);
    }
}