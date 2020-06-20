using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System;
using System.Net.Mime;

namespace PlexRipper.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public class BaseController : ControllerBase
    {
        public virtual ILogger Log { get; }

        [NonAction]
        public IActionResult InternalServerError(Exception e)
        {
            string message = $"Internal server error: {e.Message}";
            Log.Error(e, message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }
    }
}
