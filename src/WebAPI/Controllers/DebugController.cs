using Application.Contracts;
using AutoMapper;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DebugController : BaseController
{
    private readonly IMediator _mediator;

    public DebugController(
        ILog log,
        IMediator mediator,
        IMapper mapper,
        INotificationsService notificationsService) : base(log, mapper,
        notificationsService)
    {
        _mediator = mediator;
    }

    [HttpGet("InspectServerScheduler/{plexServerId:int}")]
    public async Task<IActionResult> InspectServerSchedulerJob(int plexServerId)
    {
        var result = await _mediator.Send(new QueueInspectPlexServerJobCommand(plexServerId));
        return ToActionResult(result);
    }

    [HttpGet("QueueInspectPlexServerByPlexAccountIdJob/{plexAccountId:int}")]
    public async Task<IActionResult> QueueInspectPlexServerByPlexAccountIdJob(int plexAccountId)
    {
        var result = await _mediator.Send(new InspectAllPlexServersByAccountIdCommand(plexAccountId));
        return ToActionResult(result);
    }
}