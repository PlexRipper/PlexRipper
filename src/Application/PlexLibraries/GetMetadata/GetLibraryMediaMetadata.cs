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
            var plexLibrary = await _dbContext
                .PlexLibraries.AsNoTracking()
                .Include(x => x.Roles)
                .Include(x => x.Countries)
                .Include(x => x.Genres)
                .GetAsync(req.PlexLibraryId, ct);

            await _dbContext
                .PlexCountries.Where(x => x.PlexLibraries.Any(y => y.Id == req.PlexLibraryId))
                .ToListAsync(cancellationToken: ct);

            if (plexLibrary is null)
            {
                await SendFluentResult(ResultExtensions.EntityNotFound(nameof(PlexLibrary), req.PlexLibraryId), ct);
                return;
            }

            await SendFluentResult(Result.Ok(plexLibrary), x => x.ToMetaDataDTO(), ct);
        }
        else
        {
            var uniqueCountries = await _dbContext
                .PlexCountries.Where(c =>
                    _dbContext.PlexLibraries.Any(pl => pl.Type == req.MediaType && pl.Countries.Contains(c))
                )
                .Distinct()
                .ToListAsync(ct);

            var uniqueGenres = await _dbContext
                .PlexGenres.Where(g =>
                    _dbContext.PlexLibraries.Any(pl => pl.Type == req.MediaType && pl.Genres.Contains(g))
                )
                .Distinct()
                .ToListAsync(ct);

            var uniqueRoles = await _dbContext
                .PlexRoles.Where(r =>
                    _dbContext.PlexLibraries.Any(pl => pl.Type == req.MediaType && pl.Roles.Contains(r))
                )
                .Distinct()
                .ToListAsync(ct);

            await SendFluentResult(
                Result.Ok(
                    new PlexMediaMetadataDTO
                    {
                        Roles = uniqueRoles.ToDTO(),
                        Countries = uniqueCountries.ToDTO(),
                        Genres = uniqueGenres.ToDTO(),
                    }
                ),
                ct
            );
        }
    }
}
