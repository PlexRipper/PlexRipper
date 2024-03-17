using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public static partial class ResultDTOExtensions
{
    public static ResultDTO ToResultDTO(this Result result) => new()
    {
        IsFailed = result.IsFailed,
        IsSuccess = result.IsSuccess,
        Reasons = result.Reasons.ToReasonDTOs(),
        Errors = result.Errors.ToErrorDTOs(),
        Successes = result.Successes.ToSuccessDTOs(),
    };

    public static ResultDTO<T> ToResultDTO<T>(this Result<T> result) => new()
    {
        Value = result.Value,
        IsFailed = result.IsFailed,
        IsSuccess = result.IsSuccess,
        Reasons = result.Reasons.ToReasonDTOs(),
        Errors = result.Errors.ToErrorDTOs(),
        Successes = result.Successes.ToSuccessDTOs(),
    };

    private static partial List<ReasonDTO> ToReasonDTOs(this List<IReason> reasons);
    private static partial List<ErrorDTO> ToErrorDTOs(this List<IError> reasons);
    private static partial List<SuccessDTO> ToSuccessDTOs(this List<ISuccess> reasons);

    public static IResult ToIResult(this Result result)
    {
        if (result.IsSuccess)
        {
            var resultDTO = result.ToResultDTO();
            if (result.Has201CreatedRequestSuccess())
            {
                // Status code 201 Created
                return Results.Created(resultDTO.ToString(), resultDTO);
            }

            return Results.Ok(resultDTO);
        }

        var failedResult = result.ToResultDTO();
        if (result.Has400BadRequestError())
            return Results.BadRequest(failedResult);

        if (result.Has404NotFoundError())
            return Results.NotFound(failedResult);

        if (result.Has204NoContentRequestSuccess())
            return Results.NoContent();

        // Status Code 500
        return Results.Problem(new ProblemDetails()
        {
            Detail = failedResult.ToString(),
            Status = StatusCodes.Status500InternalServerError,
        });
    }

    public static IResult ToIResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            var resultDTO = result.ToResultDTO();
            if (result.Has201CreatedRequestSuccess())
            {
                // Status code 201 Created
                return Results.Created(resultDTO.ToString(), resultDTO);
            }

            return Results.Ok(resultDTO);
        }

        // Ensure we first cast to result to avoid result.value being null
        var failedResult = result.ToResult().ToResultDTO();
        if (result.Has400BadRequestError())
            return Results.BadRequest(failedResult);

        if (result.Has404NotFoundError())
            return Results.NotFound(failedResult);

        if (result.Has204NoContentRequestSuccess())
            return Results.NoContent();

        // Status Code 500
        return Results.Problem(new ProblemDetails()
        {
            Detail = failedResult.ToString(),
            Status = StatusCodes.Status500InternalServerError,
        });
    }
}