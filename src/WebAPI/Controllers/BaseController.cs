using System.Net.Mime;
using Application.Contracts;
using AutoMapper;
using Logging.Interface;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using PlexRipper.WebAPI.Common.FluentResult;

namespace PlexRipper.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[EnableCors("CORS_Configuration")]
public abstract class BaseController : ControllerBase
{
    protected readonly ILog _log;

    protected readonly IMapper _mapper;

    protected readonly INotificationsService _notificationsService;

    protected BaseController(ILog log, IMapper mapper, INotificationsService notificationsService)
    {
        _log = log;
        _mapper = mapper;
        _notificationsService = notificationsService;
    }

    [NonAction]
    protected IActionResult InternalServerError(Exception e)
    {
        _log.Error(e);
        var resultDTO = _mapper.Map<ResultDTO>(Result.Fail($"Internal server error: {e.Message}"));
        return StatusCode(StatusCodes.Status500InternalServerError, resultDTO);
    }

    [NonAction]
    protected IActionResult InternalServerError(Result result)
    {
        _log.ErrorLine("Internal server error:");
        result.LogError();
        _notificationsService.SendResult(result);
        var resultDTO = _mapper.Map<ResultDTO>(result);
        return StatusCode(StatusCodes.Status500InternalServerError, resultDTO);
    }

    [NonAction]
    protected IActionResult BadRequest(int id, string nameOf = "")
    {
        var error = new Error($"The Id: {id} was 0 or lower");

        if (nameOf != string.Empty)
            error = new Error($"The Id parameter \"{nameOf}\" has an invalid id of {id}");

        return BadRequest(Result.Fail(error));
    }

    [NonAction]
    protected IActionResult BadRequestInvalidId(string parameterName = "Id")
    {
        return BadRequest(Result.Fail($"The {parameterName} was 0 or lower"));
    }

    [NonAction]
    protected IActionResult BadRequest(Result result)
    {
        _notificationsService.SendResult(result);
        var resultDTO = _mapper.Map<ResultDTO>(result);
        return new BadRequestObjectResult(resultDTO);
    }

    [NonAction]
    protected IActionResult Ok<T>(Result<T> result)
    {
        var resultDTO = _mapper.Map<ResultDTO<T>>(result);
        return new OkObjectResult(resultDTO);
    }

    [NonAction]
    protected IActionResult ToActionResult(Result result)
    {
        var resultDTO = _mapper.Map<ResultDTO>(result);
        if (result.HasStatusCode())
        {
            return new ObjectResult(resultDTO)
            {
                StatusCode = result.FindStatusCode(),
            };
        }

        if (result.IsSuccess)
            return new OkObjectResult(resultDTO);

        // No Status Code found
        _log.Warning("Invalid ResultDTO had no status code assigned, defaulting to 500 error: {@ResultDto}", resultDTO);
        return new ObjectResult(resultDTO)
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
    }

    [NonAction]
    protected IActionResult ToActionResult<TEntity, TDTO>(Result<TEntity> result)
    {
        // Status Code 200
        if (result.IsSuccess)
        {
            var resultDTO = _mapper.Map<ResultDTO<TDTO>>(result);
            if (result.Has201CreatedRequestSuccess())
            {
                // Status code 201 Created
                return new ObjectResult(resultDTO)
                {
                    StatusCode = StatusCodes.Status201Created,
                };
            }

            return new OkObjectResult(resultDTO);
        }

        var failedResult = _mapper.Map<ResultDTO>(result.ToResult());
        if (result.Has400BadRequestError())
            return new BadRequestObjectResult(failedResult);

        if (result.Has404NotFoundError())
            return new NotFoundObjectResult(failedResult);

        // Status Code 500
        return new ObjectResult(failedResult)
        {
            StatusCode = StatusCodes.Status500InternalServerError,
        };
    }

    protected IActionResult ToResult(ModelStateDictionary dictionary)
    {
        var result = Result.Fail("Bad request error:");

        foreach (var keyValuePair in dictionary)
            result = result.WithError(new Error($"{keyValuePair.Key} - {keyValuePair.Value.RawValue}"));

        var resultDTO = _mapper.Map<ResultDTO>(result);
        return new BadRequestObjectResult(resultDTO);
    }
}