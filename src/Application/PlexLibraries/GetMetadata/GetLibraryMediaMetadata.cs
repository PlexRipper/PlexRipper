using System.ComponentModel;
using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace PlexRipper.Application;

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
            var mediaMetadataDTO = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(x => x.Id == req.PlexLibraryId)
                .Select(x => new PlexMediaMetadataDTO
                {
                    Roles = x.Actors.ToDTO(),
                    Countries = x.Countries.ToDTO(),
                    Genres = x.Genres.ToDTO(),
                    RoleCount = x.Actors.Count,
                    CountryCount = x.Countries.Count,
                    GenreCount = x.Genres.Count,
                })
                .FirstOrDefaultAsync(ct);

            if (mediaMetadataDTO is null)
            {
                await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
                return;
            }

            await SendFluentResult(Result.Ok(mediaMetadataDTO), ct);
        }
        else
        {
            var uniqueRoles = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(pl => pl.Type == req.MediaType)
                .SelectMany(pl => pl.Actors.Select(g => g.ToDTO()))
                .Distinct()
                .ToListAsync(ct);

            var uniqueCountries = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(pl => pl.Type == req.MediaType)
                .SelectMany(pl => pl.Countries.Select(g => g.ToDTO()))
                .Distinct()
                .ToListAsync(ct);

            var uniqueGenres = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Where(pl => pl.Type == req.MediaType)
                .SelectMany(pl => pl.Genres.Select(g => g.ToDTO()))
                .Distinct()
                .ToListAsync(ct);

            await SendFluentResult(
                Result.Ok(
                    new PlexMediaMetadataDTO
                    {
                        Roles = uniqueRoles,
                        Countries = uniqueCountries,
                        Genres = uniqueGenres,
                        RoleCount = uniqueRoles.Count,
                        CountryCount = uniqueCountries.Count,
                        GenreCount = uniqueGenres.Count,
                    }
                ),
                ct
            );
        }
    }
}
