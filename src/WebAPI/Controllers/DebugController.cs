using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DebugController : BaseController
{
    private readonly IDownloadProgressScheduler _downloadProgressScheduler;
    private readonly IInspectServerScheduler _inspectServerScheduler;

    public DebugController(
        IDownloadProgressScheduler downloadProgressScheduler,
        IMapper mapper,
        INotificationsService notificationsService,
        IInspectServerScheduler inspectServerScheduler) : base(mapper,
        notificationsService)
    {
        _downloadProgressScheduler = downloadProgressScheduler;
        _inspectServerScheduler = inspectServerScheduler;
    }

    // GET: api/<DebugController>/StartServerDownloadProgress
    [HttpGet("StartServerDownloadProgress")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> StartDownloadProgressJob([FromQuery] int plexServerId)
    {
        var result = await _downloadProgressScheduler.StartDownloadProgressJob(plexServerId);
        return ToActionResult(result);
    }

    // GET: api/<DebugController>/StopServerDownloadProgress
    [HttpGet("StopServerDownloadProgress")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> StopDownloadProgressJob([FromQuery] int plexServerId)
    {
        var result = await _downloadProgressScheduler.StopDownloadProgressJob(plexServerId);
        return ToActionResult(result);
    }

    [HttpGet("InspectServerScheduler/{plexServerId:int}")]
    public async Task<IActionResult> InspectServerSchedulerJob(int plexServerId)
    {
        var result = await _inspectServerScheduler.QueueInspectPlexServerJob(plexServerId);
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