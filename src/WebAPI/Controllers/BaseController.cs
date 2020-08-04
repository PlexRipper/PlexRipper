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
        public IActionResult BadRequestInvalidId => new BadRequestObjectResult(Result.Fail("The Id was 0 or lower"));

        [NonAction]
        protected IActionResult InternalServerError(Exception e)
        {
            string msg = $"Internal server error: {e.Message}";
            Log.Error(e, msg);
            return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail(msg));
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

            var errorList = new List<Error>
            {
                new Error($"The Id: {id} was 0 or lower")
            };

            if (nameOf != string.Empty)
            {
                errorList[0] = new Error($"The Id parameter \"{nameOf}\" has an invalid id of {id}");
            }

            return new BadRequestObjectResult(errorList);
        }
    }
}
