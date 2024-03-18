using Application.Contracts;
using AutoMapper;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;
using PlexRipper.WebAPI.SignalR.Common;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : BaseController
{
    private readonly IMediator _mediator;

    public NotificationController(ILog log, INotificationsService notificationsService, IMapper mapper, IMediator mediator) : base(log, mapper,
        notificationsService)
    {
        _mediator = mediator;
    }

    // PUT api/<NotificationController>/5
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> HideNotification(int id)
    {
        if (id <= 0)
            return BadRequestInvalidId();

        var result = await _mediator.Send(new HideNotificationCommand(id));
        return ToActionResult<List<Notification>, List<NotificationDTO>>(result);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<int>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> CreateNotification([FromBody] NotificationDTO notificationDto)
    {
        var notification = _mapper.Map<Notification>(notificationDto);

        var result = await _mediator.Send(new CreateNotificationCommand(notification));
        return ToActionResult<int, int>(result);
    }

    /// <summary>
    /// Deletes/Clears all <see cref="Notification">Notifications</see>.
    /// </summary>
    /// <returns>Returns the number of <see cref="Notification">Notifications</see> that have been deleted.</returns>
    [HttpPost("clear")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<int>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> ClearAllNotifications()
    {
        var result = await _mediator.Send(new ClearAllNotificationsCommand());
        return ToActionResult<int, int>(result);
    }
}