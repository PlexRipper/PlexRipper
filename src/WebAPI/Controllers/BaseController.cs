using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PlexRipper.Domain;
using System;
using System.Net.Mime;
using AutoMapper;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces(MediaTypeNames.Application.Json)]
    public abstract class BaseController : ControllerBase
    {
        private readonly IMapper _mapper;

        protected BaseController(IMapper mapper)
        {
            _mapper = mapper;
        }

        protected IActionResult BadRequestInvalidId => new BadRequestObjectResult(Result.Fail("The Id was 0 or lower"));

        [NonAction]
        protected IActionResult InternalServerError(Exception e)
        {
            string msg = $"Internal server error: {e.Message}";
            Log.Error(e, msg);
            return StatusCode(StatusCodes.Status500InternalServerError, Result.Fail(msg));
        }

        [NonAction]
        protected IActionResult InternalServerError(Result result)
        {
            Log.Error("Internal server error:");
            return StatusCode(StatusCodes.Status500InternalServerError, result);
        }

        [NonAction]
        protected IActionResult BadRequest(int id, string nameOf = "")
        {

            var error = new Error($"The Id: {id} was 0 or lower");

            if (nameOf != string.Empty)
            {
                error = new Error($"The Id parameter \"{nameOf}\" has an invalid id of {id}");
            }

            return new BadRequestObjectResult(Result.Fail(error));
        }  
        
        [NonAction]
        protected IActionResult BadRequest(Result result)
        {
            // Filter our the value type
            var resultDTO = _mapper.Map<ResultDTO>(result);
            return new BadRequestObjectResult(resultDTO);
        }
        
        
    }
}
