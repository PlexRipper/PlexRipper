using FastEndpoints;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi.GetAllMediaSeasons;

public class GetAllSeasonsCommandHandler : ICommandHandler<GetAllMediaSeasonsCommand, Result<List<PlexTvShowSeason>>>
{
    private readonly ICommandExecutor _commandExecutor;

    public GetAllSeasonsCommandHandler(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<List<PlexTvShowSeason>>> ExecuteAsync(
        GetAllMediaSeasonsCommand command,
        CancellationToken ct
    )
    {
        var plexLibrary = command.PlexLibrary;
        var action = command.Action;

        var mediaListResult = await _commandExecutor.Send(
            new GetAllMediaByTypeFromPlexApiCommand(plexLibrary, PlexMediaType.Season, Action: action),
            ct
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowSeasons();
        return Result.Ok(mediaList);
    }
}
