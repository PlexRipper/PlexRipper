using FastEndpoints;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public class GetAllMediaEpisodesCommandHandler
    : ICommandHandler<GetAllMediaEpisodesCommand, Result<List<PlexTvShowEpisode>>>
{
    private readonly IPlexApiMediaService _plexApiMediaService;

    public GetAllMediaEpisodesCommandHandler(IPlexApiMediaService plexApiMediaService)
    {
        _plexApiMediaService = plexApiMediaService;
    }

    public async Task<Result<List<PlexTvShowEpisode>>> ExecuteAsync(
        GetAllMediaEpisodesCommand command,
        CancellationToken ct
    )
    {
        var plexLibrary = command.PlexLibrary;
        var action = command.Action;

        var mediaListResult = await _plexApiMediaService.SyncMedia(
            plexLibrary,
            PlexMediaType.Episode,
            action: action,
            cancellationToken: ct
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowEpisodes();
        return Result.Ok(mediaList);
    }
}
