using Application.Contracts;
using FastEndpoints;

namespace PlexRipper.Application;

public record ClearCompletedDownloadTasksRequest(List<Guid> DownloadTaskIds);

public class ClearCompletedDownloadTasksCustomEndpoint : BaseCustomEndpoint<ClearCompletedDownloadTasksRequest, ResultDTO>
{
    private readonly IMediator _mediator;

    public override string EndpointPath => ApiRoutes.DownloadController + "/clear";

    public ClearCompletedDownloadTasksCustomEndpoint(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override void Configure()
    {
        Verbs(Http.POST);
        Post(EndpointPath);
        AllowAnonymous();
    }

    public override async Task HandleAsync(ClearCompletedDownloadTasksRequest req, CancellationToken ct)
    {
        var result = await _mediator.Send(new ClearCompletedDownloadTasksCommand(req.DownloadTaskIds), ct);

        await SendResult(result, ct);
    }
}