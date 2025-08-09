using Application.Contracts;
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

public class GetDownloadPreviewEndpoint : BaseEndpoint<List<DownloadMediaDTO>, DownloadPreviewContainerDTO>
{
    private readonly ICommandExecutor _commandExecutor;

    public override string EndpointPath => ApiRoutes.DownloadController + "/preview";

    public GetDownloadPreviewEndpoint(ICommandExecutor commandExecutor)
    {
        _commandExecutor = commandExecutor;
    }

    public override void Configure()
    {
        Post(EndpointPath);

        Description(x =>
            x.Produces(StatusCodes.Status200OK, typeof(ResultDTO<DownloadPreviewContainerDTO>))
                .Produces(StatusCodes.Status400BadRequest, typeof(BaseResultDTO))
                .Produces(StatusCodes.Status500InternalServerError, typeof(BaseResultDTO))
        );
    }

    public override async Task HandleAsync(List<DownloadMediaDTO> downloadMedias, CancellationToken ct)
    {
        var result = await _commandExecutor.Send(new GetDownloadPreviewQuery(downloadMedias), ct);

        await SendFluentResult(result, x => x.ToDTO(), ct);
    }
}
