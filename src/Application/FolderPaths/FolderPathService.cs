using Application.Contracts;
using Data.Contracts;
using FileSystem.Contracts;

namespace PlexRipper.Application;

public class FolderPathService : IFolderPathService
{
    private readonly IMediator _mediator;
    private readonly IPlexRipperDbContext _dbContext;

    private readonly IDirectorySystem _directorySystem;

    public FolderPathService(IMediator mediator, IPlexRipperDbContext dbContext, IDirectorySystem directorySystem)
    {
        _mediator = mediator;
        _dbContext = dbContext;
        _directorySystem = directorySystem;
    }

    /// <inheritdoc/>
    public async Task<Result<Dictionary<PlexMediaType, FolderPath>>> GetDefaultDestinationFolderDictionary()
    {
        var folderPaths = await _mediator.Send(new GetAllFolderPathsQuery());
        if (folderPaths.IsFailed)
            return folderPaths.ToResult();

        var dict = new Dictionary<PlexMediaType, FolderPath>
        {
            { PlexMediaType.Movie, folderPaths.Value.FirstOrDefault(x => x.Id == 2) },
            { PlexMediaType.TvShow, folderPaths.Value.FirstOrDefault(x => x.Id == 3) },
            { PlexMediaType.Season, folderPaths.Value.FirstOrDefault(x => x.Id == 3) },
            { PlexMediaType.Episode, folderPaths.Value.FirstOrDefault(x => x.Id == 3) },
            { PlexMediaType.Music, folderPaths.Value.FirstOrDefault(x => x.Id == 4) },
            { PlexMediaType.Album, folderPaths.Value.FirstOrDefault(x => x.Id == 4) },
            { PlexMediaType.Song, folderPaths.Value.FirstOrDefault(x => x.Id == 4) },
            { PlexMediaType.Photos, folderPaths.Value.FirstOrDefault(x => x.Id == 5) },
            { PlexMediaType.OtherVideos, folderPaths.Value.FirstOrDefault(x => x.Id == 6) },
            { PlexMediaType.Games, folderPaths.Value.FirstOrDefault(x => x.Id == 7) },
            { PlexMediaType.None, folderPaths.Value.FirstOrDefault(x => x.Id == 1) },
            { PlexMediaType.Unknown, folderPaths.Value.FirstOrDefault(x => x.Id == 1) },
        };

        return Result.Ok(dict);
    }
}