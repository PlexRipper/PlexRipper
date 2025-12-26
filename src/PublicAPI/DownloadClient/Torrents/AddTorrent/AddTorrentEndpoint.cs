using BencodeNET.Exceptions;
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

public class TorrentMetadataDTOValidator : Validator<TorrentMetadataDTO>
{
    public TorrentMetadataDTOValidator()
    {
        RuleFor(x => x.LibraryId).GreaterThan(0).WithMessage("LibraryId must be greater than 0.");

        RuleFor(x => x.ServerId).GreaterThan(0).WithMessage("ServerId must be greater than 0.");

        RuleFor(x => x.MediaId).GreaterThan(0).WithMessage("MediaId must be greater than 0.");

        RuleFor(x => x.DataId).GreaterThan(0).WithMessage("DataId must be greater than 0.");

        RuleFor(x => x.PartId).GreaterThan(0).WithMessage("PartId must be greater than 0.");

        RuleFor(x => x.PartPlexId).GreaterThan(0).WithMessage("PartPlexId must be greater than 0.");

        RuleFor(x => x.Type).IsInEnum().WithMessage("Type must be a valid PlexMediaType value.");

        RuleFor(x => x.Quality).IsInEnum().WithMessage("Quality must be a valid VideoQuality value.");
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
        PreProcessor<DownloadClientAuthenticationPreProcessor<AddTorrentEndpointRequest>>();
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

        var parser = new BencodeParser();
        Torrent torrent;
        TorrentMetadataDTO metadata;
        string hashId;
        try
        {
            torrent = parser.Parse<Torrent>(req.TorrentFile!.OpenReadStream());
            metadata = torrent.ExtraFields.ToTorrentMetadataDTO();
            hashId = torrent.GetInfoHash();
        }
        catch (FormatException ex)
        {
            _log.Here()
                .Warning(
                    "[Torrent/Add] Invalid torrent file format for {TorrentFileName}: {Error}",
                    req.TorrentFile?.FileName,
                    ex.Message
                );
            AddError("torrents", "Invalid torrent file.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        catch (BencodeException ex)
        {
            _log.Here()
                .Warning(
                    "[Torrent/Add] Invalid torrent file bencode for {TorrentFileName}: {Error}",
                    req.TorrentFile?.FileName,
                    ex.Message
                );
            AddError("torrents", "Invalid torrent file.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        catch (Exception ex)
        {
            _log.Here()
                .Warning(
                    "[Torrent/Add] Failed to parse torrent file for {TorrentFileName}: {Error}",
                    req.TorrentFile?.FileName,
                    ex.Message
                );
            AddError("torrents", "Invalid torrent file.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        // Ensure this is a valid Reaparr torrent file
        var validationResult = await new TorrentMetadataDTOValidator().ValidateAsync(metadata, ct);
        if (!validationResult.IsValid)
        {
            _log.Here()
                .Error(
                    "[Torrent/Add] Invalid torrent metadata for file {TorrentFileName}, Errors: {Errors}",
                    req.TorrentFile.FileName,
                    validationResult.Errors
                );

            foreach (var error in validationResult.Errors)
                AddError(error.PropertyName, error.ErrorMessage);

            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        _log.Here()
            .Debug(
                "[Torrent/Add] Uploaded torrent file: {TorrentFileName}, {Size} bytes, MetaData={MetaData}",
                req.TorrentFile.FileName,
                torrent.File.FileSize,
                metadata
            );

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
                .Error(
                    "[Torrent/Add] Failed to create download tasks for torrent {TorrentFileName}, Error: {Error}",
                    req.TorrentFile.FileName,
                    createResult.Errors
                );
            await Send.StringAsync("Fail.", cancellation: ct);
            return;
        }

        // Set the hashId on the created download tasks so Sonarr/Radarr can keep track
        await SetHashIdOnDownloadTask(metadata, hashId);

        await Send.StringAsync("Ok.", cancellation: ct);
    }

    private async Task SetHashIdOnDownloadTask(TorrentMetadataDTO metaData, string hashId)
    {
        var count = 0;
        switch (metaData.Type)
        {
            case PlexMediaType.Episode:
                count = await _dbContext
                    .DownloadTaskTvShowEpisodeFile.Where(x =>
                        x.PlexLibraryId == metaData.LibraryId
                        && x.PlexServerId == metaData.ServerId
                        && x.PlexId == metaData.PartPlexId
                    )
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.HashId, hashId));
                break;
            case PlexMediaType.Movie:
                count = await _dbContext
                    .DownloadTaskMovieFile.Where(x =>
                        x.PlexLibraryId == metaData.LibraryId
                        && x.PlexServerId == metaData.ServerId
                        && x.PlexId == metaData.PartPlexId
                    )
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.HashId, hashId));
                break;
            default:
                _log.Here()
                    .Error(
                        "Unsupported PlexMediaType {PlexMediaType} for setting HashId on DownloadTask",
                        metaData.Type
                    );
                break;
        }

        if (count == 0)
            _log.Warning(
                "Could not find any DownloadTask to set HashId for torrent with MetaData: {MetaData}",
                metaData
            );
        else
            _log.Debug("Set HashId on {Count} DownloadTasks for torrent with MetaData: {MetaData}", count, metaData);
    }
}
