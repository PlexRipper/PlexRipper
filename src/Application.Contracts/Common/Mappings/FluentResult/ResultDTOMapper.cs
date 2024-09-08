using FluentResults;

namespace Application.Contracts;

public static class ResultDTOMapper
{
    public static ResultDTO ToResultDTO(this Result result) =>
        new()
        {
            IsFailed = result.IsFailed,
            IsSuccess = result.IsSuccess,
            Reasons = result.Reasons.ToReasonDTOs(),
            Errors = result.Errors.ToErrorDTOs(),
            Successes = result.Successes.ToSuccessDTOs(),
        };

    public static ResultDTO<T> ToResultDTO<T>(this Result<T> result) =>
        new()
        {
            Value = result.ValueOrDefault,
            IsFailed = result.IsFailed,
            IsSuccess = result.IsSuccess,
            Reasons = result.Reasons.ToReasonDTOs(),
            Errors = result.Errors.ToErrorDTOs(),
            Successes = result.Successes.ToSuccessDTOs(),
        };

    public static ResultDTO<TDTO> ToResultDTO<T, TDTO>(this Result<T> result, Func<T, TDTO> mapper) =>
        new()
        {
            Value = result.ValueOrDefault != null ? mapper(result.ValueOrDefault) : default,
            IsFailed = result.IsFailed,
            IsSuccess = result.IsSuccess,
            Reasons = result.Reasons.ToReasonDTOs(),
            Errors = result.Errors.ToErrorDTOs(),
            Successes = result.Successes.ToSuccessDTOs(),
        };

    private static List<ReasonDTO> ToReasonDTOs(this List<IReason> reasons) =>
        reasons.ConvertAll(x => new ReasonDTO { Message = x.Message, Metadata = x.Metadata });

    private static List<ErrorDTO> ToErrorDTOs(this List<IError> reasons) =>
        reasons.ConvertAll(x => new ErrorDTO
        {
            Message = x.Message,
            Reasons = x.Reasons,
            Metadata = x.Metadata,
        });

    private static List<SuccessDTO> ToSuccessDTOs(this List<ISuccess> reasons) =>
        reasons.ConvertAll(x => new SuccessDTO { Message = x.Message, Metadata = x.Metadata });
}
