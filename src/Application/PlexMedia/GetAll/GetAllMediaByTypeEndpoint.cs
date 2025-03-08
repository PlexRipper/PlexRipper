using System.ComponentModel;
using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public record GetAllMediaByTypeRequest : PlexMediaFilterQueryRequest
{
    /// <summary>
    /// NOTE: This constructor is needed to make the query param optional in the front-end typescript-api generation.
    /// </summary>
    public GetAllMediaByTypeRequest(
        PlexMediaType mediaType,
        int page,
        int size,
        bool filterOfflineMedia,
        bool filterOwnedMedia
    )
        : base(page: page, size: size, filterOfflineMedia: filterOfflineMedia, filterOwnedMedia: filterOwnedMedia)
    {
        MediaType = mediaType;
    }

    [QueryParam, BindFrom("mediaType")]
    public PlexMediaType MediaType { get; init; }
}

public class GetAllMediaByTypeRequestValidator : Validator<GetAllMediaByTypeRequest>
{
    public GetAllMediaByTypeRequestValidator()
    {
        RuleFor(x => x.MediaType)
            .Must(type => type is PlexMediaType.TvShow or PlexMediaType.Movie)
            .WithMessage(x => $"Media type {x.MediaType} is not allowed.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Size).GreaterThanOrEqualTo(0);
    }
}

public class GetAllMediaByTypeEndpoint : BaseEndpoint<GetAllMediaByTypeRequest, PlexMediaStatisticsDTO>
{
    private readonly IPlexRipperDbContext _dbContext;

    public override string EndpointPath => ApiRoutes.PlexMediaController;

    public GetAllMediaByTypeEndpoint(IPlexRipperDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Get(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<PlexMediaStatisticsDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(GetAllMediaByTypeRequest req, CancellationToken ct)
    {
        // When 0, just take everything
        var take = req.Size <= 0 ? 0 : req.Size;
        var skip = req.Page * req.Size;

        var mediaListResult = await _dbContext.GetMediaByType(
            new MediaQueryFilter
            {
                MediaType = req.MediaType,
                Skip = skip,
                Take = take,
                PlexLibraryId = 0,
                FilterOfflineMedia = req.FilterOfflineMedia,
                FilterOwnedMedia = req.FilterOwnedMedia,
                CountryId = req.CountryId,
                RoleId = req.RoleId,
                GenreId = req.GenreId,
            },
            ct: ct
        );

        if (mediaListResult.IsFailed)
        {
            await SendFluentResult(mediaListResult, ct);
            return;
        }

        await SendFluentResult(Result.Ok(mediaListResult.Value.ToStatisticsDTO()), ct);
    }
}
