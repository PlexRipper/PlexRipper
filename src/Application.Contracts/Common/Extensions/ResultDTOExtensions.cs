using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Riok.Mapperly.Abstractions;

namespace Application.Contracts;

[Mapper]
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

    public static ResultDTO<TDTO> ToResultDTO<T, TDTO>(this Result<T> result, Func<T, TDTO> mapper) => new()
    {
        Value = mapper(result.Value),
        IsFailed = result.IsFailed,
        IsSuccess = result.IsSuccess,
        Reasons = result.Reasons.ToReasonDTOs(),
        Errors = result.Errors.ToErrorDTOs(),
        Successes = result.Successes.ToSuccessDTOs(),
    };

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial List<ReasonDTO> ToReasonDTOs(this List<IReason> reasons);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial List<ErrorDTO> ToErrorDTOs(this List<IError> reasons);

    [MapperRequiredMapping(RequiredMappingStrategy.Target)]
    private static partial List<SuccessDTO> ToSuccessDTOs(this List<ISuccess> reasons);

    public static IResult ToIResult(this Result result)
    {
        if (result.IsSuccess)
        {
            var resultDTO = result.ToResultDTO();
            if (result.Has201CreatedRequestSuccess())
            {
                // Status code 201 Created
                return TypedResults.Created(resultDTO.ToString(), resultDTO);
            }

            return TypedResults.Ok(resultDTO);
        }

        var failedResult = result.ToResultDTO();
        if (result.Has400BadRequestError())
            return TypedResults.BadRequest(failedResult);

        if (result.Has404NotFoundError())
            return TypedResults.NotFound(failedResult);

        if (result.Has204NoContentRequestSuccess())
            return TypedResults.NoContent();

        // Status Code 500
        return TypedResults.Problem(new ProblemDetails()
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
                return TypedResults.Created("test", resultDTO);
            }

            return TypedResults.Ok(resultDTO);
        }

        // Ensure we first cast to result to avoid result.value being null
        var failedResult = result.ToResult().ToResultDTO();
        if (result.Has400BadRequestError())
            return TypedResults.BadRequest(failedResult);

        if (result.Has404NotFoundError())
            return TypedResults.NotFound(failedResult);

        if (result.Has204NoContentRequestSuccess())
            return TypedResults.NoContent();

        // Status Code 500
        return TypedResults.Problem(new ProblemDetails()
        {
            Detail = failedResult.ToString(),
            Status = StatusCodes.Status500InternalServerError,
        });
    }
}