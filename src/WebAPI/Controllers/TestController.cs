using Application.Contracts;
using AutoMapper;
using Logging.Interface;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.WebAPI.Common.DTO;
using PlexRipper.WebAPI.Common.FluentResult;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PlexRipper.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : BaseController
{
    public TestController(ILog log, IMapper mapper, INotificationsService notificationsService) : base(log, mapper, notificationsService) { }

    // GET: api/<FolderPathController>
    [HttpGet("OkResult")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public IActionResult GetOkResult([FromQuery] bool fail)
    {
        if (!fail)
            return ToActionResult(Result.Ok());
        else
            return ToActionResult(Result.Fail(new Error("Failed reason #1")));
    }

    // GET: api/<FolderPathController>
    [HttpGet("OkResult/int")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResultDTO<int>))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ResultDTO))]
    public IActionResult GetOkResultInt([FromQuery] bool fail)
    {
        if (!fail)
            return ToActionResult<int, int>(Result.Ok(999));
        else
            return ToActionResult(Result.Fail(new Error("Failed reason #1")));
    }
}