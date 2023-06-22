using AutoMapper;
using AutoMapper.QueryableExtensions;
using Data.Contracts;
using FluentValidation;
using Logging.Interface;
using Microsoft.EntityFrameworkCore;
using PlexRipper.Data.Common;

namespace PlexRipper.Data.PlexMedia;

public class GetPlexMediaDataByLibraryIdQueryValidator : AbstractValidator<GetPlexMediaDataByLibraryIdQuery>
{
    public GetPlexMediaDataByLibraryIdQueryValidator()
    {
        RuleFor(x => x.LibraryId).GreaterThan(0);
    }
}

public class GetPlexMediaDataByLibraryIdQueryHandler : BaseHandler, IRequestHandler<GetPlexMediaDataByLibraryIdQuery, Result<List<PlexMediaSlim>>>
{
    private readonly IMapper _mapper;

    public GetPlexMediaDataByLibraryIdQueryHandler(ILog log, IMapper mapper, PlexRipperDbContext dbContext) : base(log, dbContext)
    {
        _mapper = mapper;
    }

    public async Task<Result<List<PlexMediaSlim>>> Handle(GetPlexMediaDataByLibraryIdQuery request, CancellationToken cancellationToken)
    {
        var plexLibrary = await _dbContext.PlexLibraries.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.LibraryId, cancellationToken);

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

        if (entities.Any())
            return Result.Ok(entities);

        return ResultExtensions.IsEmptyQueryResult(new List<PlexMediaSlim>(), nameof(GetPlexMediaDataByLibraryIdQuery), nameof(PlexLibraries), request.LibraryId);
    }
}