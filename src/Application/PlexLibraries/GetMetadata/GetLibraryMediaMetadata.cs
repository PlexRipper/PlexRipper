using System.ComponentModel;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Reaparr.Application.Contracts;
using Reaparr.Data.Contracts;

namespace Reaparr.Application;

public record GetLibraryMediaMetadataRequest
{
    public int PlexLibraryId { get; init; }

    [QueryParam, BindFrom("mediaType")]
    [DefaultValue(PlexMediaType.None)]
    public PlexMediaType MediaType { get; init; }
}

public class GetLibraryMediaMetadataRequestValidator : Validator<GetLibraryMediaMetadataRequest>
{
    public GetLibraryMediaMetadataRequestValidator()
    {
        RuleFor(x => x.MediaType)
            .NotEqual(PlexMediaType.None)
            .When(x => x.PlexLibraryId == 0)
            .WithMessage("MediaType must not be 'None' when PlexLibraryId is 0.");
    }
}

public class GetLibraryMediaMetadata : BaseEndpoint<GetLibraryMediaMetadataRequest, PlexMediaMetadataDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexLibraryController + "/{PlexLibraryId}/metadata";

    public GetLibraryMediaMetadata(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaMetadataDTO>))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetLibraryMediaMetadataRequest req, CancellationToken ct)
    {
        if (req.PlexLibraryId > 0)
        {
            // First, verify the library exists
            var plexLibrary = await _dbContext.PlexLibraries.AsNoTracking().GetAsync(req.PlexLibraryId, ct);

            if (plexLibrary is null)
            {
                await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
                return;
            }

            // Get actual data efficiently using joins
            var roles = await (
                from la in _dbContext.PlexLibraryActors.AsNoTracking()
                join a in _dbContext.PlexActors.AsNoTracking() on la.PlexActorId equals a.Id
                where la.PlexLibraryId == req.PlexLibraryId
                select new PlexRoleDTO { Id = a.Id, Name = a.Name }
            ).ToListAsync(ct);

            var countries = await (
                from lc in _dbContext.PlexLibraryCountries.AsNoTracking()
                join c in _dbContext.PlexCountries.AsNoTracking() on lc.PlexCountryId equals c.Id
                where lc.PlexLibraryId == req.PlexLibraryId
                select new PlexCountryDTO { Id = c.Id, Name = c.Name }
            ).ToListAsync(ct);

            var genres = await (
                from lg in _dbContext.PlexLibraryGenres.AsNoTracking()
                join g in _dbContext.PlexGenres.AsNoTracking() on lg.PlexGenreId equals g.Id
                where lg.PlexLibraryId == req.PlexLibraryId
                select new PlexGenreDTO { Id = g.Id, Name = g.Name }
            ).ToListAsync(ct);

            var mediaMetadataDTO = new PlexMediaMetadataDTO
            {
                Roles = roles,
                Countries = countries,
                Genres = genres,
                RoleCount = plexLibrary.ActorsCount,
                CountryCount = plexLibrary.CountriesCount,
                GenreCount = plexLibrary.GenresCount,
            };

            await SendFluentResult(Result.Ok(mediaMetadataDTO), ct);
        }
        else
        {
            // Get counts efficiently for global metadata
            var roleCount = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(pl => pl.Type == req.MediaType)
                .SelectMany(pl => pl.Actors)
                .Select(a => a.Id)
                .Distinct()
                .CountAsync(ct);

            var countryCount = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(pl => pl.Type == req.MediaType)
                .SelectMany(pl => pl.Countries)
                .Select(c => c.Id)
                .Distinct()
                .CountAsync(ct);

            var genreCount = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(pl => pl.Type == req.MediaType)
                .SelectMany(pl => pl.Genres)
                .Select(g => g.Id)
                .Distinct()
                .CountAsync(ct);

            // Get actual unique data efficiently
            var uniqueRoles = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(pl => pl.Type == req.MediaType)
                .SelectMany(pl => pl.Actors)
                .Select(a => new PlexRoleDTO { Id = a.Id, Name = a.Name })
                .Distinct()
                .ToListAsync(ct);

            var uniqueCountries = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(pl => pl.Type == req.MediaType)
                .SelectMany(pl => pl.Countries)
                .Select(c => new PlexCountryDTO { Id = c.Id, Name = c.Name })
                .Distinct()
                .ToListAsync(ct);

            var uniqueGenres = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(pl => pl.Type == req.MediaType)
                .SelectMany(pl => pl.Genres)
                .Select(g => new PlexGenreDTO { Id = g.Id, Name = g.Name })
                .Distinct()
                .ToListAsync(ct);

            await SendFluentResult(
                Result.Ok(
                    new PlexMediaMetadataDTO
                    {
                        Roles = uniqueRoles,
                        Countries = uniqueCountries,
                        Genres = uniqueGenres,
                        RoleCount = roleCount,
                        CountryCount = countryCount,
                        GenreCount = genreCount,
                    }
                ),
                ct
            );
        }
    }
}
