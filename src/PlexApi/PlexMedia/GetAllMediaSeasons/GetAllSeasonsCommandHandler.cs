using FastEndpoints;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi.GetAllMediaSeasons;

public class GetAllSeasonsCommandHandler : ICommandHandler<GetAllMediaSeasonsCommand, Result<List<PlexTvShowSeason>>>
{
    private readonly IPlexApiMediaService _plexApiMediaService;

    public GetAllSeasonsCommandHandler(IPlexApiMediaService plexApiMediaService)
    {
        _plexApiMediaService = plexApiMediaService;
    }

    public async Task<Result<List<PlexTvShowSeason>>> ExecuteAsync(
        GetAllMediaSeasonsCommand command,
        CancellationToken ct
    )
    {
        var plexLibrary = command.PlexLibrary;
        var action = command.Action;

        var mediaListResult = await _plexApiMediaService.SyncMedia(
            plexLibrary,
            PlexMediaType.Season,
            action: action,
            cancellationToken: ct
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowSeasons();
        return Result.Ok(mediaList);
    }
}
