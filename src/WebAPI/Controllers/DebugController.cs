using Application.Contracts;
using AutoMapper;
using BackgroundServices.Contracts;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DebugController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IDownloadTaskScheduler _downloadTaskScheduler;
    private readonly IInspectServerScheduler _inspectServerScheduler;

    public DebugController(
        ILog log,
        IMediator mediator,
        IDownloadTaskScheduler downloadTaskScheduler,
        IMapper mapper,
        INotificationsService notificationsService,
        IInspectServerScheduler inspectServerScheduler) : base(log, mapper,
        notificationsService)
    {
        _mediator = mediator;
        _downloadTaskScheduler = downloadTaskScheduler;
        _inspectServerScheduler = inspectServerScheduler;
    }

    [HttpGet("InspectServerScheduler/{plexServerId:int}")]
    public async Task<IActionResult> InspectServerSchedulerJob(int plexServerId)
    {
        var result = await _mediator.Send(new QueueInspectPlexServerJobCommand(plexServerId));
        return ToActionResult(result);
    }

    [HttpGet("RefreshAccessiblePlexServersJob/{plexAccountId:int}")]
    public async Task<IActionResult> RefreshAccessiblePlexServersJob(int plexAccountId)
    {
        var result = await _inspectServerScheduler.QueueRefreshAccessiblePlexServersJob(plexAccountId);
        return ToActionResult(result);
    }

    [HttpGet("QueueInspectPlexServerByPlexAccountIdJob/{plexAccountId:int}")]
    public async Task<IActionResult> QueueInspectPlexServerByPlexAccountIdJob(int plexAccountId)
    {
        var result = await _inspectServerScheduler.QueueInspectPlexServerByPlexAccountIdJob(plexAccountId);
        return ToActionResult(result);
    }
}