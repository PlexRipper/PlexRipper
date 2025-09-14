using BencodeNET.Parsing;
using BencodeNET.Torrents;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

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
    private readonly IReaparrDbContext _dbContext;
    private readonly ICommandExecutor _commandExecutor;
    private readonly ILogger _log;

    public override void Configure()
    {
        Post(PublicApiRoutes.DownloadClient + "/torrents/add");
        Description(x => x.IsDownloadClient());
        AllowFileUploads();
        AllowAnonymous();
    }

    public AddTorrentEndpoint(ILogger logger, IReaparrDbContext dbContext, ICommandExecutor commandExecutor)
    {
        _log = logger.ForContext<AddTorrentEndpoint>();
        _dbContext = dbContext;
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
        var hashId = torrent.GetInfoHash();

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

        // Set the hashId on the created download tasks so Sonarr/Radarr can keep track
        await SetHashIdOnDownloadTask(metadata, hashId);

        await Send.StringAsync("Ok.", cancellation: ct);
    }

    private async Task SetHashIdOnDownloadTask(TorrentMetadataDTO metaData, string hashId)
    {
        switch (metaData.Type)
        {
            case PlexMediaType.Episode:
                await _dbContext.DownloadTaskTvShowEpisodeFile.Where(x => x.PlexLibraryId == metaData.LibraryId && 
                                                                          x.PlexServerId == metaData.ServerId &&
                                                                          x.PlexId == metaData.PartPlexId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.HashId, hashId));
                break;
            case PlexMediaType.Movie:
                await _dbContext.DownloadTaskMovieFile.Where(x => x.PlexLibraryId == metaData.LibraryId && 
                                                                  x.PlexServerId == metaData.ServerId &&
                                                                  x.PlexId == metaData.PartPlexId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.HashId, hashId));
                break;
            default:
                _log.Here().Error("Unsupported PlexMediaType {PlexMediaType} for setting HashId on DownloadTask", metaData.Type);
                break;
        }
    }
}