using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

public record GetPlexMediaDataByLibraryIdQuery(int LibraryId, int Page = 0, int PageSize = 0) : IRequest<Result<List<PlexMediaSlim>>>;

public class GetPlexMediaDataByLibraryIdQueryValidator : AbstractValidator<GetPlexMediaDataByLibraryIdQuery>
{
    public GetPlexMediaDataByLibraryIdQueryValidator()
    {
        RuleFor(x => x.LibraryId).GreaterThan(0);
    }
}

public class GetPlexMediaDataByLibraryIdQueryHandler : IRequestHandler<GetPlexMediaDataByLibraryIdQuery, Result<List<PlexMediaSlim>>>
{
    private readonly ILog _log;
    private readonly IMapper _mapper;
    private readonly IPlexRipperDbContext _dbContext;

    public GetPlexMediaDataByLibraryIdQueryHandler(ILog log, IMapper mapper, IPlexRipperDbContext dbContext)
    {
        _log = log;
        _mapper = mapper;
        _dbContext = dbContext;
    }

    public async Task<Result<List<PlexMediaSlim>>> Handle(GetPlexMediaDataByLibraryIdQuery request, CancellationToken cancellationToken)
    {
        var plexLibrary = await _dbContext.PlexLibraries.AsNoTracking().Include(x => x.PlexServer).GetAsync(request.LibraryId, cancellationToken);

        // When 0, just take everything
        var take = request.PageSize <= 0 ? -1 : request.PageSize;
        var skip = request.Page * request.PageSize;

        List<PlexMediaSlim> entities;
        switch (plexLibrary.Type)
        {
            case PlexMediaType.Movie:
            {
                entities = await _dbContext.PlexMovies.AsNoTracking()
                    .Where(x => x.PlexLibraryId == request.LibraryId)
                    .OrderBy(x => x.Title)
                    .Skip(skip)
                    .Take(take)
                    .ProjectTo<PlexMediaSlim>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                break;
            }
            case PlexMediaType.TvShow:
            {
                entities = await _dbContext.PlexTvShows.AsNoTracking()
                    .Where(x => x.PlexLibraryId == request.LibraryId)
                    .OrderBy(x => x.Title)
                    .Skip(skip)
                    .Take(take)
                    .ProjectTo<PlexMediaSlim>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                break;
            }
            case PlexMediaType.Season:
            {
                entities = await _dbContext.PlexTvShowSeason.AsNoTracking()
                    .Where(x => x.PlexLibraryId == request.LibraryId)
                    .OrderBy(x => x.Title)
                    .Skip(skip)
                    .Take(take)
                    .ProjectTo<PlexMediaSlim>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                break;
            }
            case PlexMediaType.Episode:
            {
                entities = await _dbContext.PlexTvShowEpisodes.AsNoTracking()
                    .Where(x => x.PlexLibraryId == request.LibraryId)
                    .OrderBy(x => x.Title)
                    .Skip(skip)
                    .Take(take)
                    .ProjectTo<PlexMediaSlim>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                break;
            }
            default:
                return Result.Fail($"Type {plexLibrary.Type} is not supported for retrieving the PlexMedia data by library id");
        }

        if (!entities.Any())
        {
            return ResultExtensions.IsEmptyQueryResult(new List<PlexMediaSlim>(), nameof(GetPlexMediaDataByLibraryIdQuery), nameof(PlexLibrary),
                request.LibraryId);
        }

        var plexServerId = plexLibrary.PlexServerId;

        var plexServerConnection = await _dbContext.GetValidPlexServerConnection(plexServerId, cancellationToken);
        if (plexServerConnection.IsFailed)
            return plexServerConnection.ToResult();

        var connection = plexServerConnection.Value;
        var plexServerToken = await _dbContext.GetPlexServerTokenAsync(plexServerId, cancellationToken);

        foreach (var mediaSlim in entities)
            mediaSlim.FullThumbUrl = GetThumbnailUrl(connection.Url, mediaSlim.ThumbUrl, plexServerToken.Value);

        return Result.Ok(entities);
    }

    private string GetThumbnailUrl(string connectionUrl, string thumbPath, string plexServerToken)
    {
        var uri = new Uri(connectionUrl + thumbPath);
        return $"{uri.Scheme}://{uri.Host}:{uri.Port}/photo/:/transcode?url={uri.AbsolutePath}&X-Plex-Token={plexServerToken}";
    }
}