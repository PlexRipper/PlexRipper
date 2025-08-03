using FluentResults;

namespace Application.Contracts;

public static class ResultDTOMapper
{
    public static BaseResultDTO ToResultDTO(this Result result) =>
        new()
        {
            IsSuccess = result.IsSuccess,
            Errors = result.Errors.ToErrorDTOs(),
            Successes = result.Successes.ToSuccessDTOs(),
            StatusCode = 0,
        };

    public static ResultDTO<T> ToResultDTO<T>(this Result<T> result) =>
        new()
        {
            Value = result.ValueOrDefault,
            IsSuccess = result.IsSuccess,
            Errors = result.Errors.ToErrorDTOs(),
            Successes = result.Successes.ToSuccessDTOs(),
            StatusCode = 0,
        };

    public static ResultDTO<TDTO> ToResultDTO<T, TDTO>(this Result<T> result, Func<T, TDTO> mapper) =>
        new()
        {
            Value = result.ValueOrDefault != null ? mapper(result.ValueOrDefault) : default,
            IsSuccess = result.IsSuccess,
            Errors = result.Errors.ToErrorDTOs(),
            Successes = result.Successes.ToSuccessDTOs(),
            StatusCode = 0,
        };

    private static IReadOnlyList<ErrorDTO> ToErrorDTOs(this IReadOnlyList<IError> reasons) =>
        reasons
            .Select(x => new ErrorDTO
            {
                Message = x.Message,
                Reasons = x.Reasons,
                Metadata = x.Metadata,
            })
            .ToList();

    private static IReadOnlyList<SuccessDTO> ToSuccessDTOs(this IReadOnlyList<ISuccess> reasons) =>
        reasons.Select(x => new SuccessDTO { Message = x.Message, Metadata = x.Metadata }).ToList();
}
