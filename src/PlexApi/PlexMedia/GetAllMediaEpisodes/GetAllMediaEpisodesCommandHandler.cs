using FastEndpoints;
using Reaparr.PlexApi.Contracts;

namespace Reaparr.PlexApi;

public class GetAllMediaEpisodesCommandHandler
    : ICommandHandler<GetAllMediaEpisodesCommand, Result<List<PlexTvShowEpisode>>>
{
    private readonly ICommandExecutor _commandExecutor;

    public GetAllMediaEpisodesCommandHandler(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public async Task<Result<List<PlexTvShowEpisode>>> ExecuteAsync(
        GetAllMediaEpisodesCommand command,
        CancellationToken ct
    )
    {
        var plexLibrary = command.PlexLibrary;
        var action = command.Action;

        var mediaListResult = await _commandExecutor.Send(
            new GetAllMediaByTypeFromPlexApiCommand(plexLibrary, PlexMediaType.Episode, Action: action),
            ct
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowEpisodes();
        return Result.Ok(mediaList);
    }
}
