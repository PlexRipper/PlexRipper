using Application.Contracts;
using Data.Contracts;
using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace PlexRipper.Application;

public class GetDownloadPreviewEndpointRequestValidator : Validator<List<DownloadMediaDTO>>
{
    public GetDownloadPreviewEndpointRequestValidator()
    {
        RuleFor(x => x).NotEmpty();
    }
}

public class GetDownloadPreviewEndpoint : BaseEndpoint<List<DownloadMediaDTO>, List<DownloadPreviewDTO>>
{
    private readonly IPlexRipperDbContext _dbContext;
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.DownloadController + "/preview";

    public GetDownloadPreviewEndpoint(IPlexRipperDbContext dbContext, IMediator mediator)
    {
        _dbContext = dbContext;
        _mediator = mediator;
    }

    public override void Configure()
    {
        Post(EndpointPath);
        AllowAnonymous();
        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<List<DownloadPreviewDTO>>))
                .Produces(StatusCodes.Status400BadRequest, typeof(ResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(ResultDTO))
        );
    }

    public override async Task HandleAsync(List<DownloadMediaDTO> downloadMedias, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetDownloadPreviewQuery(downloadMedias), ct);

        await SendFluentResult(result, x => x.ToDTO(), ct);
    }
}
