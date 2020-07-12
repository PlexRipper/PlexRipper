using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Domain;
using System;
using System.Collections.Generic;
using System.Net.Mime;

namespace PlexRipper.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public abstract class BaseController : ControllerBase
    {
        public IActionResult BadRequestInvalidId => new BadRequestObjectResult(new Error("The Id was 0 or lower"));

        [NonAction]
        public IActionResult InternalServerError(Exception e)
        {
            string message = $"Internal server error: {e.Message}";
            Log.Error(e, message);
            return StatusCode(StatusCodes.Status500InternalServerError, message);
        }

        [NonAction]
        public IActionResult InternalServerError(List<Error> errors)
        {
            Log.Error("Internal server error:");
            foreach (Error error in errors)
            {
                Log.Error(error.Message);
            }
            return StatusCode(StatusCodes.Status500InternalServerError, errors);
        }

        [NonAction]
        public IActionResult BadRequest(int id, string nameOf = "")
        {
            if (nameOf != string.Empty)
            {
                return new BadRequestObjectResult(new Error($"The Id parameter \"{nameOf}\" has an invalid id of {id}"));
            }

            return new BadRequestObjectResult(new Error($"The Id: {id} was 0 or lower"));
        }
    }
}
