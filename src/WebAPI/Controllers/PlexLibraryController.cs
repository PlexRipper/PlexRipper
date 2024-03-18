using Application.Contracts;
using AutoMapper;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Application;

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlexLibraryController : BaseController
{
    private readonly IMediator _mediator;

    public PlexLibraryController(
        ILog log,
        IMediator mediator,
        IMapper mapper,
        INotificationsService notificationsService) : base(log,
        mapper, notificationsService)
    {
        _mediator = mediator;
    }

    // POST api/<PlexLibrary>/settings/default/destination/{id:int}
    [HttpPut("settings/default/destination")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public async Task<IActionResult> UpdateDefaultDestination([FromBody] UpdateDefaultDestinationDTO payload)
    {
        if (payload is null)
            return BadRequest();

        var result = await _mediator.Send(new UpdatePlexLibraryDefaultDestinationByIdCommand(payload.LibraryId, payload.FolderPathId));
        return ToActionResult(result);
    }
}