using FastEndpoints;
using PlexApi.Contracts;

namespace PlexRipper.PlexApi;

public class GetAllMediaEpisodesCommandHandler
    : ICommandHandler<GetAllMediaEpisodesCommand, Result<List<PlexTvShowEpisode>>>
{
    private readonly ICommandDispatch _commandDispatch;

    public GetAllMediaEpisodesCommandHandler(ICommandDispatch commandDispatch)
    {
        _commandDispatch = commandDispatch;
    }

    public async Task<Result<List<PlexTvShowEpisode>>> ExecuteAsync(
        GetAllMediaEpisodesCommand command,
        CancellationToken ct
    )
    {
        var plexLibrary = command.PlexLibrary;
        var action = command.Action;

        var mediaListResult = await _commandDispatch.ExecuteAsync(
            new GetAllMediaByTypeFromPlexApiCommand(plexLibrary, PlexMediaType.Episode, Action: action),
            ct
        );

        if (mediaListResult.IsFailed)
            return mediaListResult.ToResult();

        var mediaList = mediaListResult.Value.ToPlexTvShowEpisodes();
        return Result.Ok(mediaList);
    }
}
